using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    class BlockCrypto
    {
        private byte[] keyBytes;
        private byte[] ivBytes;

        public BlockCrypto(byte[] keyBytes, byte[] ivBytes)
        {
            // TODO: Complete member initialization
            this.keyBytes = keyBytes;
            this.ivBytes = ivBytes;
        }
    }
}
