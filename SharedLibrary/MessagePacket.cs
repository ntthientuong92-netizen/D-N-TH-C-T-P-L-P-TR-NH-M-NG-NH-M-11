using System;
using System.Collections.Generic;

namespace SharedLibrary
{
    public enum PacketType
    {
        Login,          // Đăng nhập / Kết nối ban đầu kèm tên và avatar
        Chat,           // Tin nhắn văn bản thông thường / có kèm emoji
        Reply,          // Phản hồi một tin nhắn cụ thể
        Forward,        // Chuyển tiếp tin nhắn
        UserListUpdate  // Cập nhật danh sách người dùng online
    }

    [Serializable]
    public class MessagePacket
    {
        public PacketType Type { get; set; }        // Loại gói tin
        public string Sender { get; set; }          // Người gửi
        public string Receiver { get; set; }        // Người nhận ("All" nếu là broadcast hoặc tên cụ thể)
        public string Content { get; set; }         // Nội dung tin nhắn chính
        public string ReplyToContent { get; set; }  // Nội dung tin nhắn đang được reply (nếu có)
        public string AvatarBase64 { get; set; }    // Ảnh đại diện dưới dạng chuỗi Base64
        public DateTime Timestamp { get; set; }     // Thời gian gửi tin

        public MessagePacket()
        {
            Timestamp = DateTime.Now;
        }
    }
}