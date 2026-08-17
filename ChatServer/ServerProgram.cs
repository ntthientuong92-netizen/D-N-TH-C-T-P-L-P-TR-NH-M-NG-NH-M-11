using System;

namespace ChatServer
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int port = 8888;

            ServerCore server = new ServerCore(port);
            server.Start();

            Console.WriteLine("Nhấn phím 'Q' để dừng Server...");
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                {
                    server.Stop();
                    break;
                }
            }
        }
    }
}