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
                // FIX #5: Copy bytes to a new array to avoid stream disposal issues
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
                Color.FromArgb(70, 130, 180), Color.FromArgb(46, 139, 87), Color.FromArgb(205, 92, 92),
                Color.FromArgb(147, 112, 219), Color.FromArgb(210, 140, 60)
            };
            int hash = 0;
            foreach (char c in username ?? "") unchecked { hash += c; }
            hash = Math.Abs(hash);
            return palette[hash % palette.Length];
        }

        // FIX #4: Cache circular avatar to avoid memory leak
        public static Image GetCircularAvatar(Image avatar, string username, int size, bool isOnline)
        {
            string cacheKey = $"{username}_{size}_{isOnline}_{(avatar != null ? avatar.GetHashCode() : 0)}";
            if (_circularCache.TryGetValue(cacheKey, out Image cached))
                return cached;

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                DrawCircularInternal(g, avatar, username, new Rectangle(0, 0, size, size), isOnline);
            }
            _circularCache[cacheKey] = bmp;
            return bmp;
        }

        public static void DrawCircular(Graphics g, Image avatar, string username, Rectangle bounds, bool isOnline)
        {
            DrawCircularInternal(g, avatar, username, bounds, isOnline);
        }

        private static void DrawCircularInternal(Graphics g, Image avatar, string username, Rectangle bounds, bool isOnline)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, bounds.Width - 1, bounds.Height - 1);
                g.SetClip(path);

                if (avatar != null)
                {
                    g.DrawImage(avatar, new Rectangle(0, 0, bounds.Width, bounds.Height));
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(PickColor(username)))
                    {
                        g.FillRectangle(b, 0, 0, bounds.Width, bounds.Height);
                    }
                    string initial = string.IsNullOrEmpty(username) ? "?" : username.Substring(0, 1).ToUpper();
                    SizeF sz = g.MeasureString(initial, InitialFont);
                    g.DrawString(initial, InitialFont, Brushes.White,
                        (bounds.Width - sz.Width) / 2f, (bounds.Height - sz.Height) / 2f);
                }
            }

            using (Pen ring = new Pen(isOnline ? Color.FromArgb(46, 160, 67) : Color.Gray, 2.5f))
            {
                g.DrawEllipse(ring, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            }
        }
    }

    public class ContactControl : UserControl
    {
        private const int AvatarSize = 36;
        private ContactItem item;
        private Image _cachedAvatar;

        public ContactControl()
        {
            Width = 200;
            Height = 46;
            DoubleBuffered = true;
            Paint += ContactControl_Paint;
        }

        public void Bind(ContactItem contact)
        {
            item = contact;
            // Cache the circular avatar to avoid recreating it every paint
            _cachedAvatar = AvatarRenderer.GetCircularAvatar(item.Avatar, item.Username, AvatarSize, item.IsOnline);
            Invalidate();
        }

        public ContactItem GetItem() => item;

        private void ContactControl_Paint(object sender, PaintEventArgs e)
        {
            if (item == null || _cachedAvatar == null) return;

            Rectangle avatarBounds = new Rectangle(8, 5, AvatarSize, AvatarSize);
            e.Graphics.DrawImage(_cachedAvatar, avatarBounds.Location);

            using (Font nameFont = new Font("Segoe UI Semibold", 10f))
            {
                e.Graphics.DrawString(item.Username, nameFont, Brushes.Black, avatarBounds.Right + 8, 5);
            }

            string status = item.IsOnline ? "● Online" : "○ Offline";
            using (Font statusFont = new Font("Segoe UI", 8.5f))
            using (SolidBrush brush = new SolidBrush(item.IsOnline ? Color.FromArgb(46, 160, 67) : Color.Gray))
            {
                e.Graphics.DrawString(status, statusFont, brush, avatarBounds.Right + 8, 26);
            }
        }
    }

    public class MessageBubble : Panel
    {
        public MessagePacket Packet { get; }
        public bool IsMine { get; }

        private readonly Image senderAvatar;
        private bool selected;
        private readonly Size bodySize;
        private readonly Size headerSize;
        private readonly int quoteHeight;
        private readonly Rectangle bubbleBounds;
        private readonly Rectangle avatarBounds;

        private const int AvatarBox = 34;
        private const int BubblePad = 10;
        private static readonly Font HeaderFont = new Font("Segoe UI Semibold", 9f);
        private static readonly Font BodyFont = new Font("Segoe UI", 10f);
        private static readonly Font QuoteFont = new Font("Segoe UI", 8.5f, FontStyle.Italic);

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
            string quote = string.IsNullOrEmpty(packet.ReplyToContent) ? "" : "↩ " + packet.ReplyToContent;

            int maxBubbleWidth = Math.Min(maxWidth - (IsMine ? AvatarBox + 30 : AvatarBox + 34), 430);

            headerSize = TextRenderer.MeasureText(HeaderText, HeaderFont,
                new Size(maxBubbleWidth, int.MaxValue), TextFormatFlags.WordBreak);
            bodySize = TextRenderer.MeasureText(body, BodyFont,
                new Size(Math.Max(maxBubbleWidth - 2 * BubblePad, 60), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            quoteHeight = 0;
            if (quote.Length > 0)
            {
                Size q = TextRenderer.MeasureText(quote, QuoteFont,
                    new Size(Math.Max(maxBubbleWidth - 2 * BubblePad, 60), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                quoteHeight = q.Height + 6;
            }

            int contentWidth = Math.Max(headerSize.Width, Math.Max(bodySize.Width, quote.Length > 0 ? maxBubbleWidth - 2 * BubblePad : 0));
            int bubbleWidth = Math.Min(contentWidth + 2 * BubblePad, maxBubbleWidth);
            int bubbleHeight = 8 + headerSize.Height + quoteHeight + bodySize.Height + 10;

            if (IsMine)
            {
                Width = bubbleWidth + AvatarBox + 16;
                avatarBounds = new Rectangle(Width - AvatarBox - 6, 8, AvatarBox, AvatarBox);
                bubbleBounds = new Rectangle(4, 2, bubbleWidth, bubbleHeight);
            }
            else
            {
                Width = bubbleWidth + AvatarBox + 16;
                avatarBounds = new Rectangle(6, 8, AvatarBox, AvatarBox);
                bubbleBounds = new Rectangle(AvatarBox + 10, 2, bubbleWidth, bubbleHeight);
            }

            Height = bubbleHeight + 8;
            Paint += MessageBubble_Paint;
        }

        private string HeaderText
        {
            get
            {
                string tag = Packet.Type == PacketType.Forward ? " (Chuyển tiếp)" : "";
                return $"{Packet.Sender}{tag}  •  {Packet.Timestamp:HH:mm}";
            }
        }

        private void MessageBubble_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Image avatar = senderAvatar ?? AvatarRenderer.DefaultAvatar(Packet.Sender);
            AvatarRenderer.DrawCircular(e.Graphics, avatar, Packet.Sender, avatarBounds, true);

            Color backColor = IsMine ? Color.FromArgb(0, 120, 215) : Color.FromArgb(238, 238, 238);
            Color textColor = IsMine ? Color.White : Color.FromArgb(30, 30, 30);
            Color subColor = IsMine ? Color.FromArgb(200, 224, 250) : Color.Gray;

            using (GraphicsPath path = RoundedRect(bubbleBounds, 10))
            using (SolidBrush back = new SolidBrush(backColor))
            {
                e.Graphics.FillPath(back, path);
            }

            if (selected)
            {
                using (Pen pen = new Pen(Color.FromArgb(255, 140, 0), 2f))
                {
                    e.Graphics.DrawPath(pen, RoundedRect(bubbleBounds, 10));
                }
            }

            Point textOrigin = new Point(bubbleBounds.X + BubblePad, bubbleBounds.Y + 5);
            TextRenderer.DrawText(e.Graphics, HeaderText, HeaderFont,
                new Rectangle(textOrigin, headerSize), subColor, TextFormatFlags.Default);

            int y = textOrigin.Y + headerSize.Height;
            if (quoteHeight > 0)
            {
                string quote = "↩ " + Packet.ReplyToContent;
                TextRenderer.DrawText(e.Graphics, quote, QuoteFont,
                    new Rectangle(new Point(textOrigin.X, y), new Size(bubbleBounds.Width - 2 * BubblePad, quoteHeight)),
                    subColor, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                y += quoteHeight;
            }

            TextRenderer.DrawText(e.Graphics, Packet.Content ?? "", BodyFont,
                new Rectangle(new Point(textOrigin.X, y), new Size(bubbleBounds.Width - 2 * BubblePad, bodySize.Height + 4)),
                textColor, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
