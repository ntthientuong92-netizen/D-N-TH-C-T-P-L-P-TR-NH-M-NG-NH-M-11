using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SharedLibrary;

namespace ChatServer
{
    public class ServerCore
    {
        private TcpListener listener;
        private bool isRunning;
        private Dictionary<string, TcpClient> connectedClients;
        private int port;

        public ServerCore(int port)
        {
            this.port = port;
            connectedClients = new Dictionary<string, TcpClient>();
        }

        public void Start()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;
                ServerLogger.Log($"Server đã khởi động thành công trên cổng {port}.");

                // Vòng lặp chấp nhận client kết nối
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();
            }
            catch (Exception ex)
            {
                ExceptionManager.HandleException(ex, "ServerCore.Start");
            }
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        ExceptionManager.HandleException(ex, "ServerCore.AcceptClients");
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = null;
            string clientName = string.Empty;

            try
            {
                stream = client.GetStream();
                while (isRunning)
                {
                    MessagePacket packet = NetworkProtocol.ReceivePacket(stream);
                    if (packet == null) break;

                    if (packet.Type == PacketType.Login)
                    {
                        clientName = packet.Sender;
                        lock (connectedClients)
                        {
                            if (!connectedClients.ContainsKey(clientName))
                                connectedClients.Add(clientName, client);
                            else
                                connectedClients[clientName] = client;
                        }
                        ServerLogger.Log($"Client đăng nhập thành công: {clientName}");
                    }

                    // Chuyển tiếp gói tin đến các client khác (Broadcast)
                    BroadcastPacket(packet, clientName);
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.HandleException(ex, $"HandleClient [{clientName}]");
            }
            finally
            {
                if (!string.IsNullOrEmpty(clientName))
                {
                    lock (connectedClients)
                    {
                        if (connectedClients.ContainsKey(clientName))
                            connectedClients.Remove(clientName);
                    }
                    ServerLogger.Log($"Client đã ngắt kết nối: {clientName}");
                }
                client.Close();
            }
        }

        private void BroadcastPacket(MessagePacket packet, string senderName)
        {
            byte[] dataBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(packet);
            
            lock (connectedClients)
            {
                foreach (var kvp in connectedClients)
                {
                    // Không gửi lại cho chính người gửi
                    if (kvp.Key != senderName)
                    {
                        try
                        {
                            NetworkStream stream = kvp.Value.GetStream();
                            // Gửi length-prefix và byte data
                            byte[] lengthBytes = BitConverter.GetBytes(dataBytes.Length);
                            stream.Write(lengthBytes, 0, lengthBytes.Length);
                            stream.Write(dataBytes, 0, dataBytes.Length);
                            stream.Flush();
                        }
                        catch
                        {
                            // Lỗi khi gửi đến một client cụ thể sẽ được bỏ qua để không ảnh hưởng client khác
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            ServerLogger.Log("Server đã dừng hoạt động.");
        }
    }
}