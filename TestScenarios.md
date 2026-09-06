# TestScenarios.md

## Kế hoạch kiểm thử dự án Chat TCP Client-Server - Nhóm 11

---

## Tổng quan

| Mã TC | Mô tả | Người phụ trách | Ưu tiên |
|-------|--------|----------------|---------|
| TC_01 | Test kết nối Client - Server | Thành viên 1 | Cao |
| TC_02 | Test gửi tin nhắn | Thành viên 6 | Cao |
| TC_03 | Test Reply | Thành viên 4 | Trung |
| TC_04 | Test Forward | Thành viên 4 | Trung |
| TC_05 | Test Avatar & Emoji trên GUI | Thành viên 3 | Trung |
| TC_06 | Test tắt ứng dụng đột ngột | Thành viên 6 | Cao |

---

## TC_01: Kết nối Client - Server

### Mục tiêu
Xác nhận Server lắng nghe và Client có thể kết nối thành công.

### Điều kiện tiên quyết
- Server đã chạy trên cổng 8888
- Client có thể truy cập IP Server

### Các bước kiểm thử
1. Khởi động Server (chạy `ChatServer.exe`)
2. Khởi động Client (chạy `ChatClient.exe`)
3. Nhập Server IP: `127.0.0.1`, Tên user: `TestUser_001`
4. Nhấn **Kết nối**
5. Quan sát trạng thái trên thanh trạng thái

### Kết quả mong đợi
- Label trạng thái hiển thị **"● Đã kết nối"** màu xanh
- Server log hiển thị: `"Client đăng nhập thành công: TestUser_001"`
- Hệ thống gửi thông báo `"TestUser_001 đã tham gia phòng chat."`

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## TC_02: Gửi tin nhắn

### Mục tiêu
Xác nhận tin nhắn được gửi và nhận đúng.

### Điều kiện tiên quyết
- TC_01 đã Pass (ít nhất 2 Client đang kết nối)

### Các bước kiểm thử
1. Client A nhập tin nhắn: `"Xin chào các bạn"`
2. Nhấn **Gửi**
3. Quan sát trên Client B
4. Quan sát trên Client A (cả tin nhắn gửi đi)

### Kết quả mong đợi
- Client B thấy tin nhắn của Client A hiển thị trong bong bóng chat
- Client A thấy tin nhắn của mình hiển thị bên phải (background xanh)
- Timestamp hiển thị đúng thời gian hiện tại

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## TC_03: Reply (Trả lời)

### Mục tiêu
Xác nhận chức năng Reply hoạt động đúng.

### Điều kiện tiên quyết
- TC_02 đã Pass

### Các bước kiểm thử
1. Client A gửi tin nhắn: `"Hello"`
2. Client B **click vào bong bóng chat** của Client A
3. Client B nhập nội dung: `"Hi lại"`
4. Nhấn **Reply**

### Kết quả mong đợi
- Tin nhắn Reply hiển thị với dòng trích dẫn `"↩ Hello"` phía trên nội dung
- Tag **"(Chuyển tiếp)"** không xuất hiện (vì đây là Reply, không phải Forward)
- Timestamp hiển thị đúng

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## TC_04: Forward (Chuyển tiếp)

### Mục tiêu
Xác nhận chức năng Forward hoạt động đúng.

### Điều kiện tiên quyết
- TC_02 đã Pass

### Các bước kiểm thử
1. Client A gửi tin nhắn: `"Thông tin quan trọng"`
2. Client B **click vào bong bóng chat** của Client A
3. Nhấn **Forward** (không cần nhập nội dung)

### Kết quả mong đợi
- Tin nhắn hiển thị với tag **"(Chuyển tiếp)"** phía sau tên sender
- Nội dung trích dẫn hiển thị bên trong bong bóng
- Tin nhắn được gửi đến tất cả client khác

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## TC_05: Avatar & Emoji trên GUI

### Mục tiêu
Xác nhận hiển thị Avatar và Emoji trên giao diện chat.

### Điều kiện tiên quyết
- TC_01 đã Pass

### Các bước kiểm thử
**Phần Avatar:**
1. Client A nhấn **Chọn Avatar** → chọn file ảnh `.jpg` hoặc `.png`
2. Avatar hiển thị đúng ở thanh trên và trong bong bóng chat
3. Client B không chọn avatar → hiển thị avatar mặc định (chữ cái đầu)
4. Kiểm tra viền xanh (online) / viền xám (offline)

**Phần Emoji:**
1. Nhấn một nút emoji ở bảng emoji phía dưới
2. Emoji xuất hiện trong ô nhập tin nhắn
3. Gửi tin nhắn có chứa emoji → Emoji hiển thị đúng trên cả Client A và Client B
4. Nhập text dạng `:)` → tự động chuyển thành 😊 khi gửi

### Kết quả mong đợi
- Avatar hiển thị đúng hình tròn, có viền trạng thái
- Avatar mặc định hiển thị chữ cái đầu tên, nền màu theo tên
- Emoji nhanh hiển thị đúng trong ô nhập
- Text emoticon (`:)`, `<3`,...) chuyển thành emoji thật khi gửi

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## TC_06: Tắt ứng dụng đột ngột

### Mục tiêu
Xác nhận xử lý khi Client/Server tắt đột ngột.

### Điều kiện tiên quyết
- TC_01 đã Pass

### Các bước kiểm thử
**Phần Client:**
1. Client A kết nối bình thường
2. Tắt Client A bằng Task Manager (không nhấn nút đóng)
3. Quan sát trên Server và Client B

**Phần Server:**
1. Server đang chạy
2. Tắt Server bằng Task Manager
3. Quan sát trên tất cả Client

### Kết quả mong đợi
**Client tắt đột ngột:**
- Server log: `"Client đã ngắt kết nối: TestUser_001"`
- Client B thấy trạng thái offline của TestUser_001 (viền xám)
- Không crash

**Server tắt đột ngột:**
- Client B thấy thông báo `"Đã mất kết nối với Server!"`
- Các nút kết nối được kích hoạt lại
- Không crash, không treo

### Kết quả thực tế
- [ ] Pass
- [ ] Fail (Ghi chú: _______________)

---

## Bảng tổng hợp kết quả

| Mã TC | Mô tả | Kết quả | Ghi chú |
|-------|--------|---------|---------|
| TC_01 | Kết nối Client-Server | | |
| TC_02 | Gửi tin nhắn | | |
| TC_03 | Reply | | |
| TC_04 | Forward | | |
| TC_05 | Avatar & Emoji GUI | | |
| TC_06 | Tắt ứng dụng đột ngột | | |

**Tổng cộng:** 6 test case
- **Pass:** ____ / 6
- **Fail:** ____ / 6
- **Block:** ____ / 6

---

## Yêu cầu kiểm thử

- Hệ điều hành: Windows 10/11
- .NET: 10.0 trở lên
- IDE: Visual Studio 2022 hoặc tương đương
- Cần ít nhất 2 máy tính/máy ảo để test Client-Server
- Cổng mạng: 8888 (TCP)

---

## Test Environment Setup

```
Server:
  IP: 127.0.0.1 (localhost) hoặc IP LAN
  Port: 8888
  Command: dotnet run --project ChatServer

Client 1 (TestUser_001):
  Command: dotnet run --project ChatClient
  Server IP: [Server IP]
  Username: TestUser_001

Client 2 (TestUser_002):
  Command: dotnet run --project ChatClient
  Server IP: [Server IP]
  Username: TestUser_002
```
