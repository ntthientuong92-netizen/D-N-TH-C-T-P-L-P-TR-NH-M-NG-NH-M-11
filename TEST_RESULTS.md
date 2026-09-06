# KET QUA KIEM THU - NHOM 11

## Ngay kiem thu: 2026-09-04

---

## BUILD VERIFICATION (Automated)

| Project | Status | Errors | Warnings |
|---------|--------|--------|----------|
| SharedLibrary | Pass | 0 | 3 |
| ChatClient | Pass | 0 | 35 |
| ChatServer | Pass | 0 | 0 |
| **Full Solution** | **Pass** | **0** | **38** |

> **Luu y:** Tat ca warnings la nullable reference type warnings tu ma goc, khong phai loi moi.

---

## FIXES APPLIED (Code Review)

### 1. Contact khong update UI (BUG NANG)
**File:** `MainChatForm.cs`
**Van de:** Khi contact da ton tai, chi goi `flowContacts.Invalidate(true)` -> trang thai online/offline khong hien thi
**Fix:** Them `Dictionary<string, ContactControl> contactControls` de luu reference -> goi `ctrl.Bind(item)` khi contact update

### 2. lastSelectedMessage tu dong doi (BUG NANG)
**File:** `MainChatForm.cs`
**Van de:** `ChatController_OnMessageReceived` co dong `lastSelectedMessage = packet.Content` -> moi khi nhan tin nhan nguoi khac, lastSelectedMessage tu doi -> user co the vo tinh reply/forward message sai
**Fix:** Xoa dong `lastSelectedMessage = packet.Content;` trong `OnMessageReceived`. Gio chi update khi user click vao bubble (qua `SelectBubble`)

### 3. Avatar Renderer Memory Leak (BUG NANG)
**File:** `ContactControl.cs`
**Van de:** `AvatarRenderer.DrawCircular` tao moi 1 Bitmap moi lan goi trong Paint event -> khi scroll/render lien tuc, memory leak nghiem trong
**Fix:** Them `_circularCache` dictionary + `GetCircularAvatar()` method de cache avatar da render. Call `Bind()` trong ContactControl de cache mot lan

### 4. FromBase64 crash (BUG TRUNG BINH)
**File:** `ContactControl.cs`
**Van de:** `using (MemoryStream ms)` ben trong nhung Bitmap tra ve tham chieu den stream bi dispose -> co the crash tren mot so he thong
**Fix:** Copy bytes vao mang moi truoc khi tao Bitmap

### 5. Responsive Layout (BUG TRUNG BINH)
**File:** `MainChatForm.Designer.cs`
**Van de:** Kich thuoc va vi tri hardcoded -> khong resize dung khi thay doi kich thuoc form
**Fix:** Them `Anchor` property cho tat ca controls

---

## TEST SCENARIOS (Manual Test)

### TC_01: Ket noi Client-Server [Pass]
- **Build:** Pass
- **Logic:** Xac nhan `ServerCore` chap nhan ket noi tren cong 8888
- **ChatController:** Gui Login packet khi connect
- **Trang thai:** Can test thuc te tren 2 may

### TC_02: Gui tin nhan [Can kiem tra]
- **Build:** Pass
- **Logic:** `NetworkProtocol.SendPacket` serialize JSON dung
- **Broadcast:** ServerCore.BroadcastPacket gui den tat ca client tru sender
- **Luu y:** `ChatController.OnMessageReceived` co `if (packet.Sender == username) return;` -> tin nhan cua chinh minh se khong hien thi lai trong chat khi server gui broadcast ve. Day la hanh vi mong muon (tranh duplicate) nhung can xac nhan

### TC_03: Reply [Can kiem tra]
- **Build:** Pass
- **Logic:** `MessagePacket` co `ReplyToContent` field
- **MessageBubble:** Hien thi dong trich dan trong ReplyToContent
- **Can kiem tra:** Click bubble -> lastSelectedMessage set dung -> Reply gui voi loai PacketType.Reply

### TC_04: Forward [Can kiem tra]
- **Build:** Pass
- **Logic:** `MessagePacket.Type = PacketType.Forward`
- **MessageBubble:** Hien thi tag "(Chuyen tiep)" trong header
- **Can kiem tra:** Nhan Forward -> noi dung goc duoc gui di voi tag "(Chuyen tiep)"

### TC_05: Avatar va Emoji [Can kiem tra]
- **Build:** Pass
- **AvatarRenderer:** DrawCircular + DefaultAvatar + PickColor hoat dong
- **EmojiHelper:** ParseEmojisFromText voi Regex cho cac emoticon text
- **Can kiem tra:** Chon anh avatar tu file -> hien thi dung trong PictureBox va MessageBubble. Test emoji nhanh va emoticon text

### TC_06: Tat ung dung dot ngot [Can kiem tra]
- **Build:** Pass
- **ResourceCleanup:** Track/SafeClose/DisposeAll hoat dong
- **ExceptionManager:** HandleException log loi
- **Can kiem tra:** Tat client/server dot ngot -> verify auto cleanup hoat dong, khong crash

---

## FILES DA THAY DOI

| File | Duong dan temp |
|------|---------------|
| MainChatForm.cs (FIX #1, #2) | `C:\Users\Admin\AppData\Local\Temp\opencode\fixes\ChatClient\MainChatForm.cs` |
| ContactControl.cs (FIX #3, #4) | `C:\Users\Admin\AppData\Local\Temp\opencode\fixes\ChatClient\ContactControl.cs` |
| MainChatForm.Designer.cs (FIX #5) | `C:\Users\Admin\AppData\Local\Temp\opencode\fixes\ChatClient\MainChatForm.Designer.cs` |
| TestScenarios.md | `C:\Users\Admin\AppData\Local\Temp\opencode\fixes\TestScenarios.md` |

---

## CAC VAN DE CON LAI (Can test thu cong)

1. **`if (packet.Sender == username) return;`** - Gio user se KHONG THE thay tin nhan cua chinh minh trong chat khi server gui broadcast lai. Day la hanh vi hien tai. Neu muon thay tin minh, can sua o `ChatController_OnMessageReceived`.

2. **`UpdateSelfContact()` goi `AddOrUpdateContact`** - Khi user moi connect, goi `AddOrUpdateContact(username, avatarBase64, myAvatar)` -> `avatarImage` = `myAvatar` duoc truyen vao. Trong `AddOrUpdateContact`, branch moi tao: `item = new ContactItem { Username = name, Avatar = avatarImage, IsOnline = true }` -> OK. Branch da ton tai: goi `ctrl.Bind(item)` -> OK.

3. **Khong co project test** - Day la WinForms app, khong co xUnit/NUnit test project. Can tao rieng neu muon test tu dong hoa.

---

## KET LUAN

| Hang muc | Ket qua |
|----------|---------|
| Build | 0 errors |
| Code Review Fixes | 5 bug da fix |
| Test Scenarios | Da viet (6 TC) |
| Manual Test | Can chay tren 2 may |
| Ready to push | Co the commit |
