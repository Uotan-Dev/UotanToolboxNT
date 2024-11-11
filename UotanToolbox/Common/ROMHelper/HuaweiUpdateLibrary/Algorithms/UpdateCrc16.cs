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
using System.Security.Cryptography;

namespace HuaweiUpdateLibrary.Algorithms
{
    /// <summary>
    /// Crc algorithm
    /// </summary>
    public class UpdateCrc16 : HashAlgorithm
    {
        private readonly ushort[] _table = new ushort[256];
        private readonly ushort _polynomial;
        private readonly ushort _xorValue;
        private readonly byte[] _initialSum;
        
        private void InitializeTable()
        {
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ _polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _table[i] = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialSum">Initial sum</param>
        /// <param name="polynomial">Polynomial</param>
        /// <param name="xorValue">Value used to xor final sum with</param>
        public UpdateCrc16(ushort initialSum = 0xFFFF, ushort polynomial = 0x8408, ushort xorValue = 0xFFFF)
        {
            _initialSum = BitConverter.GetBytes(initialSum);
            _polynomial = polynomial;
            _xorValue = xorValue;
            
            // Init table
            InitializeTable();

            // Initialize sum
            HashValue = _initialSum;
        }

        /// <summary>
        /// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm"/> class.
        /// </summary>
        public override void Initialize()
        {
            HashValue = _initialSum;
        }

        /// <summary>
        /// When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.
        /// </summary>
        /// <param name="array">The input to compute the hash code for. </param><param name="ibStart">The offset into the byte array from which to begin using data. </param><param name="cbSize">The number of bytes in the byte array to use as data. </param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var sum = BitConverter.ToUInt16(HashValue, 0);
            var i = ibStart;
            var size = (cbSize - ibStart) * 8;

            while (size >= 8)
            {
                var v = array[i++];
                sum = (ushort)((_table[(v ^ sum) & 0xFF] ^ (sum >> 8)) & 0xFFFF);
                size -= 8;
            }

            if (size != 0)
            {
                for (var n = array[i] << 8;; n >>= 1)
                {
                    if (size == 0) break;
                    size -= 1;
                    var flag = ((sum ^ n) & 1) == 0;
                    sum >>= 1;
                    if (flag) sum ^= _polynomial;
                }
            }

            HashValue = BitConverter.GetBytes(sum);
            HashSizeValue = HashValue.Length;
        }

        /// <summary>
        /// When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>
        /// The computed hash code.
        /// </returns>
        protected override byte[] HashFinal()
        {
            var result = BitConverter.GetBytes((ushort)((BitConverter.ToUInt16(HashValue, 0) ^ _xorValue) & 0xFFFF));

            // Reinit
            HashValue = _initialSum;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public UInt16 ComputeSum(byte[] buffer)
        {
            return ComputeSum(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public UInt16 ComputeSum(byte[] buffer, int offset, int count)
        {
            return BitConverter.ToUInt16(ComputeHash(buffer, offset, count), 0);
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        /// <returns>
        /// Always true.
        /// </returns>
        public override bool CanReuseTransform
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        /// <returns>
        /// true if multiple blocks can be transformed; otherwise, false.
        /// </returns>
        public override bool CanTransformMultipleBlocks
        {
            get { return true; }
        }
    }
}
