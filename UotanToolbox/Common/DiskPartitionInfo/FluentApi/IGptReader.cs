using System.IO;
using DiskPartitionInfo.Gpt;

namespace DiskPartitionInfo.FluentApi
{
    public interface IGptReader
    {
#if Windows
        /// <summary>
        /// Reads the GPT from the physical drive given its number.
        /// </summary>
        /// <param name="driveNumber">Drive number, e.g. 0 or 3.</param>
        /// <returns>The GUID Partition Table information.</returns>
        GuidPartitionTable FromPhysicalDriveNumber(int driveNumber);

        /// <summary>
        /// Reads the GPT record from the physical drive given a volume letter.
        /// GPT will be read from the corresponding physical drive if it exists.
        /// </summary>
        /// <param name="volumeLetter">Volume letter, e.g. C: or F:.</param>
        /// <returns>The GUID Partition Table information.</returns>
        GuidPartitionTable FromVolumeLetter(string volumeLetter);
#endif

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
