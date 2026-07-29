using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    private static TcpListener? listener;
    private static List<TcpClient> clients = new List<TcpClient>();
    private static readonly object lockObj = new object();

    static async Task Main(string[] args)
    {
        int port = 8888;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[SERVER] Đang lắng nghe tại cổng {port}...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            lock (lockObj)
            {
                clients.Add(client);
            }
            Console.WriteLine("[SERVER] Có một Client mới kết nối!");
            _ = HandleClientAsync(client);
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[8192];

        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[NHẬN]: {jsonMessage}");

                Broadcast(jsonMessage, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LỖI]: {ex.Message}");
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(client);
            }
            client.Close();
            Console.WriteLine("[SERVER] Một Client đã ngắt kết nối.");
        }
    }

    private static void Broadcast(string message, TcpClient excludeClient)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        lock (lockObj)
        {
            foreach (var client in clients)
            {
                if (client != excludeClient)
                {
                    try
                    {
                        var stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch { }
                }
            }
        }
    }
}