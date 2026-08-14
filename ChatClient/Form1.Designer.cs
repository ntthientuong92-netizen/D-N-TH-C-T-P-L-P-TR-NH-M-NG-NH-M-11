namespace ChatClient
{
    partial class Form1
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
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnSelectAvatar = new System.Windows.Forms.Button();
            this.txtChatContent = new System.Windows.Forms.RichTextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtForwardTo = new System.Windows.Forms.TextBox();
            this.btnForward = new System.Windows.Forms.Button();
            this.btnEmoji1 = new System.Windows.Forms.Button();
            this.btnEmoji2 = new System.Windows.Forms.Button();
            this.btnEmoji3 = new System.Windows.Forms.Button();
            this.btnEmoji4 = new System.Windows.Forms.Button();
            this.btnEmoji5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            
            this.txtServerIP.Location = new System.Drawing.Point(12, 12);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(90, 22);
            this.txtServerIP.Text = "127.0.0.1";

            
            this.txtPort.Location = new System.Drawing.Point(110, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(50, 22);
            this.txtPort.Text = "8888";

           
            this.txtName.Location = new System.Drawing.Point(170, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(80, 22);
            this.txtName.Text = "User";

    
            this.btnConnect.Location = new System.Drawing.Point(260, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 26);
            this.btnConnect.Text = "Kết nối";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

      
            this.btnSelectAvatar.Location = new System.Drawing.Point(345, 10);
            this.btnSelectAvatar.Name = "btnSelectAvatar";
            this.btnSelectAvatar.Size = new System.Drawing.Size(85, 26);
            this.btnSelectAvatar.Text = "Chọn Ảnh";
            this.btnSelectAvatar.UseVisualStyleBackColor = true;
            this.btnSelectAvatar.Click += new System.EventHandler(this.btnSelectAvatar_Click);

        
            this.txtChatContent.Location = new System.Drawing.Point(12, 45);
            this.txtChatContent.Name = "txtChatContent";
            this.txtChatContent.ReadOnly = true;
            this.txtChatContent.Size = new System.Drawing.Size(760, 300);

           
            this.btnEmoji1.Location = new System.Drawing.Point(12, 355);
            this.btnEmoji1.Size = new System.Drawing.Size(35, 28);
            this.btnEmoji1.Text = "😀";
            this.btnEmoji1.Click += (s, e) => txtMessage.Text += " 😀 ";

          
            this.btnEmoji2.Location = new System.Drawing.Point(53, 355);
            this.btnEmoji2.Size = new System.Drawing.Size(35, 28);
            this.btnEmoji2.Text = "❤️";
            this.btnEmoji2.Click += (s, e) => txtMessage.Text += " ❤️ ";

            this.btnEmoji3.Location = new System.Drawing.Point(94, 355);
            this.btnEmoji3.Size = new System.Drawing.Size(35, 28);
            this.btnEmoji3.Text = "🔥";
            this.btnEmoji3.Click += (s, e) => txtMessage.Text += " 🔥 ";

           
           this.btnEmoji4.Location = new System.Drawing.Point(135, 355);
           this.btnEmoji4.Size = new System.Drawing.Size(35, 28);
           this.btnEmoji4.Text = "😂";
           this.btnEmoji4.Click += (s, e) => txtMessage.Text += " 😂 ";
     
           this.btnEmoji5.Location = new System.Drawing.Point(176, 355);
           this.btnEmoji5.Size = new System.Drawing.Size(35, 28);
           this.btnEmoji5.Text = "👍";
           this.btnEmoji5.Click += (s, e) => txtMessage.Text += " 👍 ";

    


         
            this.txtMessage.Location = new System.Drawing.Point(180 , 358);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(515, 22);

            this.btnSend.Location = new System.Drawing.Point(665, 354);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(105, 30);
            this.btnSend.Text = "Gửi";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            this.txtForwardTo.Location = new System.Drawing.Point(12, 395);
            this.txtForwardTo.Name = "txtForwardTo";
            this.txtForwardTo.Size = new System.Drawing.Size(200, 22);

            this.btnForward.Location = new System.Drawing.Point(220, 392);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(110, 28);
            this.btnForward.Text = "Chuyển tiếp";
            this.btnForward.UseVisualStyleBackColor = true;
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);


            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 440);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSelectAvatar);
            this.Controls.Add(this.txtChatContent);
            this.Controls.Add(this.btnEmoji1);
            this.Controls.Add(this.btnEmoji2);
            this.Controls.Add(this.btnEmoji3);
            this.Controls.Add(this.btnEmoji4);
            this.Controls.Add(this.btnEmoji5);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtForwardTo);
            this.Controls.Add(this.btnForward);
            this.Name = "Form1";
            this.Text = "Chat TCP Client(Group 11)";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnSelectAvatar;
        private System.Windows.Forms.RichTextBox txtChatContent;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtForwardTo;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnEmoji1;
        private System.Windows.Forms.Button btnEmoji2;
        private System.Windows.Forms.Button btnEmoji3;
        private System.Windows.Forms.Button btnEmoji4;
        private System.Windows.Forms.Button btnEmoji5;
    }
}