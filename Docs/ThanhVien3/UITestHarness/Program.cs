// Harness chụp màn hình giao diện MainChatForm (Thành viên 3) làm minh chứng báo cáo.
// Tạo form thật, hiển thị, mô phỏng gói tin đến rồi chụp màn hình — không cần thao tác chuột.
// Lưu ý: chạy từ thư mục gốc của repo để ảnh xuất ra đúng chỗ Docs/ThanhVien3/AnhMinhChung.
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using ChatClient;
using SharedLibrary;

Console.OutputEncoding = System.Text.Encoding.UTF8;
_DPIAware();
string outDir = Path.Combine(Directory.GetCurrentDirectory(), "Docs", "ThanhVien3", "AnhMinhChung");
Directory.CreateDirectory(outDir);

var form = new MainChatForm();
form.CreateControl();
foreach (Control c in form.Controls) { _ = c.Handle; } // ép tạo handle để vẽ/paint đúng

// ---- mock server: chỉ chấp nhận TCP để Client kết nối thành công (tin nhắn đến được mô phỏng bên dưới) ----
var mockServer = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 8888);
mockServer.Start();
new System.Threading.Thread(() => {
    while (true) { try { mockServer.AcceptTcpClient(); } catch { break; } }
}) { IsBackground = true }.Start();

var flags = BindingFlags.NonPublic | BindingFlags.Instance;

T GetCtrl<T>(string name) where T : Control
    => (T)typeof(MainChatForm).GetField(name, flags).GetValue(form);

void SetField(string name, object value)
    => typeof(MainChatForm).GetField(name, flags).SetValue(form, value);

void Call(string method, params object[] args)
    => typeof(MainChatForm).GetMethod(method, flags).Invoke(form, args);

void Receive(MessagePacket p) => Call("ChatController_OnMessageReceived", p);

// ---- tạo ảnh avatar mẫu (Base64) ----
string MakeAvatarB64(Color color, string letter)
{
    using var bmp = new Bitmap(96, 96);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    using var brush = new SolidBrush(color);
    g.FillEllipse(brush, 4, 4, 88, 88);
    using var font = new Font("Segoe UI", 34f, FontStyle.Bold);
    var sz = g.MeasureString(letter, font);
    g.DrawString(letter, font, Brushes.White, (96 - sz.Width) / 2f, (96 - sz.Height) / 2f);
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    return Convert.ToBase64String(ms.ToArray());
}

string minhAvatar = MakeAvatarB64(Color.FromArgb(0, 120, 215), "M");
string lanAvatar = MakeAvatarB64(Color.FromArgb(46, 139, 87), "L");

// ---- chuẩn bị dữ liệu cho "Minh" ----
GetCtrl<TextBox>("txtUsername").Text = "Minh";
SetField("avatarBase64", minhAvatar);
SetField("myAvatar", AvatarRenderer.FromBase64(minhAvatar));
GetCtrl<PictureBox>("picAvatar").Invalidate();

// ---- 1. HIỂN THỊ FORM TRƯỚC để layout tính đúng kích thước ----
form.StartPosition = FormStartPosition.Manual;
form.Location = new Point(40, 40);
form.TopMost = true;
form.Show();
for (int i = 0; i < 20; i++) { Application.DoEvents(); System.Threading.Thread.Sleep(40); }

// ---- 2. Kết nối Server thật đang chạy ở 127.0.0.1:8888 ----
Call("BtnConnect_Click", null, EventArgs.Empty);
Console.WriteLine("Da ket noi server.");

// ---- 3. Mô phỏng các thành viên khác tham gia & chat ----
Receive(new MessagePacket { Type = PacketType.Login, Sender = "Lan", Receiver = "All",
    Content = "Lan đã tham gia phòng chat.", AvatarBase64 = lanAvatar });
Receive(new MessagePacket { Type = PacketType.Chat, Sender = "Lan", Receiver = "All",
    Content = "Chào cả nhóm! Hôm nay mình bắt đầu demo đồ án nha 🔥🎉", AvatarBase64 = lanAvatar });
Receive(new MessagePacket { Type = PacketType.Login, Sender = "Tuấn", Receiver = "All",
    Content = "Tuấn đã tham gia phòng chat." }); // không có avatar -> avatar mặc định
Receive(new MessagePacket { Type = PacketType.Chat, Sender = "Tuấn", Receiver = "All",
    Content = "Chào bạn 👍" });

// ---- 3. Minh gửi tin thường & reply ----
GetCtrl<TextBox>("txtMessage").Text = "Chào mọi người 😊";
Call("BtnSend_Click", null, EventArgs.Empty);
GetCtrl<TextBox>("txtMessage").Text = "Đồng ý, bắt đầu nào!";
Call("BtnReply_Click", null, EventArgs.Empty);

// ---- 4. Người khác reply & forward ----
Receive(new MessagePacket { Type = PacketType.Reply, Sender = "Lan", Receiver = "All",
    Content = "Ok bạn! Ứng dụng chat TCP/IP của nhóm 11 🚀",
    ReplyToContent = "Đồng ý, bắt đầu nào!", AvatarBase64 = lanAvatar });
Receive(new MessagePacket { Type = PacketType.Forward, Sender = "Lan", Receiver = "All",
    Content = "Thông báo: demo lúc 9h sáng mai ⏰", AvatarBase64 = lanAvatar });

// ---- 5. Chụp màn hình thật ----
for (int i = 0; i < 20; i++) { Application.DoEvents(); System.Threading.Thread.Sleep(50); }

// dump tọa độ bubble để kiểm chứng layout
var panelChatDebug = GetCtrl<Panel>("panelChat");
foreach (Control c in panelChatDebug.Controls)
    Console.WriteLine($"  [{c.GetType().Name}] X={c.Left} Y={c.Top} W={c.Width} H={c.Height}");

using (var full = new Bitmap(form.Width, form.Height))
{
    using (var sg = Graphics.FromImage(full))
        sg.CopyFromScreen(form.Location.X, form.Location.Y, 0, 0, new Size(form.Width, form.Height));
    full.Save(Path.Combine(outDir, "TC05_ManHinhChinh.png"), ImageFormat.Png);

    var panelContacts = GetCtrl<Panel>("panelContacts");
    var panelChat = GetCtrl<Panel>("panelChat");
    var panelBottom = GetCtrl<Panel>("panelBottom");

    void Crop(Control panel, string file)
    {
        var scr = form.RectangleToScreen(panel.Bounds);
        var r = new Rectangle(scr.X - form.Location.X, scr.Y - form.Location.Y, scr.Width, scr.Height);
        r.Intersect(new Rectangle(0, 0, full.Width, full.Height));
        if (r.Width > 0 && r.Height > 0)
            using (var bmp = full.Clone(r, full.PixelFormat))
                bmp.Save(Path.Combine(outDir, file), ImageFormat.Png);
    }
    Crop(panelContacts, "TC05_DanhSachLienHe.png");
    Crop(panelChat, "TC05_KhungChat.png");
    Crop(panelBottom, "TC05_BangEmoji.png");
}
form.Hide();
Console.WriteLine("Da chup xong 4 anh: " + outDir);
form.Close();

[System.Runtime.InteropServices.DllImport("user32.dll")]
static extern bool SetProcessDPIAware();
static void _DPIAware() { try { SetProcessDPIAware(); } catch { } }
