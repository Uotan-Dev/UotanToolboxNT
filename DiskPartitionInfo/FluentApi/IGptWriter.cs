using DiskPartitionInfo.Gpt;
using System.IO;

namespace DiskPartitionInfo.FluentApi
{
    public interface IGptWriter
    {
        /// <summary>
        /// Writes the GPT to the specified path.
        /// It can be a path to a file or to a physical drive.
        /// </summary>
        /// <param name="path">Path to file to write to, e.g. C:\GPT.bin or ../disk.img.</param>
        /// <param name="gpt">The GUID Partition Table information to write.</param>
        void ToPath(string path, GuidPartitionTable gpt);

        /// <summary>
        /// Writes the GPT to the given stream.
        /// The stream is not automatically closed after write operation.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        /// <param name="gpt">The GUID Partition Table information to write.</param>
        void ToStream(Stream stream, GuidPartitionTable gpt);
    }
}