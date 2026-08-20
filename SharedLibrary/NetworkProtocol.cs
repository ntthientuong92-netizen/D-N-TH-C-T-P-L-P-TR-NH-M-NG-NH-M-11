using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json; // Hoặc Newtonsoft.Json nếu project của bạn dùng thư viện đó

namespace SharedLibrary
{
    public static class NetworkProtocol
    {
        // Gửi một MessagePacket qua NetworkStream
        public static void SendPacket(NetworkStream stream, MessagePacket packet)
        {
            string jsonString = JsonSerializer.Serialize(packet);
            byte[] dataBytes = Encoding.UTF8.GetBytes(jsonString);
            
            // Lấy độ dài của mảng byte để làm tiền tố (Length-prefix framing)
            byte[] lengthBytes = BitConverter.GetBytes(dataBytes.Length);

            // Gửi độ dài trước, sau đó gửi nội dung dữ liệu
            stream.Write(lengthBytes, 0, lengthBytes.Length);
            stream.Write(dataBytes, 0, dataBytes.Length);
            stream.Flush();
        }

        // Nhận một MessagePacket từ NetworkStream
        public static MessagePacket ReceivePacket(NetworkStream stream)
        {
            byte[] lengthBytes = new byte[4];
            int bytesRead = stream.Read(lengthBytes, 0, 4);
            if (bytesRead == 0) return null;

            int dataLength = BitConverter.ToInt32(lengthBytes, 0);
            byte[] dataBytes = new byte[dataLength];
            
            int totalRead = 0;
            while (totalRead < dataLength)
            {
                int read = stream.Read(dataBytes, totalRead, dataLength - totalRead);
                if (read == 0) return null;
                totalRead += read;
            }

            string jsonString = Encoding.UTF8.GetString(dataBytes);
            return JsonSerializer.Deserialize<MessagePacket>(jsonString);
        }
    }
}