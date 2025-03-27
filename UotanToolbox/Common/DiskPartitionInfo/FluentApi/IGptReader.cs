using DiskPartitionInfo.Gpt;
using System.IO;

namespace DiskPartitionInfo.FluentApi
{
    public interface IGptReader
    {
        /// <summary>
        /// Reads the GPT from the given path.
        /// It can be a path to a file or to a physical drive.
        /// </summary>
        /// <param name="path">Path to disk or file to read from,
        /// e.g. C:\GPT.bin or ../disk.img.</param>
        /// <returns>The GUID Partition Table information.</returns>
        GuidPartitionTable FromPath(string path);

        /// <summary>
        /// Reads the GPT from the given stream.
        /// The stream is not automatically closed after read operation.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>The GUID Partition Table information.</returns>
        GuidPartitionTable FromStream(Stream stream);
    }
}
