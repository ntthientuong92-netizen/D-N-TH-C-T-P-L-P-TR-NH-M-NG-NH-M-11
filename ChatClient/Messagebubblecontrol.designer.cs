namespace ChatClient
{
    partial class MessageBubbleControl
    {
        private System.ComponentModel.IContainer components = null;

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
            this.pbAvatar = new System.Windows.Forms.PictureBox();
            this.lblSender = new System.Windows.Forms.Label();
            this.lblContent = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).BeginInit();
            this.SuspendLayout();

            this.pbAvatar.Location = new System.Drawing.Point(8, 8);
            this.pbAvatar.Name = "pbAvatar";
            this.pbAvatar.Size = new System.Drawing.Size(40, 40);
            this.pbAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAvatar.TabStop = false;

            this.lblSender.AutoSize = true;
            this.lblSender.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSender.Location = new System.Drawing.Point(58, 8);
            this.lblSender.Name = "lblSender";
            this.lblSender.Text = "Sender";

            this.lblContent.AutoSize = true;
            this.lblContent.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblContent.Location = new System.Drawing.Point(58, 28);
            this.lblContent.Name = "lblContent";
            this.lblContent.MaximumSize = new System.Drawing.Size(600, 0);
            this.lblContent.Text = "Content";

            this.Controls.Add(this.pbAvatar);
            this.Controls.Add(this.lblSender);
            this.Controls.Add(this.lblContent);
            this.Name = "MessageBubbleControl";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Size = new System.Drawing.Size(640, 60);
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.PictureBox pbAvatar;
        private System.Windows.Forms.Label lblSender;
        private System.Windows.Forms.Label lblContent;
    }
}