#region Copyright (c) 2011 Two10 degrees
//
// (C) Copyright 2011 Two10 degrees
//      All rights reserved.
//
// This software is provided "as is" without warranty of any kind,
// express or implied, including but not limited to warranties as to
// quality and fitness for a particular purpose. Active Web Solutions Ltd
// does not support the Software, nor does it warrant that the Software
// will meet your requirements or that the operation of the Software will
// be uninterrupted or error free or that any defects will be
// corrected. Nothing in this statement is intended to limit or exclude
// any liability for personal injury or death caused by the negligence of
// Active Web Solutions Ltd, its employees, contractors or agents.
//
#endregion

namespace ByteStorm.PassthroughDrive
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Delimon.Win32.IO;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Dokan;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using ByteStorm.PassthroughDrive;
    using System.Security.Cryptography;
    using CSharpTest.Net.Collections;
    using CSharpTest.Net.Serialization;
    using System.Text;

    public class CryptViewOperations : DokanOperations
    {

        internal const int ERROR_SUCCESS = 0x0;
        internal const int ERROR_INVALID_FUNCTION = 0x1;
        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_PATH_NOT_FOUND = 0x3;
        internal const int ERROR_ACCESS_DENIED = 0x5;
        internal const int ERROR_INVALID_HANDLE = 0x6;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
        internal const int ERROR_INVALID_DATA = 0xd;
        internal const int ERROR_INVALID_DRIVE = 0xf;
        internal const int ERROR_NO_MORE_FILES = 0x12;
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_NOT_SUPPORTED = 0x32;
        internal const int ERROR_FILE_EXISTS = 0x50;
        internal const int ERROR_INVALID_PARAMETER = 0x57;

        internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        internal const string UNICODE_PREFIX = "\\\\?\\";

        private string root;// = "C:\\Livedrive";
        private readonly AesCryptoServiceProvider cbcCryptoProvider;
        private readonly AesCryptoServiceProvider cfbCryptoProvider;
        private readonly PathTranslator pathTrans;
        private bool isShutdown;

        public CryptViewOperations(string mountPath, string dbpath, byte[] key, byte[] iv)
        {
            root = mountPath;
            cbcCryptoProvider = new AesCryptoServiceProvider();
            //cryptoProvider.BlockSize = 128;
            //cryptoProvider.KeySize = 128;
            //cryptoProvider.FeedbackSize = 8;
            cbcCryptoProvider.IV = iv;
            cbcCryptoProvider.Key = key;
            cbcCryptoProvider.Mode = CipherMode.CBC;
            cbcCryptoProvider.Padding = PaddingMode.Zeros;
            cfbCryptoProvider = new AesCryptoServiceProvider();
            cfbCryptoProvider.IV = iv;
            cfbCryptoProvider.Key = key;
            cfbCryptoProvider.Mode = CipherMode.CFB;
            cfbCryptoProvider.Padding = PaddingMode.Zeros;

            pathTrans = new PathTranslator(dbpath);
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            RandomAccessEncryptedFile file = getFile(info);
            if (file != null)
                file.handle.Close();
            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            if (!Win32Api.CreateDirectory(root+filename, IntPtr.Zero))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public int CreateFile(
            string filename,
            System.IO.FileAccess access,
            System.IO.FileShare share,
            System.IO.FileMode mode,
            uint flagsAndOptions,
            DokanFileInfo info)
        {
            string extension = GetStrippedFileExtension(filename);
            filename = translateToPathOfNames(filename);
            //Trace.WriteLine(string.Format("CreateFile {0}", filename));
            string path = root + filename;
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
                flagsAndOptions |= FILE_FLAG_BACKUP_SEMANTICS;

            SafeFileHandle handle = Win32Api.CreateFile(UNICODE_PREFIX+path, access, share, IntPtr.Zero, mode, flagsAndOptions, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                return -Marshal.GetLastWin32Error();
            } else {
                long offset = 0;
                long filesize = long.MaxValue;
                if (!String.IsNullOrEmpty(extension))
                {
                    int partNum = Int32.Parse(extension);
                    filesize = new FileInfo(path).Length;
                    offset = (long)partNum * (long)CryptoConstants.MAX_FILE_SIZE;
                    filesize -= offset;
                    if (filesize > CryptoConstants.MAX_FILE_SIZE)
                        filesize = CryptoConstants.MAX_FILE_SIZE;
                }
                RandomAccessEncryptedFile raef = new RandomAccessEncryptedFile(handle, offset, filesize, cbcCryptoProvider, cfbCryptoProvider);
                info.Context = raef;
            }
            return 0;
        }

        private string GetStrippedFileExtension(string filename)
        {
            filename = Path.GetExtension(filename);
            if (!String.IsNullOrEmpty(filename))
            {
                char[] arr = filename.ToCharArray();
                arr = Array.FindAll<char>(arr, (c => (char.IsDigit(c))));
                return new string(arr);
            }
            return null;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            if (!Win32Api.RemoveDirectory(root + filename))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            if (!Win32Api.DeleteFile(root + filename))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }


        public int FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            if (!Win32Api.FlushFileBuffers(getFile(info).handle))
            {
                return -Marshal.GetLastWin32Error();
            }

            return -1;
        }

        public int FindFiles(
            string filename,
            System.Collections.ArrayList files,
            DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            //Trace.WriteLine(string.Format("FindFiles {0}", root+filename));

            Random rng = new Random();
            DirectoryInfo di = new DirectoryInfo(root+filename);
            try
            {
                foreach (DirectoryInfo f in di.GetDirectories())
                {
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.FileName = translateToPathOfIds(f.Name, rng);
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = 0;
                    files.Add(fi);
                }
                foreach (FileInfo f in di.GetFiles())
                {
                    if (f.Length > CryptoConstants.MAX_FILE_SIZE)
                    {
                        long remLen = f.Length;
                        int partCount = 0;
                        string baseFilename = translateToPathOfIds(f.Name, rng);
                        while (remLen > CryptoConstants.MAX_FILE_SIZE)
                        {
                            FileInformation fi = new FileInformation();
                            fi.Attributes = f.Attributes;
                            fi.CreationTime = f.CreationTime;
                            fi.FileName = filename = baseFilename + "." + String.Format("{0:000}", partCount); ;
                            fi.LastAccessTime = f.LastAccessTime;
                            fi.LastWriteTime = f.LastWriteTime;
                            fi.Length = CryptoConstants.MAX_FILE_SIZE;
                            files.Add(fi);
                            partCount++;
                            remLen -= CryptoConstants.MAX_FILE_SIZE;
                        }
                        if (remLen > 0)
                        {
                            FileInformation fi = new FileInformation();
                            fi.Attributes = f.Attributes;
                            fi.CreationTime = f.CreationTime;
                            fi.FileName = filename = baseFilename + "." + String.Format("{0:000}", partCount); ;
                            fi.LastAccessTime = f.LastAccessTime;
                            fi.LastWriteTime = f.LastWriteTime;
                            fi.Length = remLen;
                            files.Add(fi);
                        }
                    }
                    else
                    {
                        FileInformation fi = new FileInformation();
                        fi.Attributes = f.Attributes;
                        fi.CreationTime = f.CreationTime;
                        fi.FileName = filename = translateToPathOfIds(f.Name, rng);
                        fi.LastAccessTime = f.LastAccessTime;
                        fi.LastWriteTime = f.LastWriteTime;
                        fi.Length = f.Length;
                        files.Add(fi);
                    }
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Console.WriteLine("Directory {0} could not be accessed!", di.FullName);
                return -ERROR_PATH_NOT_FOUND;
            }
            catch
            {
                Console.WriteLine("Directory {0} could not be accessed!", di.FullName);
                return -ERROR_ACCESS_DENIED;
            }
            return 0;

        }

        public int GetFileInformation(
            string filename,
            ref BY_HANDLE_FILE_INFORMATION fileinfo,
            DokanFileInfo info)
        {
            string extension = GetStrippedFileExtension(filename);
            filename = translateToPathOfNames(filename);
            //Trace.WriteLine(string.Format("GetFileInformation {0}", root+filename));

            RandomAccessEncryptedFile file = getFile(info);
            if (!Win32Api.GetFileInformationByHandle(file.handle, out fileinfo))
            {
                return -Marshal.GetLastWin32Error();
            }

            if (file.eof < long.MaxValue)
            {
                long partLen = file.eof - file.bof;
                fileinfo.nFileSizeHigh = (uint)(partLen >> 32);
                fileinfo.nFileSizeLow = (uint)(partLen & 0xffffffff);
            }
            return 0;
        }

        public int LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            RandomAccessEncryptedFile file = getFile(info);
            offset += file.bof;
            uint offsetHi = (uint)(offset >> 32);
            uint offsetLo = (uint)(offset & 0xffffffff);
            uint lengthHi = (uint)(length >> 32);
            uint lengthLo = (uint)(length & 0xffffffff);
            if (!Win32Api.LockFile(file.handle, offsetLo, offsetHi, lengthLo, lengthHi))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public int MoveFile(
            string filename,
            string newname,
            bool replace,
            DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            newname = translateToPathOfNames(newname);
            //Trace.WriteLine(string.Format("MoveFile {0}", root+filename));

            if (!Win32Api.MoveFile(root+filename, root+newname))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;

        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            DirectoryInfo di = new DirectoryInfo(root + filename);
            if (!di.Exists)
            {
                return -ERROR_FILE_NOT_FOUND;
            }
            return 0;
        }


        public int ReadFile(
            string filename,
            byte[] buffer,
            ref uint readBytes,
            long offset,
            DokanFileInfo info)
        {
            //Trace.WriteLine(string.Format("ReadFile {0} offset {1} length {2}", root+filename, offset, buffer.Length));

            RandomAccessEncryptedFile file = getFile(info);
            int bytesRead = file.read(buffer, offset, buffer.Length);
            if (bytesRead < 0)
            {
                //Trace.WriteLine(string.Format("Error: ReadFile {0} offset {1} length {2}", root + filename, offset, buffer.Length));
                return -Marshal.GetLastWin32Error();
            }

            //Trace.WriteLine(string.Format("ReadFile {0} offset {1} length {2} actually read {3}", root + filename, offset, buffer.Length, bytesRead));
            readBytes = (uint)bytesRead;
            return 0;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            RandomAccessEncryptedFile file = getFile(info);
            length += file.bof;
            SafeFileHandle handle = file.handle;
            setFilePointer(handle, length);
            if (!(Win32Api.SetEndOfFile(handle)))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int SetFileAttributes(
            string filename,
            System.IO.FileAttributes attr,
            DokanFileInfo info)
        {
            filename = translateToPathOfNames(filename);
            //Trace.WriteLine(string.Format("SetFileAttributes {0}", root+filename));

            if (!(Win32Api.SetFileAttributes(root+filename, attr)))
            {
                return -Marshal.GetLastWin32Error();
            }

            return -1;
        }

        public int SetFileTime(
            string filename,
            DateTime ctime,
            DateTime atime,
            DateTime mtime,
            DokanFileInfo info)
        {
            //Trace.WriteLine(string.Format("SetFileTime {0}", root+filename));

            long lCreationTime = ctime.ToFileTime();
            long lAccessTime = atime.ToFileTime();
            long lModifiedTime = mtime.ToFileTime();

            if (!(Win32Api.SetFileTime(getFile(info).handle, ref lCreationTime, ref lAccessTime, ref lModifiedTime)))
            {
                return -Marshal.GetLastWin32Error();
            }
            return -1;
        }



        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            RandomAccessEncryptedFile file = getFile(info);
            offset += file.bof;
            uint offsetHi = (uint)(offset >> 32);
            uint offsetLo = (uint)(offset & 0xffffffff);
            uint lengthHi = (uint)(length >> 32);
            uint lengthLo = (uint)(length & 0xffffffff);
            if (!Win32Api.UnlockFile(file.handle, offsetLo, offsetHi, lengthLo, lengthHi))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {
            //Trace.WriteLine("Unmount");
            shutdown();
            return 0;
        }

        public int GetDiskFreeSpace(
           ref ulong freeBytesAvailable,
           ref ulong totalBytes,
           ref ulong totalFreeBytes,
           DokanFileInfo info)
        {
           uint lpSectorsPerCluster;
           uint lpBytesPerSector;
           uint lpNumberOfFreeClusters;
           uint lpTotalNumberOfClusters;

           if (!Win32Api.GetDiskFreeSpace(root, out lpSectorsPerCluster, out lpBytesPerSector, out lpNumberOfFreeClusters, out lpTotalNumberOfClusters))
            {
                return -Marshal.GetLastWin32Error();
            }

           freeBytesAvailable = (ulong)lpNumberOfFreeClusters * lpSectorsPerCluster * lpBytesPerSector;
           totalBytes = (ulong)lpTotalNumberOfClusters * lpSectorsPerCluster * lpBytesPerSector;
           totalFreeBytes = (ulong)lpNumberOfFreeClusters * lpSectorsPerCluster * lpBytesPerSector;

           return 0;
        }

        public int WriteFile(
            string filename,
            byte[] buffer,
            ref uint writtenBytes,
            long offset,
            DokanFileInfo info)
        {
            //Trace.WriteLine(string.Format("WriteFile {0}", root+filename));

            SafeFileHandle handle = getFile(info).handle;
            setFilePointer(handle, offset);
            if (!(Win32Api.WriteFile(handle, buffer, buffer.Length, out writtenBytes, IntPtr.Zero)))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;
        }

        public void shutdown()
        {
            if (isShutdown) return;
            isShutdown = true;
            if (pathTrans != null)
            {
                pathTrans.commit();
                pathTrans.close();
            }
        }

        public static bool IsContainerNameValid(string containerName)
        {
            return (Regex.IsMatch(containerName, @"(^([a-z]|\d))((-([a-z]|\d)|([a-z]|\d))+)$") && (3 <= containerName.Length) && (containerName.Length <= 63));
        }

        private bool setFilePointer(SafeFileHandle handle, long position)
        {
            return Win32Api.SetFilePointerEx(handle, position, out position, ByteStorm.PassthroughDrive.Win32Api.EMoveMethod.Begin);
        }

        private RandomAccessEncryptedFile getFile(DokanFileInfo info)
        {
            return (RandomAccessEncryptedFile)(info.Context);
        }

        private static readonly string separatorStr = new string(System.IO.Path.DirectorySeparatorChar, 1);
        private static readonly char[] separatorChr = new char[] { System.IO.Path.DirectorySeparatorChar };

        private string translateToPathOfIds(string plaintextPath, Random rng)
        {
            bool startsWithBackslash = plaintextPath.StartsWith(separatorStr);
            bool endsWithBackslash = plaintextPath.EndsWith(separatorStr);

            string[] pathSegments = plaintextPath.Split(separatorChr, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathSegments.Length; i++)
            {
                string translatedSegment = pathTrans.getOrCreateIdFor(pathSegments[i], rng);
                if (translatedSegment != null)
                    pathSegments[i] = translatedSegment;
            }

            return buildPath(pathSegments, startsWithBackslash, endsWithBackslash);
        }

        private string translateToPathOfNames(string ciphertextPath)
        {
            bool startsWithBackslash = ciphertextPath.StartsWith(separatorStr);
            bool endsWithBackslash = ciphertextPath.Length > 2 && ciphertextPath.EndsWith(separatorStr);
            if (!endsWithBackslash && ciphertextPath.Length > 2) // strip off extension if the path looks like it could contain a filename
                try
                {
                    string cp = ciphertextPath;
                    ciphertextPath = Path.GetDirectoryName(cp) + separatorStr + StripExtension(Path.GetFileName(cp));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(ciphertextPath);
                    Trace.WriteLine(e.Message);
                    Trace.WriteLine(e.StackTrace);
                }

            string[] pathSegments = ciphertextPath.Split(separatorChr, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathSegments.Length; i++)
            {
                string translatedSegment = pathTrans.getNameFor(pathSegments[i]);
                if (translatedSegment != null)
                    pathSegments[i] = translatedSegment;
            }

            return buildPath(pathSegments, startsWithBackslash, endsWithBackslash);
        }

        private string StripExtension(string s)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;
            int dotIndex = s.LastIndexOf(".");
            if (dotIndex < 0)
                return s;
            return s.Substring(0, dotIndex);
        }

        private string buildPath(string[] pathSegments, bool startsWithBackslash, bool endsWithBackslash)
        {
            StringBuilder sb = new StringBuilder();
            if (startsWithBackslash)
                sb.Append(separatorStr);
            if (pathSegments.Length > 0)
            {
                sb.Append(pathSegments[0]);
                for (int i = 1; i < pathSegments.Length; i++)
                {
                    sb.Append(separatorStr);
                    sb.Append(pathSegments[i]);
                }
            }
            if (endsWithBackslash)
                sb.Append(separatorStr);

            return sb.ToString();
        }

    }

}
