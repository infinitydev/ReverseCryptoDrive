using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    public class CryptoConstants
    {
        public static readonly int BLOCK_SIZE = 4096; // must be a power of 2!
        public static readonly int BLOCK_SIZE_MASK = BLOCK_SIZE - 1;
        public static readonly int BLOCK_SIZE_BITSHIFT = NumberOfSetBits(BLOCK_SIZE_MASK);

        public static readonly int CIPHER_KEY_SIZE = 16;
        public static readonly int CIPHER_BLOCK_SIZE = 16;

        public static readonly int NUM_LOCKS = 16; // must be a power of 2!
        public static readonly int NUM_LOCKS_MASK = NUM_LOCKS - 1;

        public static readonly int MAX_CRYPTER_POOL_SIZE = 32;

        public static readonly int MAX_FILE_SIZE = 500*1024*1024; // 500MiB

        public static int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

    }
}
