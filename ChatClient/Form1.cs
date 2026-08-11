using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private TcpClient? client;
        private StreamReader? reader;
        private StreamWriter? writer;
        private string avatarBase64 = "";
        private string replyTo = "";

        public Form1()
        {
            InitializeComponent();
            txtChatContent.DoubleClick += txtChatContent_DoubleClick;
        }

        // Double-click vào 1 dòng tin nhắn để chọn trả lời tin đó
        private void txtChatContent_DoubleClick(object? sender, EventArgs e)
        {
            int line = txtChatContent.GetLineFromCharIndex(txtChatContent.SelectionStart);
            if (line >= 0 && line < txtChatContent.Lines.Length)
            {
                string selected = txtChatContent.Lines[line];
                if (!string.IsNullOrWhiteSpace(selected) && !selected.StartsWith("    ↳"))
                {
                    replyTo = selected;
                    this.Text = "Đang trả lời: " + selected;
                }
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(txtServerIP.Text, int.Parse(txtPort.Text));
                var stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);
                writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                MessageBox.Show("Kết nối thành công tới Server!");
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private void btnSelectAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files(*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] imageBytes = File.ReadAllBytes(ofd.FileName);
                    avatarBase64 = Convert.ToBase64String(imageBytes);
                    MessageBox.Show("Đã tải ảnh đại diện thành công!");
                }
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (client != null && client.Connected)
            {
                var msg = new ChatMessage
                {
                    Sender = txtName.Text,
                    Receiver = "All",
                    Content = txtMessage.Text,
                    AvatarBase64 = avatarBase64,
                    MessageType = string.IsNullOrEmpty(avatarBase64) ? "Text" : "ImageText",
                    Timestamp = DateTime.Now
                };

                if (!string.IsNullOrEmpty(replyTo))
                {
                    msg.MessageType = "Reply";
                    msg.OriginalMessage = replyTo;
                }

                string json = System.Text.Json.JsonSerializer.Serialize(msg);
                if (writer != null)
                {
                    await writer.WriteLineAsync(json);
                }

                if (!string.IsNullOrEmpty(replyTo))
                {
                    txtChatContent.AppendText($"    ↳ Trả lời {replyTo}\n");
                }
                txtChatContent.AppendText($"[Tôi]: {msg.Content}\n");

                txtMessage.Clear();
                replyTo = "";
                this.Text = "Chat TCP Client(Group 11)";
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (reader != null)
                {
                    string? json = await reader.ReadLineAsync();
                    if (json == null) break;

                    var msg = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(json);
                    if (msg != null)
                    {
                        Invoke(new Action(() =>
                        {
                            string displayPrefix = string.IsNullOrEmpty(msg.AvatarBase64) ? "" : "[Có Ảnh] ";
                            txtChatContent.AppendText($"{displayPrefix}[{msg.Sender}]: {msg.Content}\n");
                        }));
                    }
                }
            }
            catch { }
        }
    }
}