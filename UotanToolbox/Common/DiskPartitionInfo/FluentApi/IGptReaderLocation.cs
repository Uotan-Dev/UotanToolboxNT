namespace DiskPartitionInfo.FluentApi
{
    public interface IGptReaderLocation
    {
        /// <summary>
        /// Read the primary GPT located at the beginning of the disk.
        /// </summary>
        IGptReader Primary();

        /// <summary>
        /// Read the secondary GPT located at the end of the disk.
        /// </summary>
        IGptReader Secondary();
    }
}
