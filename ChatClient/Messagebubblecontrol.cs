using System.Drawing;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class MessageBubbleControl : Panel
    {
        public MessageBubbleControl()
        {
            InitializeComponent();
        }

        public MessageBubbleControl(string sender, string content, string avatarBase64, bool isMine) : this()
        {
            lblSender.Text = sender;
            lblContent.Text = content;
            BackColor = isMine ? Color.FromArgb(220, 248, 198) : Color.WhiteSmoke;

            if (!string.IsNullOrEmpty(avatarBase64) && AvatarHelper.TryDecodeBase64ToImage(avatarBase64, out Image? img))
            {
                pbAvatar.Image = img;
            }
        }
    }
}