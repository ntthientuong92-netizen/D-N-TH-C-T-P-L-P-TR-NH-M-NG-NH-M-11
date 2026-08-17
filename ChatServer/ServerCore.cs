using System;

namespace ChatServer
{
    public static class ServerLogger
    {
        public static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(logEntry);
        }
    }
}