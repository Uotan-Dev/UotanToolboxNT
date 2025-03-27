using DiskPartitionInfo.FluentApi;

namespace DiskPartitionInfo
{
    public static class DiskPartitionInfo
    {
        public static IMbrReader ReadMbr()
            => new MbrReader();

        public static IGptReaderLocation ReadGpt()
            => new GptReader();
    }
}
