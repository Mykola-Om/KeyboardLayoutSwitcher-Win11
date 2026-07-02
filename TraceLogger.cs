using System;
using System.IO;
using System.Text;
using System.Threading;

namespace KeyboardLayoutSwitcher
{
    public static class TraceLogger
    {
        private static bool isEnabled = false;
        private static readonly object lockObj = new object();
        private static string logPath;
        private static long maxLogSizeBytes = 5 * 1024 * 1024; // 5 MB

        public static void Initialize(bool enableTrace)
        {
            isEnabled = enableTrace;
            if (isEnabled)
            {
                logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "KeyboardLayoutSwitcher",
                    "trace.log");

                CleanOldLogIfNeeded();
            }
        }

        public static void Trace(string message)
        {
            if (!isEnabled || string.IsNullOrEmpty(logPath))
                return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    lock (lockObj)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                        string line = $"{DateTime.Now:HH:mm:ss.fff} | {message}\r\n";
                        File.AppendAllText(logPath, line, Encoding.UTF8);
                    }
                }
                catch
                {
                    // Logging failed silently - don't crash the app
                }
            });
        }

        private static void CleanOldLogIfNeeded()
        {
            try
            {
                if (File.Exists(logPath))
                {
                    FileInfo info = new FileInfo(logPath);
                    if (info.Length > maxLogSizeBytes)
                    {
                        File.Delete(logPath);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
