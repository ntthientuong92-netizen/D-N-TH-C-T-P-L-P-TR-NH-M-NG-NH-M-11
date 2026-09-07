using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatClient
{
    partial class MainChatForm
    {
        private System.ComponentModel.IContainer components = null;

        private BorderPanel panelTop;
        private Panel panelLogo;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblIp;
        private RoundedInput ipInput;
        private TextBox txtServerIp;
        private Label lblUser;
        private RoundedInput nameInput;
        private TextBox txtUsername;
        private ModernButton btnConnect;
        private PictureBox picAvatar;
        private ModernButton btnSelectAvatar;
        private Label lblStatus;

        private BorderPanel panelContacts;
        private Label lblContactsHeader;
        private FlowLayoutPanel flowContacts;

        private BorderPanel panelChat;
        private Label lblChatHint;

        private BorderPanel panelBottom;
        private FlowLayoutPanel panelEmojis;
        private RoundedInput msgInput;
        private TextBox txtMessage;
        private ModernButton btnSend;
        private ModernButton btnReply;
        private ModernButton btnForward;

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

            // ===== Thanh trên: logo + kết nối server =====
            panelTop = new BorderPanel { Edge = BorderEdge.Bottom };
            panelLogo = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblIp = new Label();
            ipInput = new RoundedInput(132, 34);
            txtServerIp = ipInput.Inner;
            lblUser = new Label();
            nameInput = new RoundedInput(150, 34);
            txtUsername = nameInput.Inner;
            btnConnect = new ModernButton();
            picAvatar = new PictureBox();
            btnSelectAvatar = new ModernButton();
            lblStatus = new Label();

            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 68;
            panelTop.BackColor = UiTheme.WindowBg;

            panelLogo.Location = new Point(16, 16);
            panelLogo.Size = new Size(36, 36);
            panelLogo.Paint += PanelLogo_Paint;

            lblTitle.Text = "Netizen Chat";
            lblTitle.Location = new Point(62, 15);
            lblTitle.Size = new Size(160, 20);
            lblTitle.Font = UiTheme.Font(10.5f, FontStyle.Bold);
            lblTitle.ForeColor = UiTheme.TextPrimary;

            lblSubtitle.Text = "UDM_08 · TCP Client–Server";
            lblSubtitle.Location = new Point(62, 36);
            lblSubtitle.Size = new Size(180, 15);
            lblSubtitle.Font = UiTheme.Font(7.75f);
            lblSubtitle.ForeColor = UiTheme.TextSecondary;

            lblIp.Text = "MÁY CHỦ";
            lblIp.Location = new Point(268, 14);
            lblIp.Size = new Size(132, 13);
            lblIp.Font = UiTheme.Font(7.5f, FontStyle.Bold);
            lblIp.ForeColor = UiTheme.TextSecondary;

            ipInput.Location = new Point(268, 30);
            txtServerIp.Text = "127.0.0.1";

            lblUser.Text = "TÊN CỦA BẠN";
            lblUser.Location = new Point(416, 14);
            lblUser.Size = new Size(150, 13);
            lblUser.Font = UiTheme.Font(7.5f, FontStyle.Bold);
            lblUser.ForeColor = UiTheme.TextSecondary;

            nameInput.Location = new Point(416, 30);

            btnConnect.Text = "Kết nối";
            btnConnect.Primary = true;
            btnConnect.Location = new Point(582, 30);
            btnConnect.Size = new Size(104, 34);
            btnConnect.Font = UiTheme.Font(9.5f, FontStyle.Bold);

            picAvatar.Location = new Point(702, 29);
            picAvatar.Size = new Size(36, 36);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.Paint += PicAvatar_Paint;

            btnSelectAvatar.Text = "Đổi ảnh";
            btnSelectAvatar.Primary = false;
            btnSelectAvatar.Location = new Point(746, 30);
            btnSelectAvatar.Size = new Size(84, 34);
            btnSelectAvatar.Font = UiTheme.Font(9f);

            lblStatus.Text = "◌  Chưa kết nối";
            lblStatus.Location = new Point(860, 37);
            lblStatus.Size = new Size(140, 20);
            lblStatus.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            lblStatus.Font = UiTheme.Font(9f, FontStyle.Bold);
            lblStatus.ForeColor = UiTheme.TextSecondary;

            panelTop.Controls.AddRange(new Control[]
            {
                lblIp, ipInput, lblUser, nameInput, btnConnect,
                picAvatar, btnSelectAvatar, lblStatus, panelLogo, lblTitle, lblSubtitle
            });

            // ===== Danh sách liên hệ bên trái =====
            panelContacts = new BorderPanel { Edge = BorderEdge.Right };
            lblContactsHeader = new Label();
            flowContacts = new FlowLayoutPanel();

            panelContacts.Dock = DockStyle.Left;
            panelContacts.Width = 244;
            panelContacts.BackColor = UiTheme.SidebarBg;

            lblContactsHeader.Dock = DockStyle.Top;
            lblContactsHeader.Height = 46;
            lblContactsHeader.Text = "  THÀNH VIÊN";
            lblContactsHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblContactsHeader.Font = UiTheme.Font(9f, FontStyle.Bold);
            lblContactsHeader.ForeColor = UiTheme.TextSecondary;
            lblContactsHeader.BackColor = Color.FromArgb(248, 250, 252);

            flowContacts.Dock = DockStyle.Fill;
            flowContacts.AutoScroll = true;
            flowContacts.FlowDirection = FlowDirection.TopDown;
            flowContacts.WrapContents = false;
            flowContacts.Padding = new Padding(8, 8, 8, 8);
            flowContacts.BackColor = UiTheme.SidebarBg;

            panelContacts.Controls.Add(flowContacts);
            panelContacts.Controls.Add(lblContactsHeader);

            // ===== Khu vực chat ở giữa =====
            panelChat = new BorderPanel();
            panelChat.Dock = DockStyle.Fill;
            panelChat.AutoScroll = true;
            panelChat.BackColor = UiTheme.ChatBg;

            lblChatHint = new Label();
            lblChatHint.Text = "Kết nối đến server để bắt đầu trò chuyện 👋";
            lblChatHint.AutoSize = false;
            lblChatHint.Size = new Size(420, 24);
            lblChatHint.TextAlign = ContentAlignment.MiddleCenter;
            lblChatHint.Font = UiTheme.Font(10f);
            lblChatHint.ForeColor = UiTheme.TextSecondary;
            lblChatHint.BackColor = UiTheme.ChatBg;
            lblChatHint.Anchor = AnchorStyles.None;
            lblChatHint.Location = new Point(250, 60);

            // ===== Emoji + ô nhập tin nhắn phía dưới =====
            panelBottom = new BorderPanel { Edge = BorderEdge.Top };
            panelEmojis = new FlowLayoutPanel();
            msgInput = new RoundedInput(560, 38);
            txtMessage = msgInput.Inner;
            btnSend = new ModernButton();
            btnReply = new ModernButton();
            btnForward = new ModernButton();

            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Height = 104;
            panelBottom.BackColor = UiTheme.WindowBg;

            panelEmojis.Location = new Point(16, 10);
            panelEmojis.Size = new Size(988, 34);
            panelEmojis.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panelEmojis.WrapContents = false;

            btnSend.Text = "Gửi  ➤";
            btnSend.Primary = true;
            btnSend.Location = new Point(894, 54);
            btnSend.Size = new Size(104, 38);
            btnSend.Font = UiTheme.Font(9.5f, FontStyle.Bold);
            btnSend.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            btnForward.Text = "Chuyển tiếp";
            btnForward.Primary = false;
            btnForward.Location = new Point(794, 54);
            btnForward.Size = new Size(94, 38);
            btnForward.Font = UiTheme.Font(9f);
            btnForward.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            btnReply.Text = "Trả lời";
            btnReply.Primary = false;
            btnReply.Location = new Point(700, 54);
            btnReply.Size = new Size(88, 38);
            btnReply.Font = UiTheme.Font(9f);
            btnReply.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            msgInput.Location = new Point(16, 54);
            msgInput.Size = new Size(678, 38);
            msgInput.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            panelBottom.Controls.AddRange(new Control[]
            {
                panelEmojis, msgInput, btnSend, btnReply, btnForward
            });

            // ===== Form chính =====
            this.Text = "UDM_08 · Netizen Chat (TCP Client–Server)";
            this.Size = new Size(1020, 700);
            this.MinimumSize = new Size(940, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = UiTheme.Font(9.5f);
            this.BackColor = UiTheme.WindowBg;

            panelChat.Controls.Add(lblChatHint);

            this.Controls.Add(panelChat);
            this.Controls.Add(panelContacts);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
        }

        private static readonly Font LogoFont = new Font("Segoe UI Emoji", 13.5f);

        private void PanelLogo_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UiTheme.RoundedPath(new Rectangle(0, 0, 35, 35), 10))
            using (SolidBrush brush = new SolidBrush(UiTheme.Accent))
            {
                e.Graphics.FillPath(brush, path);
            }
            TextRenderer.DrawText(e.Graphics, "💬", LogoFont,
                new Rectangle(0, 0, 36, 36), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void PicAvatar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            AvatarRenderer.DrawCircular(e.Graphics, myAvatar, username, new Rectangle(0, 0, 36, 36), isConnectedToServer);
        }
    }
}
