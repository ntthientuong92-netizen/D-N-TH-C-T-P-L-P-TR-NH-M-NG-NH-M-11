using System;
using System.Collections.Generic;
using System.IO;
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
        using StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);

        try
        {
            while (true)
            {
                string? jsonMessage = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(jsonMessage)) break;

                Console.WriteLine($"[NHẬN]: {jsonMessage}");

                await BroadcastAsync(jsonMessage, client);
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

    private static async Task BroadcastAsync(string message, TcpClient excludeClient)
    {
        List<TcpClient> targetClients;
        lock (lockObj)
        {
            targetClients = new List<TcpClient>(clients);
        }

        foreach (var c in targetClients)
        {
            if (c != excludeClient)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(c.GetStream(), Encoding.UTF8) { AutoFlush = true };
                    await writer.WriteLineAsync(message);
                }
                catch { }
            }
        }
    }
}
