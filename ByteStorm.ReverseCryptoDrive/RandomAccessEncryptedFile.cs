using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ByteStorm.PassthroughDrive;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Concurrent;

namespace ByteStorm.PassthroughDrive
{
    public class RandomAccessEncryptedFile
    {
        public readonly SafeFileHandle handle;
        private BlockBasedRandomAccessFile braf;
        //private BlockLock[] blockLocks = new BlockLock[CryptoConstants.NUM_LOCKS];
        //private BlockCrypto crypto;
        //private long lastBlockOffset;
        private Object lastBlockLock = new Object();
        private long filepointer = 0;
        private AesCryptoServiceProvider cbcCryptoProvider;
        private AesCryptoServiceProvider cfbCryptoProvider;
        public readonly long bof; // beginning of file offset
        public readonly long eof; // end of file offset (exclusive)

        /*public RandomAccessEncryptedFile(SafeFileHandle handle, long fileLength, byte[] keyBytes, byte[] ivBytes)
        {
            this.handle = handle;
            this.braf = new BlockBasedRandomAccessFile(new RandomAccessFile(handle));
            this.crypto = new BlockCrypto(keyBytes, ivBytes);
            for (int i = 0; i < blockLocks.Length; i++)
            {
                blockLocks[i] = new BlockLock();
            }
            lastBlockOffset = fileLength & ~CryptoConstants.BLOCK_SIZE_MASK;
        }*/

        public RandomAccessEncryptedFile(SafeFileHandle handle, long offset, long length, AesCryptoServiceProvider cbcCryptoProvider, AesCryptoServiceProvider cfbCryptoProvider)
        {
            this.handle = handle;
            this.braf = new BlockBasedRandomAccessFile(new RandomAccessFile(handle));
            this.cbcCryptoProvider = cbcCryptoProvider;
            this.cfbCryptoProvider = cfbCryptoProvider;
            this.bof = offset;
            this.eof = offset+length;
        }

        public int read(byte[] buffer, long offset, int len)
        {
            // adjust offset by part offset
            offset += bof;
            // is the offset beyond the end of the file part
            if (offset >= eof)
                return -1;
            // did the caller request bytes beyond the end of the file part?
            if (offset+len > eof)
                len = (int)(eof - offset);

            if (len == 0)
                return 0;

            int blockOffset = (int)(offset & CryptoConstants.BLOCK_SIZE_MASK);
            long fileOffset = offset & ~((long)CryptoConstants.BLOCK_SIZE_MASK);
            int read = 0;
            List<ManualResetEvent> resultQueue = new List<ManualResetEvent>();

            //int i = len + blockOffset;
            while (len > 0)
            {
                ByteBuffer block = readBlock(fileOffset);
                if (block == null)
                    break;
                block.Position(blockOffset);
                int copyLen = Math.Min(len, Math.Min(CryptoConstants.BLOCK_SIZE - blockOffset, block.Remaining()));
                ManualResetEvent future = asyncEncryptAndCopy(block, buffer, read, copyLen);
                resultQueue.Add(future);
                //block.ReadBytes(buffer, read, copyLen);
                len -= copyLen;
                read += copyLen;
                blockOffset = 0;
                fileOffset += CryptoConstants.BLOCK_SIZE;
            }

            foreach (ManualResetEvent future in resultQueue)
            {
                future.WaitOne();
            }

            /*ByteBuffer block;
            while (len > 0) {
                readAhead(queue, fileOffset, len);
                Future<ByteBuffer> future = queue.removeFirst();
                try {
                    block = future.get();
                } catch (InterruptedException | ExecutionException e) {
                    throw new IOException(e);
                }
                if (block == null)
                    break;

                // copy the desired part of the block into the supplied buffer
                block.position(blockOffset);
                int copyLen = Math.min(len, Math.min(CryptoConstants.BLOCK_SIZE-blockOffset, block.remaining()));
                block.get(buffer, read, copyLen);
                len -= copyLen;
                read += copyLen;
                blockOffset = 0;
                fileOffset += CryptoConstants.BLOCK_SIZE;
            } */

            if (read > 0)
                return read;

            return -1;
        }

        private ManualResetEvent asyncEncryptAndCopy(ByteBuffer block, byte[] buffer, int offset, int length)
        {
            AsyncEncryptAndCopy crypt = new AsyncEncryptAndCopy(block, buffer, offset, length, cbcCryptoProvider, cfbCryptoProvider);
            ThreadPool.QueueUserWorkItem(crypt.ThreadPoolCallback);
            return crypt.future;
        }

        private ByteBuffer readBlock(long fileOffset)
        {
            byte[] bytes = new byte[CryptoConstants.BLOCK_SIZE];
            ByteBuffer buffer = new ByteBuffer(bytes);
            uint actuallyRead;
            setFilePointerIfNecessary(fileOffset);
            if (Win32Api.ReadFile(handle, bytes, bytes.Length, out actuallyRead, IntPtr.Zero) && actuallyRead > 0)
            {
                buffer.Position((int)actuallyRead);
                if (buffer.Remaining() > 0)
                {
                    bytes = new byte[buffer.Remaining()];
                    while (buffer.Remaining() > 0 && Win32Api.ReadFile(handle, bytes, bytes.Length, out actuallyRead, IntPtr.Zero) && actuallyRead > 0)
                    {
                        buffer.Position(buffer.Position() + (int)actuallyRead);
                    }
                }
            }
            else
            {
                return null;
            }

            buffer.Flip();
            filepointer += buffer.Limit();
            return buffer;
        }

        private void setFilePointerIfNecessary(long fileOffset)
        {
            if (filepointer != fileOffset)
            {
                filepointer = fileOffset;
                Win32Api.SetFilePointerEx(handle, fileOffset, out fileOffset, Win32Api.EMoveMethod.Begin);
            }
        }

    }

    class AsyncEncryptAndCopy
    {
        private ByteBuffer block;
        private byte[] buffer;
        private int offset;
        private int length;
        public readonly ManualResetEvent future;
        public bool usedCfb;
        private AesCryptoServiceProvider cbcCryptoProvider;
        private AesCryptoServiceProvider cfbCryptoProvider;

        private static readonly ConcurrentQueue<ICryptoTransform> cbcEncrypterPool = new ConcurrentQueue<ICryptoTransform>();
        private static readonly ConcurrentQueue<ICryptoTransform> cfbEncrypterPool = new ConcurrentQueue<ICryptoTransform>();

        public AsyncEncryptAndCopy(ByteBuffer block, byte[] buffer, int offset, int length, AesCryptoServiceProvider cbcCryptoProvider, AesCryptoServiceProvider cfbCryptoProvider)
        {
            this.block = block;
            this.buffer = buffer;
            this.offset = offset;
            this.length = length;
            this.future = new ManualResetEvent(false);
            this.cbcCryptoProvider = cbcCryptoProvider;
            this.cfbCryptoProvider = cfbCryptoProvider;
        }

        private ICryptoTransform createCBCEncryptor()
        {
            //Trace.WriteLine(cbcCryptoProvider.Mode);
            return cbcCryptoProvider.CreateEncryptor();
        }

        private ICryptoTransform createCFBEncryptor()
        {
            return cfbCryptoProvider.CreateEncryptor();
        }

        private ICryptoTransform getCBCEncryptor()
        {
            ICryptoTransform enc;
            if (cbcEncrypterPool.TryDequeue(out enc))
            {
                return enc;
            }
            else
            {
                return createCBCEncryptor();
            }
        }

        private ICryptoTransform getCFBEncryptor()
        {
            ICryptoTransform enc;
            if (cfbEncrypterPool.TryDequeue(out enc))
            {
                return enc;
            }
            else
            {
                return createCFBEncryptor();
            }
        }

        public void encryptAndCopy()
        {
            /*IBlockCipher coder = new AesFastEngine();
            coder.Init(true, new KeyParameter(new byte[16]));
            CfbBlockCipher cfbCipher = new CfbBlockCipher(coder, 8);
            IStreamCipher streamCipher = new StreamBlockCipher(cfbCipher);
            streamCipher.Init(true, new ParametersWithIV(new KeyParameter(new byte[16]), new byte[16]));
            streamCipher.ProcessBytes(block.getArray(), block.Position(), block.Remaining(), block.getArray(), block.Position());*/

            // encrypt full blocks with CBC and partial blocks with CFB mode
            // to allow blocks that are not a multiple of the cipher block size
            byte[] blockArray = block.getArray();
            if (block.Remaining() < CryptoConstants.BLOCK_SIZE)
            {
                ICryptoTransform crypto = getCFBEncryptor();
                encrypt(crypto);
                //crypto.TransformFinalBlock(blockArray, 0, 0);
                //cfbEncrypterPool.Enqueue(crypto);
                usedCfb = true;
            }
            else
            {
                ICryptoTransform crypto = getCBCEncryptor();
                //Trace.WriteLine("Cbc reuse " + crypto.CanReuseTransform);
                encrypt(crypto);
                //crypto.TransformFinalBlock(blockArray, 0, 0);
                //cbcEncrypterPool.Enqueue(crypto);
                usedCfb = false;
            }


            /*try
            {
                ICryptoTransform crypto = getEncryptor();

                byte[] blockArray = block.getArray();
                int len = block.Remaining();
                for (int i = block.Position(); i < len; i += crypto.InputBlockSize)
                {
                    crypto.TransformBlock(blockArray, i, crypto.InputBlockSize, blockArray, i);
                }
                crypto.TransformFinalBlock(blockArray, 0, 0);
                encqueue.Enqueue(crypto);
            }
            catch (CryptographicException e)
            {
                Trace.WriteLine("Crypto failed: "+e.ToString());
            }*/
            block.ReadBytes(buffer, offset, length);
        }

        private void encrypt(ICryptoTransform crypto)
        {
            byte[] blockArray = block.getArray();
            using (MemoryStream fsCrypt = new MemoryStream(blockArray))
            {
                using (CryptoStream cs = new CryptoStream(fsCrypt, crypto, CryptoStreamMode.Write))
                {
                    cs.Write(blockArray, 0, blockArray.Length);
                    //cs.FlushFinalBlock();
                }
            }
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            encryptAndCopy();

            // mark the job as done, allowing the calling thread to continue
            future.Set();

            // generate and add a new encrypter to the pool
            // this is an optimization to speed up future encryption jobs
            if (usedCfb)
            {
                if (cfbEncrypterPool.Count < CryptoConstants.MAX_CRYPTER_POOL_SIZE)
                    cfbEncrypterPool.Enqueue(createCFBEncryptor());
            }
            else
            {
                if (cbcEncrypterPool.Count < CryptoConstants.MAX_CRYPTER_POOL_SIZE)
                    cbcEncrypterPool.Enqueue(createCBCEncryptor());
            }
        }
    }

}
