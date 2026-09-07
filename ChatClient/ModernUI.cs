using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatClient
{
    /// <summary>
    /// Bảng màu và font dùng chung cho toàn bộ giao diện.
    /// </summary>
    public static class UiTheme
    {
        // Màu chủ đạo
        public static readonly Color Accent = Color.FromArgb(43, 108, 233);        // xanh dương hiện đại
        public static readonly Color AccentDark = Color.FromArgb(33, 88, 196);
        public static readonly Color AccentSoft = Color.FromArgb(227, 238, 255);    // nền xanh nhạt (hover/selected)

        // Nền & bề mặt
        public static readonly Color WindowBg = Color.White;
        public static readonly Color ChatBg = Color.FromArgb(236, 240, 245);        // nền khung chat xám xanh nhẹ
        public static readonly Color SidebarBg = Color.White;
        public static readonly Color HoverBg = Color.FromArgb(241, 245, 249);
        public static readonly Color Border = Color.FromArgb(226, 232, 240);

        // Chữ
        public static readonly Color TextPrimary = Color.FromArgb(26, 32, 44);
        public static readonly Color TextSecondary = Color.FromArgb(120, 131, 148);
        public static readonly Color TextOnAccent = Color.White;

        // Trạng thái
        public static readonly Color Online = Color.FromArgb(34, 197, 94);
        public static readonly Color Offline = Color.FromArgb(160, 168, 180);
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);

        // Bubble
        public static readonly Color BubbleMine = Color.FromArgb(43, 108, 233);
        public static readonly Color BubbleOther = Color.White;
        public static readonly Color BubbleOtherBorder = Color.FromArgb(226, 232, 240);
        public static readonly Color QuoteMineBg = Color.FromArgb(33, 88, 196);
        public static readonly Color QuoteOtherBg = Color.FromArgb(241, 245, 249);

        public static Font Font(float size, FontStyle style = FontStyle.Regular)
            => new Font("Segoe UI", size, style);

        public static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            if (bounds.Width <= 0 || bounds.Height <= 0) { path.CloseFigure(); return path; }
            if (d > bounds.Width) d = bounds.Width;
            if (d > bounds.Height) d = bounds.Height;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Đường bo góc với 4 bán kính riêng (dùng để vẽ "đuôi" bubble).</summary>
        public static GraphicsPath AsymmetricPath(Rectangle b, int rTopLeft, int rTopRight, int rBottomRight, int rBottomLeft)
        {
            GraphicsPath p = new GraphicsPath();
            int maxR = Math.Min(b.Width, b.Height) / 2;
            rTopLeft = Math.Min(rTopLeft, maxR); rTopRight = Math.Min(rTopRight, maxR);
            rBottomRight = Math.Min(rBottomRight, maxR); rBottomLeft = Math.Min(rBottomLeft, maxR);
            p.AddArc(b.X, b.Y, rTopLeft * 2, rTopLeft * 2, 180, 90);
            p.AddArc(b.Right - rTopRight * 2, b.Y, rTopRight * 2, rTopRight * 2, 270, 90);
            p.AddArc(b.Right - rBottomRight * 2, b.Bottom - rBottomRight * 2, rBottomRight * 2, rBottomRight * 2, 0, 90);
            p.AddArc(b.X, b.Bottom - rBottomLeft * 2, rBottomLeft * 2, rBottomLeft * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    /// <summary>
    /// Nút phẳng bo góc, tự vẽ hoàn toàn (hover + nhấn + trạng thái vô hiệu).
    /// </summary>
    public class ModernButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CornerRadius { get; set; } = 8;

        /// <summary>true = nút màu chủ đạo (nền xanh, chữ trắng); false = nút ghost (nền xám nhạt, chữ đậm).</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Primary { get; set; } = true;

        private bool hovered = false;
        private bool pressed = false;

        public ModernButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            TabStop = false;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { pressed = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { pressed = false; Invalidate(); base.OnMouseUp(mevent); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color back, fore;

            if (!Enabled)
            {
                back = Color.FromArgb(225, 229, 236);
                fore = Color.FromArgb(160, 168, 180);
            }
            else if (Primary)
            {
                back = pressed ? UiTheme.AccentDark : (hovered ? UiTheme.AccentDark : UiTheme.Accent);
                fore = UiTheme.TextOnAccent;
            }
            else
            {
                back = pressed ? Color.FromArgb(224, 231, 241) : (hovered ? UiTheme.HoverBg : Color.FromArgb(244, 247, 250));
                fore = Enabled ? UiTheme.TextPrimary : UiTheme.TextSecondary;
            }

            using (GraphicsPath path = UiTheme.RoundedPath(rect, CornerRadius))
            using (SolidBrush brush = new SolidBrush(back))
            {
                e.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(e.Graphics, Text, Font,
                new Rectangle(0, -1, Width, Height), fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>
    /// Panel có buffer kép, tự vẽ một đường viền mảnh ở cạnh chỉ định.
    /// </summary>
    public class BorderPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BorderEdge Edge { get; set; } = BorderEdge.None;

        public BorderPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Edge == BorderEdge.None) return;
            using Pen pen = new Pen(UiTheme.Border);
            switch (Edge)
            {
                case BorderEdge.Top: e.Graphics.DrawLine(pen, 0, 0, Width - 1, 0); break;
                case BorderEdge.Bottom: e.Graphics.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1); break;
                case BorderEdge.Right: e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1); break;
                case BorderEdge.Left: e.Graphics.DrawLine(pen, 0, 0, 0, Height - 1); break;
            }
        }
    }

    public enum BorderEdge { None, Top, Bottom, Right, Left }

    /// <summary>
    /// Khung bo góc bọc quanh một TextBox không viền, tạo ô nhập hiện đại.
    /// </summary>
    public class RoundedInput : Panel
    {
        public TextBox Inner { get; }

        public RoundedInput(int width, int height)
        {
            BackColor = Color.White;
            DoubleBuffered = true;

            Inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Multiline = false,
                Font = UiTheme.Font(9.75f),
                ForeColor = UiTheme.TextPrimary,
                Width = width - 22,
                Location = new Point(11, (height - 15) / 2 + 1),
                BackColor = Color.White
            };
            Controls.Add(Inner);
            Paint += RoundedInput_Paint;

            // Đặt kích thước sau cùng để Inner đã tồn tại khi OnSizeChanged kích hoạt
            Size = new Size(width, height);
        }

        private void RoundedInput_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiTheme.RoundedPath(rect, 9))
            {
                using Pen pen = new Pen(Inner.Enabled && Focused ? UiTheme.Accent : UiTheme.Border, 1.6f);
                e.Graphics.DrawPath(pen, path);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Inner == null || IsDisposed) return;
            Inner.Width = Math.Max(Width - 22, 40);
            Inner.Location = new Point(11, Math.Max(0, (Height - Inner.PreferredHeight) / 2));
            Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control == Inner)
            {
                Inner.GotFocus += (_, _) => Invalidate();
                Inner.LostFocus += (_, _) => Invalidate();
                Inner.EnabledChanged += (_, _) => Invalidate();
            }
        }
    }
}
