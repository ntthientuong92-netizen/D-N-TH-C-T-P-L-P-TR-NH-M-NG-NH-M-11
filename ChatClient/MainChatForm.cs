using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SharedLibrary;

namespace ChatClient
{
    public partial class MainChatForm : Form
    {
        private ChatController chatController;
        private string username = "";
        private string avatarBase64 = "";
        private Image myAvatar;
        private string lastSelectedMessage = "";
        private bool isConnectedToServer = false;
        private bool formClosing = false;

        private readonly Dictionary<string, ContactItem> contacts = new Dictionary<string, ContactItem>();
        private readonly Dictionary<string, ContactControl> contactControls = new Dictionary<string, ContactControl>();
        private MessageBubble selectedBubble;
        private int nextBubbleY = 8;

        public MainChatForm()
        {
            InitializeComponent();

            txtUsername.Text = "User_" + new Random().Next(100, 999);

            foreach (var emoji in EmojiHelper.GetQuickEmojis())
            {
                Button btnEmoji = new Button { Text = emoji, Size = new Size(38, 32) };
                btnEmoji.Click += (s, e) => { txtMessage.Text += emoji; txtMessage.Focus(); };
                panelEmojis.Controls.Add(btnEmoji);
            }

            chatController = new ChatController();
            chatController.OnMessageReceived += ChatController_OnMessageReceived;
            chatController.OnDisconnected += ChatController_OnDisconnected;

            btnConnect.Click += BtnConnect_Click;
            btnSelectAvatar.Click += BtnSelectAvatar_Click;
            btnSend.Click += BtnSend_Click;
            btnReply.Click += BtnReply_Click;
            btnForward.Click += BtnForward_Click;
            this.FormClosing += MainChatForm_FormClosing;
            panelChat.Resize += PanelChat_Resize;
        }

        private void BtnSelectAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    avatarBase64 = Convert.ToBase64String(bytes);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        myAvatar = new Bitmap(ms);
                        ResourceCleanup.Track(myAvatar);
                    }
                    picAvatar.Invalidate();
                    UpdateSelfContact();
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
                isConnectedToServer = true;
                btnConnect.Enabled = false;
                txtUsername.Enabled = false;
                txtServerIp.Enabled = false;
                lblStatus.Text = "● Đã kết nối";
                lblStatus.ForeColor = Color.FromArgb(46, 160, 67);
                picAvatar.Invalidate();

                UpdateSelfContact();
                AddSystemNotice($"Bạn đã tham gia phòng chat với tên \"{username}\".");
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

            content = EmojiHelper.ParseEmojisFromText(content);

            MessagePacket packet = new MessagePacket
            {
                Type = PacketType.Chat,
                Sender = username,
                Receiver = "All",
                Content = content,
                AvatarBase64 = avatarBase64
            };

            chatController.SendMessage(packet);
            AddBubble(packet, isMine: true);
            lastSelectedMessage = content;
            txtMessage.Clear();
        }

        private void BtnReply_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;
            if (string.IsNullOrEmpty(lastSelectedMessage))
            {
                MessageBox.Show("Hãy bấm chọn một tin nhắn trong khung chat để trả lời!");
                return;
            }

            content = EmojiHelper.ParseEmojisFromText(content);

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
            AddBubble(packet, isMine: true);
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
            AddBubble(packet, isMine: true);
        }

        private void ChatController_OnMessageReceived(MessagePacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<MessagePacket>(ChatController_OnMessageReceived), packet);
                return;
            }

            if (packet.Sender == username) return;

            if (packet.Type == PacketType.Login)
            {
                AddSystemNotice($"{packet.Sender} đã tham gia phòng chat.");
            }
            else
            {
                AddBubble(packet, isMine: false);
                // FIX #2: Không tự động update lastSelectedMessage khi nhận tin nhắn
                // lastSelectedMessage = packet.Content;  ← ĐÃ XÓA
            }

            AddOrUpdateContact(packet.Sender, packet.AvatarBase64);
        }

        private void ChatController_OnDisconnected()
        {
            if (formClosing) return;

            if (InvokeRequired)
            {
                Invoke(new Action(ChatController_OnDisconnected));
                return;
            }

            MessageBox.Show("Đã mất kết nối với Server!");
            isConnectedToServer = false;
            btnConnect.Enabled = true;
            txtUsername.Enabled = true;
            txtServerIp.Enabled = true;
            lblStatus.Text = "◌ Chưa kết nối";
            lblStatus.ForeColor = Color.Gray;
            picAvatar.Invalidate();

            foreach (ContactItem item in contacts.Values)
            {
                item.IsOnline = false;
            }
            // FIX #1: Cập nhật tất cả ContactControl thay vì chỉ Invalidate
            foreach (var ctrl in contactControls.Values)
            {
                ctrl.Bind(ctrl.GetItem());
            }
            flowContacts.Invalidate(true);
        }

        private void MainChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
            chatController.Disconnect();
            ResourceCleanup.DisposeAll();
        }

        private void AddBubble(MessagePacket packet, bool isMine)
        {
            Image avatar = isMine ? myAvatar : GetContactAvatar(packet.Sender);
            MessageBubble bubble = new MessageBubble(packet, isMine, panelChat.ClientSize.Width - 24, avatar);

            bubble.Location = new Point(
                isMine ? panelChat.ClientSize.Width - bubble.Width - 30 : 10,
                nextBubbleY);
            bubble.Click += (s, e) => SelectBubble(bubble);

            nextBubbleY += bubble.Height + 8;
            panelChat.Controls.Add(bubble);
            ScrollChatToBottom();
        }

        private void AddSystemNotice(string text)
        {
            Label notice = new Label
            {
                Text = text,
                AutoSize = false,
                Height = 22,
                Width = panelChat.ClientSize.Width - 40,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(20, nextBubbleY)
            };
            nextBubbleY += 28;
            panelChat.Controls.Add(notice);
            ScrollChatToBottom();
        }

        private void SelectBubble(MessageBubble bubble)
        {
            if (selectedBubble != null) selectedBubble.Selected = false;
            selectedBubble = bubble;
            bubble.Selected = true;
            lastSelectedMessage = bubble.Packet.Content;
        }

        private void ScrollChatToBottom()
        {
            panelChat.PerformLayout();
            panelChat.AutoScrollPosition = new Point(0, panelChat.DisplayRectangle.Height);
        }

        private void PanelChat_Resize(object sender, EventArgs e)
        {
            foreach (Control ctrl in panelChat.Controls)
            {
                MessageBubble bubble = ctrl as MessageBubble;
                if (bubble != null && bubble.IsMine)
                {
                    bubble.Left = panelChat.ClientSize.Width - bubble.Width - 30;
                }
                else if (ctrl is Label)
                {
                    ctrl.Width = panelChat.ClientSize.Width - 40;
                }
            }
        }

        private void UpdateSelfContact()
        {
            AddOrUpdateContact(username, avatarBase64, myAvatar);
        }

        private void AddOrUpdateContact(string name, string avatarB64, Image avatarImage = null)
        {
            if (string.IsNullOrEmpty(name)) return;

            if (!contacts.TryGetValue(name, out ContactItem item))
            {
                item = new ContactItem { Username = name, IsOnline = true };
                item.Avatar = avatarImage ?? (!string.IsNullOrEmpty(avatarB64) ? AvatarRenderer.FromBase64(avatarB64) : null);
                if (item.Avatar == null)
                    item.Avatar = AvatarRenderer.DefaultAvatar(name);

                contacts[name] = item;

                ContactControl ctrl = new ContactControl();
                ctrl.Bind(item);
                ctrl.Name = "contact_" + name;
                flowContacts.Controls.Add(ctrl);
                contactControls[name] = ctrl; // FIX #1: Lưu reference để cập nhật sau
            }
            else
            {
                item.IsOnline = true;
                Image newAvatar = avatarImage ?? (!string.IsNullOrEmpty(avatarB64) ? AvatarRenderer.FromBase64(avatarB64) : null);
                if (newAvatar != null) item.Avatar = newAvatar;

                // FIX #1: Gọi Bind để cập nhật UI thay vì chỉ Invalidate
                if (contactControls.TryGetValue(name, out ContactControl ctrl))
                {
                    ctrl.Bind(item);
                }
                else
                {
                    flowContacts.Invalidate(true);
                }
            }
        }

        private Image GetContactAvatar(string name)
        {
            ContactItem item;
            return contacts.TryGetValue(name, out item) ? item.Avatar : null;
        }
    }
}
