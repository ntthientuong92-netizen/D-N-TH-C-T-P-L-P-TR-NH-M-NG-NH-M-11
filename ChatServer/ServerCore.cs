using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
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

                        // Chuyển tiếp gói tin đăng nhập đến các client khác (Broadcast)
                        BroadcastPacket(packet, clientName);

                        // Gửi danh sách thành viên cho TẤT CẢ: client mới cũng biết được
                        // những người đã vào phòng trước đó (để chọn người nhận khi Forward)
                        BroadcastUserList();
                        continue;
                    }

                    RoutePacket(packet);
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

                    // Cập nhật lại danh sách thành viên để người đã thoát không còn
                    // hiện trong danh sách chọn người nhận khi Forward
                    BroadcastUserList();
                }
                client.Close();
            }
        }

        /// <summary>
        /// Định tuyến gói tin: "All" -> broadcast cho mọi người khác;
        /// ngược lại -> chỉ gửi cho đúng người nhận (phục vụ Forward).
        /// </summary>
        private void RoutePacket(MessagePacket packet)
        {
            string receiver = packet.Receiver;

            if (string.IsNullOrWhiteSpace(receiver) ||
                receiver.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                BroadcastPacket(packet, packet.Sender);
                return;
            }

            TcpClient target = null;
            lock (connectedClients)
            {
                connectedClients.TryGetValue(receiver, out target);
            }

            if (target != null && SendPacketTo(target, packet))
            {
                ServerLogger.Log($"Đã chuyển tin nhắn riêng: {packet.Sender} -> {receiver}");
                return;
            }

            // Người nhận không trực tuyến -> báo lại cho người gửi
            TcpClient senderClient = null;
            lock (connectedClients)
            {
                connectedClients.TryGetValue(packet.Sender, out senderClient);
            }

            if (senderClient != null)
            {
                SendPacketTo(senderClient, new MessagePacket
                {
                    Type = PacketType.System,
                    Sender = "Server",
                    Receiver = packet.Sender,
                    Content = $"Không thể gửi tới \"{receiver}\": người dùng không trực tuyến."
                });
            }
        }

        private void BroadcastPacket(MessagePacket packet, string senderName)
        {
            lock (connectedClients)
            {
                foreach (var kvp in connectedClients)
                {
                    // Không gửi lại cho chính người gửi (senderName = null -> gửi cho tất cả)
                    if (kvp.Key != senderName)
                    {
                        SendPacketTo(kvp.Value, packet);
                    }
                }
            }
        }

        /// <summary>
        /// Gửi danh sách tên thành viên đang trực tuyến cho toàn bộ client.
        /// Content là JSON của List&lt;string&gt;.
        /// </summary>
        private void BroadcastUserList()
        {
            List<string> names;
            lock (connectedClients)
            {
                names = connectedClients.Keys.ToList();
            }

            BroadcastPacket(new MessagePacket
            {
                Type = PacketType.UserListUpdate,
                Sender = "Server",
                Receiver = "All",
                Content = JsonSerializer.Serialize(names)
            }, null);
        }

        /// <summary>
        /// Ghi một gói tin xuống kết nối của client. Khoá theo từng TcpClient để nhiều luồng
        /// không ghi xen kẽ làm hỏng khung dữ liệu (length-prefix).
        /// </summary>
        private bool SendPacketTo(TcpClient client, MessagePacket packet)
        {
            try
            {
                lock (client)
                {
                    NetworkProtocol.SendPacket(client.GetStream(), packet);
                }
                return true;
            }
            catch
            {
                // Lỗi khi gửi đến một client cụ thể sẽ được bỏ qua để không ảnh hưởng client khác
                return false;
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
