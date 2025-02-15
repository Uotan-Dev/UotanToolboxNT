using System.IO;
using DiskPartitionInfo.Extensions;
using DiskPartitionInfo.Mbr;
using DiskPartitionInfo.Models;

namespace DiskPartitionInfo.FluentApi
{
    internal partial class MbrReader : IMbrReader
    {
        private const int MbrLength = 512;

        /// <inheritdoc/>
        public MasterBootRecord FromPath(string path)
        {
            using var stream = File.OpenRead(path);

            return FromStream(stream);
        }

        /// <inheritdoc/>
        public MasterBootRecord FromStream(Stream stream)
        {
            var data = new byte[MbrLength];
            stream.Read(buffer: data, offset: 0, count: MbrLength);

            var mbr = data.ToStruct<ClassicalMasterBootRecord>();
            return new MasterBootRecord(mbr);
        }
    }
}
