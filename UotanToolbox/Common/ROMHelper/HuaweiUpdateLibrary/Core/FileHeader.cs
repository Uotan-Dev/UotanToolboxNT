/*
 *  Copyright 2015 worstenbrood
 *  
 *  This file is part of HuaweiUpdateLibrary.
 *  
 *  HuaweiUpdateLibrary is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as 
 *  published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *  
 *  HuaweiUpdateLibrary is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *  You should have received a copy of the GNU General Public License along with HuaweiUpdateLibrary. 
 *  If not, see http://www.gnu.org/licenses/.
 *  
 */

using System;
using System.Runtime.InteropServices;

namespace HuaweiUpdateLibrary.Core
{
    // Credits: ZeBadger (zebadger@hotmail.com)
    // For some stuff ripped from split_updata.pl

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct FileHeader
    {
        public UInt32 HeaderId;
        public UInt32 HeaderSize;
        public UInt32 Unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] HardwareId;
        public UInt32 FileSequence;
        public UInt32 FileSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileDate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Blank1;
        public UInt16 HeaderChecksum;
        public UInt16 BlockSize;
        public UInt16 Blank2;

        /// <summary>
        /// Return size of this struct
        /// </summary>
        public static readonly int Size = Marshal.SizeOf(typeof (FileHeader));

        /// <summary>
        /// Create intance of <see cref="FileHeader"/>
        /// </summary>
        /// <returns></returns>
        public static FileHeader Create()
        {
            var result = new FileHeader
            {
                HardwareId = new byte[8],
                FileDate = new byte[16],
                FileTime = new byte[16],
                FileType = new byte[16],
                Blank1 = new byte[16]
            };

            return result;
        }
    }
}
