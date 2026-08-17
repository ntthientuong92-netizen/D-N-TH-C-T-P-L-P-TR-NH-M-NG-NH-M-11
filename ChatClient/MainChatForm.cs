using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SharedLibrary;

namespace ChatClient
{
    public class MainChatForm : Form
    {
        private ChatController chatController;
        private string username;
        private string avatarBase64 = "";
        private string lastSelectedMessage = "";

        private TextBox txtServerIp;
        private TextBox txtUsername;
        private Button btnConnect;
        private PictureBox picAvatar;
        private Button btnSelectAvatar;
        private TextBox txtChatBox;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnReply;
        private Button btnForward;
        private FlowLayoutPanel panelEmojis;

        public MainChatForm()
        {
            InitializeComponentCustom();

            chatController = new ChatController();
            chatController.OnMessageReceived += ChatController_OnMessageReceived;
            chatController.OnDisconnected += ChatController_OnDisconnected;
        }

        private void InitializeComponentCustom()
        {
            this.Text = "UDM_08 - Chat TCP Client-Server";
            this.Size = new Size(820, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblIp = new Label() { Text = "Server IP:", Location = new Point(12, 15), Width = 65 };
            txtServerIp = new TextBox() { Text = "127.0.0.1", Location = new Point(80, 12), Width = 100 };
            
            Label lblUser = new Label() { Text = "Tên:", Location = new Point(190, 15), Width = 35 };
            txtUsername = new TextBox() { Text = "User_" + new Random().Next(100, 999), Location = new Point(225, 12), Width = 110 };

            btnConnect = new Button() { Text = "Kết nối", Location = new Point(345, 10), Width = 80, Height = 25 };
            btnConnect.Click += BtnConnect_Click;

            picAvatar = new PictureBox() { Location = new Point(440, 8), Size = new Size(35, 35), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            btnSelectAvatar = new Button() { Text = "Avatar", Location = new Point(480, 10), Width = 75, Height = 25 };
            btnSelectAvatar.Click += BtnSelectAvatar_Click;

            txtChatBox = new TextBox() { Multiline = true, Location = new Point(12, 55), Size = new Size(780, 360), ScrollBars = ScrollBars.Vertical, ReadOnly = true };

            panelEmojis = new FlowLayoutPanel() { Location = new Point(12, 425), Size = new Size(780, 40) };
            foreach (var emoji in EmojiHelper.GetQuickEmojis())
            {
                Button btnEmoji = new Button() { Text = emoji, Size = new Size(38, 32) };
                btnEmoji.Click += (s, e) => { txtMessage.Text += emoji; txtMessage.Focus(); };
                panelEmojis.Controls.Add(btnEmoji);
            }

            txtMessage = new TextBox() { Location = new Point(12, 480), Size = new Size(490, 30) };
            
            btnSend = new Button() { Text = "Gửi", Location = new Point(515, 478), Size = new Size(80, 32) };
            btnSend.Click += BtnSend_Click;

            btnReply = new Button() { Text = "Reply", Location = new Point(605, 478), Size = new Size(90, 32) };
            btnReply.Click += BtnReply_Click;

            btnForward = new Button() { Text = "Forward", Location = new Point(702, 478), Size = new Size(90, 32) };
            btnForward.Click += BtnForward_Click;

            this.Controls.Add(lblIp);
            this.Controls.Add(txtServerIp);
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUsername);
            this.Controls.Add(btnConnect);
            this.Controls.Add(picAvatar);
            this.Controls.Add(btnSelectAvatar);
            this.Controls.Add(txtChatBox);
            this.Controls.Add(panelEmojis);
            this.Controls.Add(txtMessage);
            this.Controls.Add(btnSend);
            this.Controls.Add(btnReply);
            this.Controls.Add(btnForward);
        }

        private void BtnSelectAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picAvatar.Image = Image.FromFile(ofd.FileName);
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    avatarBase64 = Convert.ToBase64String(bytes);
                }
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            username = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên người dùng!");
                return;
            }

            bool success = chatController.Connect(txtServerIp.Text.Trim(), 8888, username, avatarBase64);
            if (success)
            {
                MessageBox.Show("Kết nối thành công tới Server!");
                btnConnect.Enabled = false;
                txtUsername.Enabled = false;
                txtServerIp.Enabled = false;
            }
            else
            {
                MessageBox.Show("Không thể kết nối đến Server!");
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            MessagePacket packet = new MessagePacket
            {
                Type = PacketType.Chat,
                Sender = username,
                Receiver = "All",
                Content = content,
                AvatarBase64 = avatarBase64
            };

            chatController.SendMessage(packet);
            txtChatBox.AppendText($"[Tôi]: {content}\r\n");
            lastSelectedMessage = content;
            txtMessage.Clear();
        }

        private void BtnReply_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            MessagePacket packet = new MessagePacket
            {
                Type = PacketType.Reply,
                Sender = username,
                Receiver = "All",
                Content = content,
                ReplyToContent = lastSelectedMessage,
                AvatarBase64 = avatarBase64
            };

            chatController.SendMessage(packet);
            txtChatBox.AppendText($"[Tôi (Reply \"{lastSelectedMessage}\")]: {content}\r\n");
            txtMessage.Clear();
        }

        private void BtnForward_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastSelectedMessage))
            {
                MessageBox.Show("Chưa có nội dung tin nhắn nào để forward!");
                return;
            }

            MessagePacket packet = new MessagePacket
            {
                Type = PacketType.Forward,
                Sender = username,
                Receiver = "All",
                Content = lastSelectedMessage,
                AvatarBase64 = avatarBase64
            };

            chatController.SendMessage(packet);
            txtChatBox.AppendText($"[Tôi (Forward)]: {lastSelectedMessage}\r\n");
        }

        private void ChatController_OnMessageReceived(MessagePacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<MessagePacket>(ChatController_OnMessageReceived), packet);
                return;
            }

            string formatted = MessageProcessor.FormatMessageDisplay(packet);
            txtChatBox.AppendText($"{formatted}\r\n");
            lastSelectedMessage = packet.Content;
        }

        private void ChatController_OnDisconnected()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ChatController_OnDisconnected));
                return;
            }
            MessageBox.Show("Đã mất kết nối với Server!");
            btnConnect.Enabled = true;
            txtUsername.Enabled = true;
            txtServerIp.Enabled = true;
        }
    }
}