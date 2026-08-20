using System.Drawing;

namespace ChatClient
{
    public class ContactItem
    {
        public string Username { get; set; }
        public Image Avatar { get; set; }
        public bool IsOnline { get; set; }
    }
}