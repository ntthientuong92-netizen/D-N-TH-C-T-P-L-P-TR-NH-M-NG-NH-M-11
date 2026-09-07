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

        /// Nhận một MessagePacket từ NetworkStream.
        /// Trả về null nếu kết nối đã đóng, hoặc nếu dữ liệu nhận được bị lỗi/không hợp lệ (thay vì làm crash chương trình).
        /// </summary>
        public static MessagePacket ReceivePacket(NetworkStream stream)
        {
            byte[] lengthBytes = ReadExact(stream, 4);
            if (lengthBytes == null) return null; // kết nối đã đóng

            int dataLength = BitConverter.ToInt32(lengthBytes, 0);

            // Chặn trường hợp dataLength âm hoặc quá lớn bất thường (dữ liệu bị hỏng)
            if (dataLength < 0 || dataLength > 10_000_000)
            {
                return null;
            }

            byte[] dataBytes = ReadExact(stream, dataLength);
            if (dataBytes == null) return null; // kết nối đóng giữa chừng

            try
            {
                string jsonString = Encoding.UTF8.GetString(dataBytes);
                return JsonSerializer.Deserialize<MessagePacket>(jsonString);
            }
            catch (JsonException)
            {
                // Dữ liệu nhận được không phải JSON hợp lệ (gói tin bị lỗi/bị cắt)
                // -> bỏ qua gói tin này thay vì làm crash Server/Client
                return null;
            }
        }

        /// <summary>
        /// Đọc chính xác 'count' byte từ stream, dồn lại thành 1 buffer đầy đủ.
        /// stream.Read() có thể trả về ít hơn số byte yêu cầu trong 1 lần gọi,
        /// nên cần lặp lại tới khi đọc đủ.
        /// Trả về null nếu bên kia đóng kết nối giữa chừng (đọc được 0 byte).
        /// </summary>
        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            if (count == 0) return Array.Empty<byte>();

            byte[] buffer = new byte[count];
            int totalRead = 0;

            while (totalRead < count)
            {
                int bytesRead = stream.Read(buffer, totalRead, count - totalRead);
                if (bytesRead == 0)
                {
                    return null; // Client/Server bên kia đã đóng kết nối đột ngột
                }
                totalRead += bytesRead;
            }

            return buffer;
        }
    }
}