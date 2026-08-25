using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SharedLibrary;

namespace ChatClient
{
    /// <summary>
    /// Form chat chính phía Client (phần logic).
    /// - Thiết kế giao diện nằm ở MainChatForm.Designer.cs
    /// - ContactControl.cs chứa danh sách liên hệ và bong bóng chat (avatar hiển thị trong khu vực chat)
    /// </summary>
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
        private MessageBubble selectedBubble;
        private int nextBubbleY = 8;

        public MainChatForm()
        {
            InitializeComponent();

            txtUsername.Text = "User_" + new Random().Next(100, 999);

            // Bảng chọn emoji nhanh (dữ liệu emoji do EmojiHelper - TV5 - cung cấp)
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

        // ===== Chọn ảnh đại diện từ file, chuyển sang Base64 để gửi kèm gói tin =====
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
                        ResourceCleanup.Track(myAvatar); // TV5: Theo dõi avatar để giải phóng RAM
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

                // Thêm chính mình vào danh sách liên hệ
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

            content = EmojiHelper.ParseEmojisFromText(content); // TV5: Chuyển đổi text thành Emoji bằng Regex

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

            content = EmojiHelper.ParseEmojisFromText(content); // TV5: Chuyển đổi text thành Emoji bằng Regex

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

        // ===== Nhận gói tin từ Server: hiển thị bong bóng chat + cập nhật danh sách liên hệ =====
        private void ChatController_OnMessageReceived(MessagePacket packet)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<MessagePacket>(ChatController_OnMessageReceived), packet);
                return;
            }

            if (packet.Sender == username) return; // Server không gửi lại cho mình, chỉ phòng hờ

            if (packet.Type == PacketType.Login)
            {
                AddSystemNotice($"{packet.Sender} đã tham gia phòng chat.");
            }
            else
            {
                AddBubble(packet, isMine: false);
                lastSelectedMessage = packet.Content;
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

            // Đánh dấu tất cả liên hệ offline
            foreach (ContactItem item in contacts.Values)
            {
                item.IsOnline = false;
            }
            flowContacts.Invalidate(true);
        }

        private void MainChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing = true;
            chatController.Disconnect();
            ResourceCleanup.DisposeAll(); // TV5: Giải phóng toàn bộ tài nguyên khi tắt form
        }

        // ===== Quản lý bong bóng chat trong khu vực chat =====
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

        // Thông báo hệ thống (tham gia phòng...) hiển thị giữa khung chat
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

        // Khi thay đổi kích thước cửa sổ: căn lại tin của mình sát phải
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

        // ===== Quản lý danh sách liên hệ =====
        private void UpdateSelfContact()
        {
            AddOrUpdateContact(username, avatarBase64, myAvatar);
        }

        private void AddOrUpdateContact(string name, string avatarB64, Image avatarImage = null)
        {
            if (string.IsNullOrEmpty(name)) return;

            ContactItem item;
            if (!contacts.TryGetValue(name, out item))
            {
                item = new ContactItem { Username = name, Avatar = avatarImage, IsOnline = true };
                if (item.Avatar == null && !string.IsNullOrEmpty(avatarB64))
                    item.Avatar = AvatarRenderer.FromBase64(avatarB64);
                if (item.Avatar == null)
                    item.Avatar = AvatarRenderer.DefaultAvatar(name);

                contacts[name] = item;

                ContactControl control = new ContactControl();
                control.Bind(item);
                control.Name = "contact_" + name;
                flowContacts.Controls.Add(control);
            }
            else
            {
                item.IsOnline = true;
                Image newAvatar = avatarImage ?? AvatarRenderer.FromBase64(avatarB64);
                if (newAvatar != null) item.Avatar = newAvatar;
                flowContacts.Invalidate(true);
            }
        }

        private Image GetContactAvatar(string name)
        {
            ContactItem item;
            return contacts.TryGetValue(name, out item) ? item.Avatar : null;
        }
    }
}
