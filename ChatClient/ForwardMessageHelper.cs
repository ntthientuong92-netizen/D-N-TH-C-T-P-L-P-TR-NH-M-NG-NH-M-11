using System;

namespace ChatClient
{
    public static class ForwardMessageHelper
    {
        public static ChatMessage CreateForwardMessage(
            ChatMessage originalMessage,
            string sender,
            string receiver)
        {
            return new ChatMessage
            {
                Sender = sender,
                Receiver = receiver,
                Content = originalMessage.Content,
                AvatarBase64 = originalMessage.AvatarBase64,
                MessageType = "Forward",
                OriginalMessage = originalMessage.Content,
                OriginalSender = originalMessage.Sender,
                Timestamp = DateTime.Now
            };
        }
    }
}