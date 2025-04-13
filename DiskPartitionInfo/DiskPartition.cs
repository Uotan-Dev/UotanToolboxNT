using DiskPartitionInfo.FluentApi;

namespace DiskPartitionInfo
{
    public static class DiskPartition
    {
        public static IMbrReader ReadMbr()
            => new MbrReader();

        public static IGptReaderLocation ReadGpt()
            => new GptReader();

        public static IGptWriterLocation WriteGpt()
            => new GptWriter();
    }
}
