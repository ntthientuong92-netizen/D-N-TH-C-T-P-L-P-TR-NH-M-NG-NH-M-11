using System.Drawing;
using System.Windows.Forms;

namespace ChatClient
{
    /// <summary>
    /// Phần thiết kế giao diện (Designer) của MainChatForm.
    /// Toàn bộ layout được tạo bằng code GUI: thanh kết nối phía trên,
    /// danh sách liên hệ bên trái, khu vực chat bong bóng ở giữa,
    /// bảng emoji + ô nhập tin nhắn phía dưới.
    /// </summary>
    partial class MainChatForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelTop;
        private Label lblIp;
        private TextBox txtServerIp;
        private Label lblUser;
        private TextBox txtUsername;
        private Button btnConnect;
        private PictureBox picAvatar;
        private Button btnSelectAvatar;
        private Label lblStatus;

        private Panel panelContacts;
        private Label lblContactsHeader;
        private FlowLayoutPanel flowContacts;

        private Panel panelChat;

        private Panel panelBottom;
        private FlowLayoutPanel panelEmojis;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnReply;
        private Button btnForward;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ===== Thanh kết nối phía trên =====
            panelTop = new Panel();
            lblIp = new Label();
            txtServerIp = new TextBox();
            lblUser = new Label();
            txtUsername = new TextBox();
            btnConnect = new Button();
            picAvatar = new PictureBox();
            btnSelectAvatar = new Button();
            lblStatus = new Label();

            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 52;
            panelTop.Padding = new Padding(10, 10, 10, 0);

            lblIp.Text = "Server IP:";
            lblIp.Location = new Point(12, 15);
            lblIp.Width = 62;

            txtServerIp.Text = "127.0.0.1";
            txtServerIp.Location = new Point(76, 12);
            txtServerIp.Width = 100;

            lblUser.Text = "Tên:";
            lblUser.Location = new Point(186, 15);
            lblUser.Width = 36;

            txtUsername.Location = new Point(224, 12);
            txtUsername.Width = 110;

            btnConnect.Text = "Kết nối";
            btnConnect.Location = new Point(344, 10);
            btnConnect.Size = new Size(80, 27);

            picAvatar.Location = new Point(440, 8);
            picAvatar.Size = new Size(36, 36);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.Paint += PicAvatar_Paint;

            btnSelectAvatar.Text = "Chọn Avatar";
            btnSelectAvatar.Location = new Point(482, 10);
            btnSelectAvatar.Size = new Size(95, 27);

            lblStatus.Text = "◌ Chưa kết nối";
            lblStatus.Location = new Point(600, 16);
            lblStatus.Width = 200;
            lblStatus.ForeColor = Color.Gray;

            panelTop.Controls.AddRange(new Control[] { lblIp, txtServerIp, lblUser, txtUsername, btnConnect, picAvatar, btnSelectAvatar, lblStatus });

            // ===== Danh sách liên hệ bên trái =====
            panelContacts = new Panel();
            lblContactsHeader = new Label();
            flowContacts = new FlowLayoutPanel();

            panelContacts.Dock = DockStyle.Left;
            panelContacts.Width = 216;
            panelContacts.BackColor = Color.FromArgb(247, 247, 247);

            lblContactsHeader.Dock = DockStyle.Top;
            lblContactsHeader.Height = 34;
            lblContactsHeader.Text = "  DANH SÁCH LIÊN HỆ";
            lblContactsHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblContactsHeader.Font = new Font("Segoe UI Semibold", 9.75f);
            lblContactsHeader.BackColor = Color.FromArgb(230, 230, 230);

            flowContacts.Dock = DockStyle.Fill;
            flowContacts.AutoScroll = true;
            flowContacts.FlowDirection = FlowDirection.TopDown;
            flowContacts.WrapContents = false;
            flowContacts.Padding = new Padding(4);

            panelContacts.Controls.Add(flowContacts);
            panelContacts.Controls.Add(lblContactsHeader);

            // ===== Khu vực chat ở giữa =====
            panelChat = new Panel();
            panelChat.Dock = DockStyle.Fill;
            panelChat.AutoScroll = true;
            panelChat.BackColor = Color.White;
            panelChat.Padding = new Padding(8);

            // ===== Bảng emoji + ô nhập tin nhắn phía dưới =====
            panelBottom = new Panel();
            panelEmojis = new FlowLayoutPanel();
            txtMessage = new TextBox();
            btnSend = new Button();
            btnReply = new Button();
            btnForward = new Button();

            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Height = 92;

            panelEmojis.Location = new Point(10, 6);
            panelEmojis.Size = new Size(790, 40);

            txtMessage.Location = new Point(10, 52);
            txtMessage.Size = new Size(480, 27);

            btnSend.Text = "Gửi";
            btnSend.Location = new Point(500, 50);
            btnSend.Size = new Size(85, 30);

            btnReply.Text = "Reply";
            btnReply.Location = new Point(592, 50);
            btnReply.Size = new Size(100, 30);

            btnForward.Text = "Forward";
            btnForward.Location = new Point(699, 50);
            btnForward.Size = new Size(100, 30);

            panelBottom.Controls.AddRange(new Control[] { panelEmojis, txtMessage, btnSend, btnReply, btnForward });

            // ===== Form chính =====
            this.Text = "UDM_08 - Chat TCP Client-Server";
            this.Size = new Size(900, 640);
            this.MinimumSize = new Size(760, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);

            this.Controls.Add(panelChat);
            this.Controls.Add(panelContacts);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
        }

        // Vẽ avatar cá nhân hình tròn ở thanh trên
        private void PicAvatar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            AvatarRenderer.DrawCircular(e.Graphics, myAvatar, username, new Rectangle(0, 0, 36, 36), isConnectedToServer);
        }
    }
}
