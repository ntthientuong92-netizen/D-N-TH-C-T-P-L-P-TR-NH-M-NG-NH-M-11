using System;

namespace ChatClient
{
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public string AvatarBase64 { get; set; }
        public string MessageType { get; set; }
        public string OriginalMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}