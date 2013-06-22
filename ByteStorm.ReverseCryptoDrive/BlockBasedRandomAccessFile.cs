using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    class BlockBasedRandomAccessFile
    {
        private RandomAccessFile randomAccessFile;

        public BlockBasedRandomAccessFile(RandomAccessFile randomAccessFile)
        {
            // TODO: Complete member initialization
            this.randomAccessFile = randomAccessFile;
        }
    }
}
