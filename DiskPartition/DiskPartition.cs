using DiskPartition.FluentApi;

namespace DiskPartition
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
