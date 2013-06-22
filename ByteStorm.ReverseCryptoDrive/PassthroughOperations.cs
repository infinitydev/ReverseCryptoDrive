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

    public class PassthroughOperations : DokanOperations
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

        internal const uint FILE_FLAG_BACKUP_SEMANTICS  = 0x02000000;

        private string root = "C:\\Livedrive";

        public PassthroughOperations()
        {
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            SafeFileHandle handle = (SafeFileHandle)info.Context;
            if (handle != null)
                handle.Close();
            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            //Trace.WriteLine(string.Format("CreateDirectory {0}", filename));
            if (!Win32Api.CreateDirectory(root + filename, IntPtr.Zero))
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
            //Trace.WriteLine(string.Format("CreateFile {0}", filename));
            string path = root + filename;
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
                flagsAndOptions |= FILE_FLAG_BACKUP_SEMANTICS;

            //Trace.WriteLine("Win32Api.CreateFile(" + (root + filename) + ", [" + access + "], [" + share + "], " + IntPtr.Zero + ", " + mode + ", " + flagsAndOptions + ", " + IntPtr.Zero + ");");


            SafeFileHandle handle = Win32Api.CreateFile(root+filename, access, share, IntPtr.Zero, mode, flagsAndOptions, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                return -Marshal.GetLastWin32Error();
            } else {
                info.Context = handle;
            }
            return 0;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            if (!Win32Api.RemoveDirectory(root+filename))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            if (!Win32Api.DeleteFile(root+filename))
            {
                return -Marshal.GetLastWin32Error();
            }
            return 0;
        }


        public int FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            if (!Win32Api.FlushFileBuffers((SafeFileHandle)info.Context))
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
            //Trace.WriteLine(string.Format("FindFiles {0}", root+filename));

            DirectoryInfo di = new DirectoryInfo(root+filename);
            try
            {
                foreach (DirectoryInfo f in di.GetDirectories())
                {
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.FileName = f.Name;
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = 0;
                    files.Add(fi);
                }
                foreach (FileInfo f in di.GetFiles())
                {
                    FileInformation fi = new FileInformation();
                    fi.Attributes = f.Attributes;
                    fi.CreationTime = f.CreationTime;
                    fi.FileName = f.Name;
                    fi.LastAccessTime = f.LastAccessTime;
                    fi.LastWriteTime = f.LastWriteTime;
                    fi.Length = f.Length;
                    files.Add(fi);
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
            //Trace.WriteLine(string.Format("GetFileInformation {0}", root+filename));

            if (!Win32Api.GetFileInformationByHandle((SafeFileHandle)info.Context, out fileinfo))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;
        }

        public int LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            uint offsetHi = (uint)(offset >> 32);
            uint offsetLo = (uint)(offset & 0xffffffff);
            uint lengthHi = (uint)(length >> 32);
            uint lengthLo = (uint)(length & 0xffffffff);
            if (!Win32Api.LockFile((SafeFileHandle)info.Context, offsetLo, offsetHi, lengthLo, lengthHi))
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
            //Trace.WriteLine(string.Format("MoveFile {0}", root+filename));

            if (!Win32Api.MoveFile(root+filename, root+newname))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;

        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            DirectoryInfo di = new DirectoryInfo(root+filename);
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
            //Trace.WriteLine(string.Format("ReadFile {0}", root+filename));

            setFilePointer((SafeFileHandle)info.Context, offset);
            if (!(Win32Api.ReadFile((SafeFileHandle)info.Context, buffer, buffer.Length, out readBytes, IntPtr.Zero)))
            {
                return -Marshal.GetLastWin32Error();
            }
            
            return 0;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            setFilePointer((SafeFileHandle)info.Context, length);
            if (!(Win32Api.SetEndOfFile((SafeFileHandle)info.Context)))
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

            if (!(Win32Api.SetFileTime((SafeFileHandle)info.Context, ref lCreationTime, ref lAccessTime, ref lModifiedTime)))
            {
                return -Marshal.GetLastWin32Error();
            }
            return -1;
        }



        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            uint offsetHi = (uint)(offset >> 32);
            uint offsetLo = (uint)(offset & 0xffffffff);
            uint lengthHi = (uint)(length >> 32);
            uint lengthLo = (uint)(length & 0xffffffff);
            if (!Win32Api.UnlockFile((SafeFileHandle)info.Context, offsetLo, offsetHi, lengthLo, lengthHi))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {
            //Trace.WriteLine("Unmount");
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

            setFilePointer((SafeFileHandle)info.Context, offset);
            if (!(Win32Api.WriteFile((SafeFileHandle)info.Context, buffer, buffer.Length, out writtenBytes, IntPtr.Zero)))
            {
                return -Marshal.GetLastWin32Error();
            }

            return 0;
        }


        public static bool IsContainerNameValid(string containerName)
        {
            return (Regex.IsMatch(containerName, @"(^([a-z]|\d))((-([a-z]|\d)|([a-z]|\d))+)$") && (3 <= containerName.Length) && (containerName.Length <= 63));
        }

        private bool setFilePointer(SafeFileHandle handle, long position)
        {
            return Win32Api.SetFilePointerEx(handle, position, out position, ByteStorm.PassthroughDrive.Win32Api.EMoveMethod.Begin);
        }
    }

}
