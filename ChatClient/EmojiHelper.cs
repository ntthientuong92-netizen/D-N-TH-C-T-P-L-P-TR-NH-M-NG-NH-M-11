using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatClient
{
    public static class EmojiHelper
    {
        private static readonly Dictionary<string, string> Smileys = new Dictionary<string, string>
        {
            { ":)", "😊" },
            { ":D", "😁" }, 
            { ":'(", "😢" }, 
            { "B-)", "😎" },
            { ":*", "😘" }, 
            { "-_-", "😑" }, 
            { ":p", "😛" }, 
            { ":O", "😲" },
            { ";)", "😉" }, 
            { ">_<", "😫" }, 
            { "T_T", "😭" }
        };

        private static readonly Dictionary<string, string> SymbolsAndOthers = new Dictionary<string, string>
        {
            { "<3", "❤️" }, 
            { "(y)", "👍" }, 
            { "(n)", "👎" }, 
            { "(ok)", "👌" },
            { "(clap)", "👏" }, 
            { "(fire)", "🔥" }, 
            { "(party)", "🎉" }, 
            { "(pray)", "🙏" },
            { "(star)", "✨" }, 
            { "(coffee)", "☕" }
        };

        private static readonly Dictionary<string, string> AllEmojis = 
            Smileys.Concat(SymbolsAndOthers).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        private static readonly Random rand = new Random();

        // Danh sách emoji nhanh 
        public static List<string> GetQuickEmojis()
        {
            return Smileys.Values.Take(5).Concat(SymbolsAndOthers.Values.Take(5)).ToList();
        }

        public static string GetRandomGreetingEmoji()
        {
            var greetings = new string[] { "👋", "🤝", "🎉", "✨", "😊", "🥳" };
            return greetings[rand.Next(greetings.Length)];
        }

        public static string ParseEmojisFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string parsedText = text;
            foreach (var mapping in AllEmojis)
            {
            
                string pattern = Regex.Escape(mapping.Key);
                
                if (Regex.IsMatch(mapping.Key, @"^\w+$"))
                {
                    parsedText = Regex.Replace(parsedText, $@"\b{pattern}\b", mapping.Value, RegexOptions.IgnoreCase);
                }
                else
                {
                    parsedText = parsedText.Replace(mapping.Key, mapping.Value);
                }
            }
            return parsedText;
        }
    }
}
