using System.Collections.Generic;

namespace ChatClient
{
    public static class EmojiHelper
    {
        public static List<string> GetQuickEmojis()
        {
            return new List<string> { "😊", "😂", "❤️", "👍", "🔥", "😢", "😎", "🎉", "🙏", "✨" };
        }
    }
}