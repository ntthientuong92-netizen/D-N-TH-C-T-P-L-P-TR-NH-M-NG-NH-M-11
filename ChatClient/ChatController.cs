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
                stream = client.GetStream();
                isConnected = true;

                MessagePacket loginPacket = new MessagePacket
                {
                    Type = PacketType.Login,
                    Sender = username,
                    Receiver = "Server",
                    AvatarBase64 = avatarBase64,
                    Content = $"{username} đã tham gia phòng chat."
                };
                NetworkProtocol.SendPacket(stream, loginPacket);

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

        public void SendReply(string sender, string content, string replyToContent, string avatarBase64)
        {
            MessagePacket packet = MessageProcessor.CreateReplyPacket(sender, content, replyToContent, avatarBase64);
            SendMessage(packet);
        }

        public void SendForward(string sender, string originalContent, string avatarBase64)
        {
            MessagePacket packet = MessageProcessor.CreateForwardPacket(sender, originalContent, avatarBase64);
            SendMessage(packet);
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