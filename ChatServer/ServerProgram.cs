using System;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            int port = 8888;

            ServerLogger.Log("========================================");
            ServerLogger.Log("    HỆ THỐNG TCP CHAT SERVER - NHÓM 11   ");
            ServerLogger.Log("========================================");

            ServerCore server = new ServerCore(port);

            Thread serverThread = new Thread(() =>
            {
                server.Start();
            });
            serverThread.IsBackground = true;
            serverThread.Start();

            ServerLogger.Log("Server đang chạy ngầm...");
            ServerLogger.Log("Nhấn phím [Enter] hoặc gõ 'exit' rồi Enter để tắt Server.");

            string input;
            do
            {
                input = Console.ReadLine();
            } while (input != null && input.ToLower() != "exit");

            ServerLogger.Log("Đang tiến hành đóng Server...");
            server.Stop();
            ServerLogger.Log("Đã thoát ứng dụng Server thành công.");
        }
    }
}