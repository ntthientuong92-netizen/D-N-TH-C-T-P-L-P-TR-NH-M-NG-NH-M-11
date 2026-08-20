using SharedLibrary;

namespace ChatClient
{
    public static class MessageProcessor
    {
        public static string FormatMessageDisplay(MessagePacket packet)
        {
            string display = "";
            if (!string.IsNullOrEmpty(packet.ReplyToContent))
            {
                display += $"[Replying to: \"{packet.ReplyToContent}\"]\r\n";
            }

            if (packet.Type == PacketType.Forward)
            {
                display += $"[Forwarded from {packet.Sender}]: {packet.Content}";
            }
            else
            {
                display += $"{packet.Sender}: {packet.Content}";
            }

            return display;
        }
    }
}