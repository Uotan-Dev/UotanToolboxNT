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
using System.IO;
using HuaweiUpdateLibrary.Streams;

namespace HuaweiUpdateLibrary.Core
{
    /// <summary>
    /// Class to work with entries inside an <see cref="UpdateFile"/>
    /// </summary>
    public class UpdateEntry
    {
        /// <summary>
        /// Default block size for an <see cref="UpdateEntry"/>
        /// </summary>
        public const ushort DefaultBlockSize = 4096;
        private const UInt32 FileMagic = 0xA55AAA55;
        private FileHeader _fileHeader;
        
        // Can be set by UpdateFile
        internal long DataOffset;
        internal ushort[] CheckSumTable;

        /// <summary>
        /// <see cref="EntryType"/>
        /// </summary>
        public EntryType Type { get; internal set; }
        
        /// <summary>
        /// Header Id
        /// </summary>
        public UInt32 HeaderId
        {
            get { return _fileHeader.HeaderId; }
            private set { _fileHeader.HeaderId = value; }
        }

        /// <summary>
        /// Header size
        /// </summary>
        public UInt32 HeaderSize
        {
            get { return _fileHeader.HeaderSize; }
            internal set { _fileHeader.HeaderSize = value; }
        }

        /// <summary>
        /// Hardware id
        /// </summary>
        public string HardwareId
        {
            get { return Utilities.GetString(_fileHeader.HardwareId); }
            set { Utilities.SetCharArray(value, _fileHeader.HardwareId); }
        }

        /// <summary>
        /// File sequence
        /// </summary>
        public UInt32 FileSequence
        {
            get { return _fileHeader.FileSequence; }
            set { _fileHeader.FileSequence = value; }
        }

        /// <summary>
        /// File size
        /// </summary>
        public UInt32 FileSize
        {
            get { return _fileHeader.FileSize; }
            internal set { _fileHeader.FileSize = value; }
        }

        /// <summary>
        /// File date
        /// </summary>
        public string FileDate
        {
            get { return Utilities.GetString(_fileHeader.FileDate); }
            set { Utilities.SetCharArray(value, _fileHeader.FileDate); }
        }

        /// <summary>
        /// File time
        /// </summary>
        public string FileTime
        {
            get { return Utilities.GetString(_fileHeader.FileTime); }
            set { Utilities.SetCharArray(value, _fileHeader.FileTime); }
        }

        /// <summary>
        /// File type
        /// </summary>
        public string FileType
        {
            get { return Utilities.GetString(_fileHeader.FileType); }
            set { Utilities.SetCharArray(value, _fileHeader.FileType); }
        }

        /// <summary>
        /// Header checksum
        /// </summary>
        public UInt16 HeaderChecksum
        {
            get { return _fileHeader.HeaderChecksum; }
            private set { _fileHeader.HeaderChecksum = value; }
        }

        /// <summary>
        /// Block size
        /// </summary>
        public UInt16 BlockSize
        {
            get { return _fileHeader.BlockSize; }
            private set { _fileHeader.BlockSize = value; }
        }

        internal byte[] GetHeader()
        {
            byte[] result;

            if (!Utilities.TypeToByte(_fileHeader, out result))
                throw new Exception("TypeToByte() failed.");

            return result;
        }

        internal void ComputeHeaderChecksum()
        {
            // Reset checksum
            HeaderChecksum = 0;

            // Calculate checksum
            HeaderChecksum = Utilities.Crc.ComputeSum(GetHeader());
        }

        private void ReadEntry(Stream stream, bool checksum)
        {
            var reader = new BinaryReader(stream);

            // Read header
            if (!Utilities.ByteToType(reader, out _fileHeader))
                throw new Exception("ByteToType() failed @" + reader.BaseStream.Position);

            // Check header magic
            if (HeaderId != FileMagic)
                throw new Exception("Invalid file.");

            // Validate checksum
            if (checksum)
            {
                // Save current checksum
                var crc = HeaderChecksum;

                // Caclulate checksum
                ComputeHeaderChecksum();
                
                // Verify crc
                if (HeaderChecksum != crc)
                {
                    throw new Exception(string.Format("Checksum error @{0:X08}: {1:X04}<>{2:X04}", stream.Position, _fileHeader.HeaderChecksum, crc));
                }
            }

            // Calculate checksum table size
            var checksumTableSize = HeaderSize - FileHeader.Size;

            // Allocate checksum table
            CheckSumTable = new ushort[checksumTableSize / Utilities.UshortSize];

            // Read checksum table
            for (var count = 0; count < CheckSumTable.Length; count++) { CheckSumTable[count] = reader.ReadUInt16(); }

            // Save position of file data
            DataOffset = stream.Position;
        }

        private UpdateEntry(Stream stream, bool checksum)
        {
            ReadEntry(stream, checksum);
        }

        private void CreateEntry(ushort blockSize)
        {
            // Set FileHeader
            _fileHeader = FileHeader.Create();

            var now = DateTime.Now;

            // Set default date/time
            FileDate = now.ToString("yyyy.MM.dd");
            FileTime = now.ToString("HH.mm.ss");

            // Set id
            HeaderId = FileMagic;
            
            // Block size
            BlockSize = blockSize;
        }
        
        private UpdateEntry(ushort blockSize)
        {
            CreateEntry(blockSize);
        }

        /// <summary>
        /// Read an <see cref="UpdateEntry"/> from a given <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read from</param>
        /// <param name="checksum">Verify header checksum</param>
        /// <returns><see cref="UpdateEntry"/></returns>
        public static UpdateEntry Read(Stream stream, bool checksum = true)
        {
            return new UpdateEntry(stream, checksum);
        }

        /// <summary>
        /// Create <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="blockSize">BlockSize</param>
        /// <returns><see cref="UpdateEntry"/></returns>
        public static UpdateEntry Create(ushort blockSize = DefaultBlockSize)
        {
            return new UpdateEntry(blockSize);
        }

        /// <summary>
        /// Get a <see cref="Stream"/> to the file data in the given <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read from</param>
        /// <returns><see cref="Stream"/></returns>
        public Stream GetDataStream(Stream stream)
        {
            // Seek to offset
            stream.Seek(DataOffset, SeekOrigin.Begin);

            // Return stream
            return new PartialStream(stream, FileSize);
        }

        /// <summary>
        /// Get a <see cref="Stream"/> to the file data in the given file
        /// </summary>
        /// <param name="fileName">File to read from</param>
        /// <returns><see cref="Stream"/></returns>
        public Stream GetDataStream(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Seek to offset
            stream.Seek(DataOffset, SeekOrigin.Begin);

            // Return stream
            return new PartialStream(stream, FileSize);
        }
        
        /// <summary>
        /// Extract the current <see cref="UpdateEntry"/> from an input <see cref="Stream"/> to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="input">Input <see cref="Stream"/></param>
        /// <param name="output">Output <see cref="Stream"/></param>
        /// <param name="checksum">Verify file data checksum</param>
        public void Extract(Stream input, Stream output, bool checksum = true)
        {
            // Get stream to file data
            var reader = GetDataStream(input);
            var buffer = new byte[BlockSize];
            var blockNumber = 0;
            int size;

            // Read file data
            while ((size = reader.Read(buffer, 0, BlockSize)) > 0)
            {
                // Verify crc
                if (checksum)
                {
                    // Calculate block crc
                    var crc = Utilities.Crc.ComputeSum(buffer, 0, size);

                    // Verify
                    if (crc != CheckSumTable[blockNumber])
                    {
                        throw new Exception(string.Format("Checksum error in block {0}@{1:X08}: {2:X04}<>{3:X04}", blockNumber, (reader.Position - size),
                            CheckSumTable[blockNumber], crc));
                    }
                }

                // Write to output file
                output.Write(buffer, 0, size);

                // Increase block
                blockNumber++;
            }
        }

        /// <summary>
        /// Extract the current <see cref="UpdateEntry"/> from an input <see cref="Stream"/> to an output file
        /// </summary>
        /// <param name="input">Input <see cref="Stream"/></param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify file data checksum</param>
        public void Extract(Stream input, string output, bool checksum = true)
        {
            using (var outputStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Extract
                Extract(input, outputStream, checksum);
            }
        }

        /// <summary>
        /// Extract the current <see cref="UpdateEntry"/> from an input file to an output <see cref="Stream"/>
        /// </summary>
        /// <param name="input">Input file</param>
        /// <param name="output">Output <see cref="Stream"/></param>
        /// <param name="checksum">Verify file data checksum</param>
        public void Extract(string input, Stream output, bool checksum = true)
        {
            using (var inputStream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Extract
                Extract(inputStream, output, checksum);
            }
        }

        /// <summary>
        /// Extract the current <see cref="UpdateEntry"/> from an input file to an output file
        /// </summary>
        /// <param name="input">Input file</param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify file data checksum</param>
        public void Extract(string input, string output, bool checksum = true)
        {
            using (var inputStream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var outputStream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Extract
                    Extract(inputStream, outputStream, checksum);
                }
            }
        }

        /// <summary>
        /// Write FileHeader to a stream
        /// </summary>
        /// <param name="stream">Output <see cref="Stream"/></param>
        public void ExtractHeader(Stream stream)
        {
            var result = GetHeader();

            // Write
            stream.Write(result, 0, result.Length);
        }

        /// <summary>
        /// Write FileHeader to a file
        /// </summary>
        /// <param name="output">Ouput file</param>
        public void ExtractHeader(string output)
        {
            using (var stream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // Extract
                ExtractHeader(stream);
            }
        }
    }
}
