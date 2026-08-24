using System;
using System.IO;

namespace ChatServer
{
    public static class ServerLogger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_log.txt");
        private static readonly object logLock = new object();
        public static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

            Console.WriteLine(logEntry);

            lock (logLock)
            {
                try
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch
                {

                }
            }
        }
    }
}