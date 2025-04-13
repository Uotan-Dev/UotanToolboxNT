namespace DiskPartitionInfo.FluentApi
{
    public interface IGptWriterLocation
    {
        /// <summary>
        /// Write to the primary GPT located at the beginning of the disk.
        /// </summary>
        IGptWriter Primary();

        /// <summary>
        /// Write to the secondary GPT located at the end of the disk.
        /// </summary>
        IGptWriter Secondary();
    }
}