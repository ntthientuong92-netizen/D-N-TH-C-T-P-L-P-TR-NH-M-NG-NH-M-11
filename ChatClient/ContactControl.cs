using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using SharedLibrary;

namespace ChatClient
{
    public class ContactItem
    {
        public string Username { get; set; }
        public Image Avatar { get; set; }
        public bool IsOnline { get; set; }
    }

    public static class AvatarRenderer
    {
        private static readonly Font InitialFont = new Font("Segoe UI", 14f, FontStyle.Bold);
        private static readonly Dictionary<string, Image> _avatarCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Image> _circularCache = new Dictionary<string, Image>();

        public static Image FromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                // Copy bytes ra mảng mới để tránh lỗi stream bị dispose (FIX cũ giữ nguyên)
                byte[] safeBytes = new byte[bytes.Length];
                Buffer.BlockCopy(bytes, 0, safeBytes, 0, bytes.Length);
                return new Bitmap(new MemoryStream(safeBytes));
            }
            catch
            {
                return null;
            }
        }

        public static Image DefaultAvatar(string username)
        {
            string initial = string.IsNullOrEmpty(username) ? "?" : username.Substring(0, 1).ToUpper();
            string cacheKey = "default_" + initial;

            if (_avatarCache.TryGetValue(cacheKey, out Image cached))
                return cached;

            Bitmap bmp = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(PickColor(username)))
                {
                    g.FillEllipse(brush, 0, 0, 47, 47);
                }
                SizeF size = g.MeasureString(initial, InitialFont);
                g.DrawString(initial, InitialFont, Brushes.White, (48 - size.Width) / 2f, (48 - size.Height) / 2f);
            }
            _avatarCache[cacheKey] = bmp;
            return bmp;
        }

        public static Color PickColor(string username)
        {
            Color[] palette =
            {
                Color.FromArgb(43, 108, 233), Color.FromArgb(22, 149, 122), Color.FromArgb(214, 92, 74),
                Color.FromArgb(126, 96, 211), Color.FromArgb(206, 132, 46), Color.FromArgb(66, 139, 197)
            };
            int hash = 0;
            foreach (char c in username ?? "") unchecked { hash += c; }
            hash = Math.Abs(hash);
            return palette[hash % palette.Length];
        }

        // Cache avatar tròn để tránh tạo Bitmap mỗi lần vẽ (FIX cũ giữ nguyên)
        public static Image GetCircularAvatar(Image avatar, string username, int size, bool isOnline)
        {
            return GetCircularAvatar(avatar, username, size,
                isOnline ? UiTheme.Online : UiTheme.Offline);
        }

        public static Image GetCircularAvatar(Image avatar, string username, int size, Color? ringColor)
        {
            string cacheKey = $"{username}_{size}_{ringColor?.ToArgb() ?? -1}_{(avatar != null ? avatar.GetHashCode() : 0)}";
            if (_circularCache.TryGetValue(cacheKey, out Image cached))
                return cached;

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                DrawCircularInternal(g, avatar, username, new Rectangle(0, 0, size, size), ringColor);
            }
            _circularCache[cacheKey] = bmp;
            return bmp;
        }

        public static void DrawCircular(Graphics g, Image avatar, string username, Rectangle bounds, bool isOnline)
        {
            DrawCircularInternal(g, avatar, username, bounds, isOnline ? UiTheme.Online : UiTheme.Offline);
        }

        public static void DrawCircular(Graphics g, Image avatar, string username, Rectangle bounds, Color ringColor)
        {
            DrawCircularInternal(g, avatar, username, bounds, ringColor);
        }

        private static void DrawCircularInternal(Graphics g, Image avatar, string username, Rectangle bounds, Color? ringColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                // Clip và vẽ đúng theo vị trí thật của vùng avatar (bounds), không phải gốc (0,0)
                path.AddEllipse(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                g.SetClip(path);

                if (avatar != null)
                {
                    g.DrawImage(avatar, bounds);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(PickColor(username)))
                    {
                        g.FillRectangle(b, bounds);
                    }
                    string initial = string.IsNullOrEmpty(username) ? "?" : username.Substring(0, 1).ToUpper();
                    SizeF sz = g.MeasureString(initial, InitialFont);
                    g.DrawString(initial, InitialFont, Brushes.White,
                        bounds.X + (bounds.Width - sz.Width) / 2f,
                        bounds.Y + (bounds.Height - sz.Height) / 2f);
                }
            }
            g.ResetClip();

            if (ringColor.HasValue)
            {
                using (Pen ring = new Pen(ringColor.Value, 2.5f))
                {
                    g.DrawEllipse(ring, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }
        }
    }

    /// <summary>Một dòng trong danh sách liên hệ: avatar tròn + chấm trạng thái + tên.</summary>
    public class ContactControl : UserControl
    {
        private const int AvatarSize = 36;
        private ContactItem item;
        private Image _cachedAvatar;
        private bool hovered;

        public ContactControl()
        {
            Width = 224;
            Height = 52;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Paint += ContactControl_Paint;
            MouseEnter += (_, _) => { hovered = true; Invalidate(); };
            MouseLeave += (_, _) => { hovered = false; Invalidate(); };
        }

        public void Bind(ContactItem contact)
        {
            item = contact;
            // Avatar tròn không viền (chấm online vẽ riêng để cập nhật trạng thái linh hoạt)
            _cachedAvatar = AvatarRenderer.GetCircularAvatar(item.Avatar, item.Username, AvatarSize, (Color?)null);
            Invalidate();
        }

        public ContactItem GetItem() => item;

        private void ContactControl_Paint(object sender, PaintEventArgs e)
        {
            if (item == null) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (hovered)
            {
                using (GraphicsPath p = UiTheme.RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10))
                using (SolidBrush b = new SolidBrush(UiTheme.HoverBg))
                {
                    e.Graphics.FillPath(b, p);
                }
            }

            // Avatar (nếu chưa kịp cache thì vẽ trực tiếp)
            Rectangle avatarBounds = new Rectangle(10, 8, AvatarSize, AvatarSize);
            Image avatar = _cachedAvatar ?? AvatarRenderer.DefaultAvatar(item.Username);
            e.Graphics.DrawImage(avatar, avatarBounds.Location);

            // Chấm trạng thái online/offline ở góc avatar
            int dotR = 5;
            Point dotCenter = new Point(avatarBounds.Right - dotR - 1, avatarBounds.Bottom - dotR - 1);
            using (SolidBrush ring = new SolidBrush(UiTheme.SidebarBg))
                e.Graphics.FillEllipse(ring, dotCenter.X - dotR - 1.5f, dotCenter.Y - dotR - 1.5f, dotR * 2 + 3, dotR * 2 + 3);
            using (SolidBrush dot = new SolidBrush(item.IsOnline ? UiTheme.Online : UiTheme.Offline))
                e.Graphics.FillEllipse(dot, dotCenter.X - dotR, dotCenter.Y - dotR, dotR * 2, dotR * 2);

            // Tên + trạng thái
            int textX = avatarBounds.Right + 10;
            using (Font nameFont = UiTheme.Font(9.75f, FontStyle.Bold))
            {
                TextRenderer.DrawText(e.Graphics, item.Username, nameFont,
                    new Rectangle(textX, 9, Width - textX - 8, 18), UiTheme.TextPrimary,
                    TextFormatFlags.Default | TextFormatFlags.EndEllipsis);
            }
            using (Font statusFont = UiTheme.Font(8.25f))
            {
                TextRenderer.DrawText(e.Graphics,
                    item.IsOnline ? "Đang hoạt động" : "Ngoại tuyến",
                    statusFont, new Rectangle(textX, 30, Width - textX - 8, 14),
                    item.IsOnline ? UiTheme.Online : UiTheme.TextSecondary,
                    TextFormatFlags.Default | TextFormatFlags.EndEllipsis);
            }
        }
    }

    /// <summary>Bong bóng tin nhắn: bo góc lệch tạo "đuôi", khối trích dẫn reply, header tên + giờ.</summary>
    public class MessageBubble : Panel
    {
        public MessagePacket Packet { get; }
        public bool IsMine { get; }

        private readonly Image senderAvatar;
        private bool selected;
        private readonly Size nameSize;
        private readonly Size timeSize;
        private readonly Size bodySize;
        private readonly Size quoteSize;
        private readonly int quoteBlockHeight;
        private readonly Rectangle bubbleBounds;
        private readonly Rectangle avatarBounds;
        private readonly string nameText;
        private readonly string timeText;

        private const int AvatarBox = 34;
        private const int BubblePad = 12;
        private static readonly Font NameFont = UiTheme.Font(9f, FontStyle.Bold);
        private static readonly Font TimeFont = UiTheme.Font(8f);
        private static readonly Font BodyFont = UiTheme.Font(9.75f);
        private static readonly Font QuoteFont = UiTheme.Font(8.5f, FontStyle.Italic);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get { return selected; }
            set { selected = value; Invalidate(); }
        }

        public MessageBubble(MessagePacket packet, bool isMine, int maxWidth, Image avatar)
        {
            Packet = packet;
            IsMine = isMine;
            senderAvatar = avatar;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            string body = packet.Content ?? "";
            bool hasQuote = !string.IsNullOrEmpty(packet.ReplyToContent);

            int maxBubbleWidth = Math.Min(maxWidth - (AvatarBox + 26), 460);
            if (maxBubbleWidth < 140) maxBubbleWidth = 140;

            nameText = Packet.Type == PacketType.Forward
                ? $"{Packet.Sender} · Chuyển tiếp"
                : Packet.Sender;
            timeText = packet.Timestamp.ToString("HH:mm");

            nameSize = TextRenderer.MeasureText(nameText, NameFont);
            timeSize = TextRenderer.MeasureText(timeText, TimeFont);
            bodySize = TextRenderer.MeasureText(body, BodyFont,
                new Size(maxBubbleWidth - 2 * BubblePad, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            quoteBlockHeight = 0;
            if (hasQuote)
            {
                string quote = "↩  " + packet.ReplyToContent;
                quoteSize = TextRenderer.MeasureText(quote, QuoteFont,
                    new Size(Math.Max(maxBubbleWidth - 2 * BubblePad - 16, 60), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                quoteBlockHeight = quoteSize.Height + 10;
            }
            else
            {
                quoteSize = Size.Empty;
            }

            int headerWidth = nameSize.Width + 8 + timeSize.Width;
            int contentWidth = Math.Max(headerWidth,
                Math.Max(bodySize.Width, hasQuote ? quoteSize.Width + 16 : 0));
            int bubbleWidth = Math.Min(contentWidth + 2 * BubblePad, maxBubbleWidth);
            int bubbleHeight = 8 + nameSize.Height
                             + (hasQuote ? 4 + quoteBlockHeight : 0)
                             + 4 + bodySize.Height + 11;

            if (IsMine)
            {
                Width = bubbleWidth + AvatarBox + 14;
                Height = bubbleHeight + 6;
                bubbleBounds = new Rectangle(2, 3, bubbleWidth, bubbleHeight);
                avatarBounds = new Rectangle(Width - AvatarBox - 2, 6, AvatarBox, AvatarBox);
            }
            else
            {
                Width = bubbleWidth + AvatarBox + 14;
                Height = bubbleHeight + 6;
                bubbleBounds = new Rectangle(AvatarBox + 8, 3, bubbleWidth, bubbleHeight);
                avatarBounds = new Rectangle(4, 6, AvatarBox, AvatarBox);
            }

            Paint += MessageBubble_Paint;
        }

        private void MessageBubble_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Image avatar = senderAvatar ?? AvatarRenderer.DefaultAvatar(Packet.Sender);
            AvatarRenderer.DrawCircular(e.Graphics, avatar, Packet.Sender, avatarBounds,
                Color.FromArgb(224, 230, 238));

            // Đuôi bubble: góc gần avatar bo nhỏ hơn
            GraphicsPath bubblePath = IsMine
                ? UiTheme.AsymmetricPath(bubbleBounds, 14, 14, 4, 14)
                : UiTheme.AsymmetricPath(bubbleBounds, 14, 14, 14, 4);

            using (SolidBrush back = new SolidBrush(IsMine ? UiTheme.BubbleMine : UiTheme.BubbleOther))
            {
                e.Graphics.FillPath(back, bubblePath);
            }
            if (!IsMine)
            {
                using (Pen border = new Pen(UiTheme.BubbleOtherBorder, 1f))
                {
                    e.Graphics.DrawPath(border, bubblePath);
                }
            }
            if (selected)
            {
                using (Pen pen = new Pen(UiTheme.Warning, 2f))
                {
                    e.Graphics.DrawPath(pen, bubblePath);
                }
            }

            Color nameColor = IsMine ? Color.FromArgb(219, 234, 255) : AvatarRenderer.PickColor(Packet.Sender);
            Color timeColor = IsMine ? Color.FromArgb(178, 205, 248) : UiTheme.TextSecondary;
            Color bodyColor = IsMine ? Color.White : UiTheme.TextPrimary;

            int textX = bubbleBounds.X + BubblePad;
            int y = bubbleBounds.Y + 8;

            // Header: tên + giờ
            TextRenderer.DrawText(e.Graphics, nameText, NameFont,
                new Rectangle(textX, y, nameSize.Width + 4, nameSize.Height), nameColor,
                TextFormatFlags.Default | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(e.Graphics, timeText, TimeFont,
                new Rectangle(textX + nameSize.Width + 8, y + 1, timeSize.Width + 4, timeSize.Height), timeColor,
                TextFormatFlags.Default);
            y += nameSize.Height + 4;

            // Khối trích dẫn reply
            if (quoteBlockHeight > 0)
            {
                Rectangle quoteRect = new Rectangle(textX, y, bubbleBounds.Width - 2 * BubblePad, quoteBlockHeight);
                using (GraphicsPath qPath = UiTheme.RoundedPath(quoteRect, 6))
                using (SolidBrush qBg = new SolidBrush(IsMine ? UiTheme.QuoteMineBg : UiTheme.QuoteOtherBg))
                {
                    e.Graphics.FillPath(qBg, qPath);
                }
                using (SolidBrush bar = new SolidBrush(IsMine ? Color.FromArgb(140, 180, 255) : UiTheme.Accent))
                {
                    e.Graphics.FillRectangle(bar, quoteRect.X + 4, quoteRect.Y + 3, 3, quoteRect.Height - 6);
                }
                TextRenderer.DrawText(e.Graphics, "↩  " + Packet.ReplyToContent, QuoteFont,
                    new Rectangle(quoteRect.X + 13, quoteRect.Y + 5, quoteRect.Width - 17, quoteRect.Height - 8),
                    IsMine ? Color.FromArgb(206, 226, 255) : UiTheme.TextSecondary,
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.EndEllipsis);
                y += quoteBlockHeight + 4;
            }

            TextRenderer.DrawText(e.Graphics, Packet.Content ?? "", BodyFont,
                new Rectangle(textX, y, bubbleBounds.Width - 2 * BubblePad, bodySize.Height + 4), bodyColor,
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        }
    }
}
