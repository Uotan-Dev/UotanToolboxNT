using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WuXingLibrary.code.Utility
{
    public static class Log
    {
        private static readonly object _lock = new();
        private static readonly string BaseLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"tool", "log");

        // Gets the line number where this method is called.
        public static int GetLineNum()
        {
            return new StackTrace(1, true).GetFrame(0).GetFileLineNumber();
        }

        // Gets the current source file name where this method is called.
        public static string GetCurSourceFileName()
        {
            return new StackTrace(1, true).GetFrame(0).GetFileName();
        }

        public static void W(string deviceName, Exception ex, bool stopFlash)
        {
            string message = ex.Message;
            string stackTrace = ex.StackTrace;
            if (stopFlash)
            {
                message = $"error: {message}";
                stackTrace = $"error {stackTrace}";
                W(deviceName, message, stackTrace);
            }
            else
            {
                W(deviceName, message, true);
            }
        }

        public static void W(string deviceName, string msg)
        {
            W(deviceName, msg, true);
        }

        public static void W(string deviceName, string msg, bool throwEx)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                W(msg, throwEx);
                return;
            }

            WriteLogToFile($"[{GetCurrentTimestamp()}  {deviceName}]: {msg}", throwEx);
        }

        public static void W(string msg)
        {
            W(msg, false);
        }

        public static void W(string msg, bool throwEx)
        {
            WriteLogToFile($"[{GetCurrentTimestamp()}]: {msg}", throwEx);
        }

        public static void WFlashStatus(string msg)
        {
            WriteFlashLog(msg, "Result");
        }

        public static void WFlashDebug(string msg)
        {
            WriteFlashLog(msg, "mesdebug");
        }

        private static void WriteFlashLog(string msg, string logType)
        {
            try
            {
                string fileName = $"{logType}@{DateTime.Now:yyyyMd}.txt";
                string path = Path.Combine(BaseLogPath, fileName);
                EnsureDirectoryExists(path);
                using var writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.Default);
                writer.WriteLine($"[{DateTime.Now.ToLongTimeString()}]: {msg}");
            }
            catch (Exception ex)
            {
                W($"{logType} {msg} {ex.Message} {ex.StackTrace}", false);
            }
        }

        public static void WriteFile(string log, string path)
        {
            lock (_lock)
            {
                EnsureDirectoryExists(path);
                using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(log);
            }
        }

        public static void WriteLog(string log, string logPath)
        {
            WriteFile(log, logPath);
        }

        public static void WriteErrorLog(string log, string logPath)
        {
            WriteFile(log, logPath);
        }

        // Overload for combining title, stack trace, and message.
        public static void W(string title, string stackTrace, string message)
        {
            W($"{title} {message} {stackTrace}", false);
        }

        private static string GetLogFileName()
        {
            return $"wuxing@{DateTime.Now:yyyyMd}.txt";
        }

        private static string GetCurrentTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff");
        }

        private static void EnsureDirectoryExists(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Helper method that writes logs to the default log file.
        private static void WriteLogToFile(string log, bool throwEx = true)
        {
            string logFileName = GetLogFileName();
            string path = Path.Combine(BaseLogPath, logFileName);
            try
            {
                WriteFile(log, path);
            }
            catch (Exception ex)
            {
                if (throwEx)
                {
                    throw new InvalidOperationException("Failed to write log.", ex);
                }
            }
        }
    }
}