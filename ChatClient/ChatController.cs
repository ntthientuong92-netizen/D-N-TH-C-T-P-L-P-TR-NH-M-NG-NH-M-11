using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SharedLibrary;

namespace ChatClient
{
    public class ChatController
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected;

        public event Action<MessagePacket> OnMessageReceived;
        public event Action OnDisconnected;

        public bool Connect(string ip, int port, string username, string avatarBase64)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                ResourceCleanup.Track(client);
                
                stream = client.GetStream();
                ResourceCleanup.Track(stream);
                
                isConnected = true;

                // Gửi gói tin đăng nhập kèm Avatar
                MessagePacket loginPacket = new MessagePacket
                {
                    Type = PacketType.Login,
                    Sender = username,
                    Receiver = "Server",
                    AvatarBase64 = avatarBase64,
                    Content = $"{username} đã tham gia phòng chat. {EmojiHelper.GetRandomGreetingEmoji()}"
                };
                NetworkProtocol.SendPacket(stream, loginPacket);

                // Khởi động luồng lắng nghe tin nhắn từ Server
                receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReceiveLoop()
        {
            try
            {
                while (isConnected)
                {
                    MessagePacket packet = NetworkProtocol.ReceivePacket(stream);
                    if (packet == null) break;

                    OnMessageReceived?.Invoke(packet);
                }
            }
            catch
            {
                // Mất kết nối
            }
            finally
            {
                Disconnect();
            }
        }

        public void SendMessage(MessagePacket packet)
        {
            if (isConnected && stream != null)
            {
                try
                {
                    NetworkProtocol.SendPacket(stream, packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi gửi tin: " + ex.Message);
                }
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            ResourceCleanup.SafeClose(stream, "NetworkStream");
            ResourceCleanup.SafeClose(client, "TcpClient");
            OnDisconnected?.Invoke();
        }
    }
}
