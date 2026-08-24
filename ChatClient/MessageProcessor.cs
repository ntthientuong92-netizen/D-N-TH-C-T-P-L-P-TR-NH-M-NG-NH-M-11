using System;
using SharedLibrary;

namespace ChatClient
{
    public static class MessageProcessor
    {
        public static MessagePacket CreateReplyPacket(string sender, string content,
                                                      string replyToContent, string avatarBase64)
        {
            return new MessagePacket
            {
                Type = PacketType.Reply,
                Sender = sender,
                Receiver = "All",
                Content = content,
                ReplyToContent = replyToContent,
                AvatarBase64 = avatarBase64
            };
        }

        public static MessagePacket CreateForwardPacket(string sender, string originalContent,
                                                        string avatarBase64)
        {
            return new MessagePacket
            {
                Type = PacketType.Forward,
                Sender = sender,
                Receiver = "All",
                Content = originalContent,
                AvatarBase64 = avatarBase64
            };
        }

        public static string FormatMessageDisplay(MessagePacket packet)
        {
            string display = "";

            if (packet.Type == PacketType.Reply && !string.IsNullOrEmpty(packet.ReplyToContent))
            {
                display += $"   ↳ Trả lời: \"{packet.ReplyToContent}\"\r\n";
            }

            if (packet.Type == PacketType.Forward)
            {
                display += $"[Chuyển tiếp từ {packet.Sender}]: {packet.Content}";
            }
            else
            {
                display += $"{packet.Sender}: {packet.Content}";
            }

            return display;
        }
    }
}
