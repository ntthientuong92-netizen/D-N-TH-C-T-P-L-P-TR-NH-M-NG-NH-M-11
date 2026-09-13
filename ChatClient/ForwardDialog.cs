using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChatClient
{
    /// <summary>
    /// Hộp thoại chọn nơi chuyển tiếp tin nhắn: cả phòng chat hoặc một người dùng cụ thể.
    /// Kết quả nằm ở <see cref="SelectedTarget"/> ("All" = cả phòng).
    /// </summary>
    public class ForwardDialog : Form
    {
        public const string RoomTarget = "All";

        private readonly ListBox lstTargets;

        /// <summary>Tên người nhận, hoặc "All" nếu chuyển tiếp cho cả phòng.</summary>
        public string SelectedTarget { get; private set; } = RoomTarget;

        public bool ForwardToAll => SelectedTarget == RoomTarget;

        public ForwardDialog(string originalSender, string content, IEnumerable<string> members)
        {
            string originalContent = content ?? string.Empty;

            Text = "Chuyển tiếp tin nhắn";
            Size = new Size(430, 430);
            MinimumSize = new Size(380, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            BackColor = UiTheme.WindowBg;
            Font = UiTheme.Font(9f);

            // ===== Chọn nơi nhận =====
            Label lblTarget = new Label
            {
                Text = "Chuyển tiếp tới:",
                Location = new Point(16, 14),
                Size = new Size(380, 18),
                Font = UiTheme.Font(9f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary
            };

            lstTargets = new ListBox
            {
                Location = new Point(16, 36),
                Size = new Size(382, 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = UiTheme.Font(9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false
            };
            lstTargets.Items.Add("🌐  Cả phòng chat");
            foreach (string member in members)
            {
                if (!string.IsNullOrWhiteSpace(member))
                    lstTargets.Items.Add("👤  " + member);
            }
            lstTargets.SelectedIndex = 0;

            // ===== Xem trước nội dung =====
            Label lblPreview = new Label
            {
                Text = string.IsNullOrEmpty(originalSender)
                    ? "Nội dung được chuyển tiếp:"
                    : $"Nội dung được chuyển tiếp (từ {originalSender}):",
                Location = new Point(16, 196),
                Size = new Size(382, 18),
                Font = UiTheme.Font(9f, FontStyle.Bold),
                ForeColor = UiTheme.TextPrimary,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            TextBox txtPreview = new TextBox
            {
                Text = originalContent,
                Location = new Point(16, 218),
                Size = new Size(382, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 250, 252),
                BorderStyle = BorderStyle.FixedSingle,
                Font = UiTheme.Font(9.5f)
            };

            // ===== Nút =====
            ModernButton btnOk = new ModernButton
            {
                Text = "Chuyển tiếp",
                Primary = true,
                Size = new Size(120, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnOk.Location = new Point(278, 348);
            btnOk.Click += (s, e) =>
            {
                SelectedTarget = ReadTarget();
                DialogResult = DialogResult.OK;
                Close();
            };

            ModernButton btnCancel = new ModernButton
            {
                Text = "Hủy",
                Primary = false,
                Size = new Size(88, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCancel.Location = new Point(182, 348);
            btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            // Enter = chuyển tiếp, Esc = hủy
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[]
            {
                lblTarget, lstTargets, lblPreview, txtPreview, btnOk, btnCancel
            });
        }

        private string ReadTarget()
        {
            string selected = lstTargets.SelectedItem?.ToString() ?? string.Empty;
            if (lstTargets.SelectedIndex <= 0 || selected.StartsWith("🌐"))
                return RoomTarget;

            // Bỏ tiền tố biểu tượng "👤  " để lấy đúng tên người dùng
            int idx = selected.IndexOf(' ');
            return idx >= 0 ? selected.Substring(idx).Trim() : selected.Trim();
        }
    }
}
