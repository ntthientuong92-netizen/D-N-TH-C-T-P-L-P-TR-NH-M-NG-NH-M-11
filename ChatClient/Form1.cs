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

        public Form1()
        {
            InitializeComponent();
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

                string json = System.Text.Json.JsonSerializer.Serialize(msg);
                if (writer != null)
                {
                    await writer.WriteLineAsync(json);
                }

                txtChatContent.AppendText($"[Tôi]: {msg.Content}\n");
                txtMessage.Clear();
            }
        }
        private async void btnForward_Click(object sender, EventArgs e)
        {
            string selectedText = txtChatContent.SelectedText.Trim();
            if (string.IsNullOrEmpty(selectedText))
            {
                MessageBox.Show("Vui lòng bôi đen đoạn tin nhắn muốn chuyển tiếp.");
                return;
            }

            string receiver = txtForwardTo.Text.Trim();
            if (string.IsNullOrEmpty(receiver))
            {
                MessageBox.Show("Vui lòng nhập tên người nhận.");
                return;
            }

            if (client == null || !client.Connected || writer == null)
            {
                MessageBox.Show("Bạn chưa kết nối tới Server.");
                return;
            }

            var originalMsg = new ChatMessage
            {
                Sender = txtName.Text,
                Content = selectedText
            };

            ChatMessage forwardMsg = ForwardMessageHelper.CreateForwardMessage(originalMsg, txtName.Text, receiver);

            string json = System.Text.Json.JsonSerializer.Serialize(forwardMsg);
            await writer.WriteLineAsync(json);

            txtChatContent.AppendText($"↪ [Đã forward tới {receiver}] {selectedText}\n");
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
                            var bubble = new MessageBubbleControl(msg.Sender, msg.Content, msg.AvatarBase64, isMine: false);
                            txtChatContent.Controls.Add(bubble);
                            string displayPrefix = string.IsNullOrEmpty(msg.AvatarBase64) ? "" : "[Có Ảnh] ";

                            if (msg.MessageType == "Forward")
                            {
                                txtChatContent.AppendText($"↪ [Chuyển tiếp từ {msg.OriginalSender}, gửi bởi {msg.Sender}]: {msg.Content}\n");
                            }
                            else
                            {
                                txtChatContent.AppendText($"{displayPrefix}[{msg.Sender}]: {msg.Content}\n");
                            }
                        }));
                    }
                }
            }
            catch { }
        }
    }
}