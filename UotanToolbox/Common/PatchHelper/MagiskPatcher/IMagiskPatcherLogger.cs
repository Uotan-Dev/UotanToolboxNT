namespace MagiskPatcher
{
    public interface IMagiskPatcherLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Debug(string message);
    }
}
