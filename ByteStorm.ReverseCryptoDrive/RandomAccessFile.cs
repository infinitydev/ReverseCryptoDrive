using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteStorm.PassthroughDrive
{
    class RandomAccessFile
    {
        private SafeFileHandle handle;

        public RandomAccessFile(SafeFileHandle handle)
        {
            // TODO: Complete member initialization
            this.handle = handle;
        }
    }
}
