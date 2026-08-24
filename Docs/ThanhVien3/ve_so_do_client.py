# -*- coding: utf-8 -*-
"""Sơ đồ kiến trúc phía Client - Thành viên 3 (Đồ án LTM Nhóm 11)"""
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
import os

fig, ax = plt.subplots(figsize=(12.5, 9), dpi=160)
ax.set_xlim(0, 100)
ax.set_ylim(0, 100)
ax.axis("off")

C_GUI   = "#DCEBFF"; E_GUI   = "#3B7DD8"
C_LOGIC = "#E3F7E3"; E_LOGIC = "#3D9A3D"
C_SHARE = "#FFF3D6"; E_SHARE = "#D89A2B"
C_NET   = "#FBE3E3"; E_NET   = "#C95454"
C_OUT   = "#F0F0F0"; E_OUT   = "#999999"

def box(x, y, w, h, text, fc, ec, fs=10, bold=False, tc="#1A1A1A"):
    p = FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.6,rounding_size=1.2",
                       linewidth=1.6, edgecolor=ec, facecolor=fc)
    ax.add_patch(p)
    ax.text(x + w/2, y + h/2, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", color=tc, linespacing=1.45)

def arrow(x1, y1, x2, y2, label="", color="#444444", style="-|>", ls="-", lx=0, ly=1.6, fs=8.5, rad=0.0):
    a = FancyArrowPatch((x1, y1), (x2, y2), arrowstyle=style, mutation_scale=16,
                        linewidth=1.5, color=color, linestyle=ls,
                        connectionstyle=f"arc3,rad={rad}")
    ax.add_patch(a)
    if label:
        ax.text((x1+x2)/2 + lx, (y1+y2)/2 + ly, label, ha="center", va="center",
                fontsize=fs, color=color, style="italic",
                bbox=dict(boxstyle="round,pad=0.18", fc="white", ec="none", alpha=0.9))

ax.text(50, 97.5, "SƠ ĐỒ KIẾN TRÚC PHÍA CLIENT", ha="center", fontsize=15, fontweight="bold", color="#17365D")
ax.text(50, 93.8, "Ứng dụng Chat TCP/IP Client–Server — Thành viên 3 phụ trách giao diện", ha="center", fontsize=10, color="#666666")

# Người dùng
box(38, 85.5, 24, 6, "NGƯỜI DÙNG\n(nhập tin, chọn avatar/emoji)", "#FFFFFF", "#555555", 9.5, True)

# Lớp GUI (TV3)
box(4, 55.5, 92, 26, "", "#FCFDFF", E_GUI)
ax.text(6.5, 78.8, "LỚP GIAO DIỆN — GUI (Thành viên 3: MainChatForm.cs · MainChatForm.Designer.cs · ContactControl.cs)",
        fontsize=9.5, fontweight="bold", color=E_GUI)
box(6, 57.5, 27, 18, "MainChatForm.cs\n(form chính — logic UI)\n• Kết nối IP/Port\n• Gửi / Reply / Forward\n• Chọn file Avatar (Base64)", C_GUI, E_GUI, 8.8)
box(35.5, 57.5, 27, 18, "ContactControl.cs\n• Danh sách liên hệ\n• Avatar tròn + trạng thái\nOnline/Offline\n• Avatar mặc định (chữ cái)", C_GUI, E_GUI, 8.8)
box(65, 57.5, 29, 18, "MessageBubble (trong\nContactControl.cs)\n• Bong bóng chat kèm avatar\n• Tin mình căn phải / tin khác\ncăn trái • Bấm chọn để Reply", C_GUI, E_GUI, 8.8)
box(6, 51.5, 88, 4.2, "Bảng emoji nhanh (10 nút) — dữ liệu từ EmojiHelper.cs (TV5)", C_GUI, E_GUI, 8.3)

# Lớp logic (TV4)
box(4, 38.5, 92, 11, "", "#FCFFFC", E_LOGIC)
ax.text(6.5, 47.6, "LỚP ĐIỀU KHIỂN CLIENT (Thành viên 4: ChatController.cs · MessageProcessor.cs)",
        fontsize=9.5, fontweight="bold", color=E_LOGIC)
box(10, 39.5, 36, 6.5, "ChatController.cs\nTcpClient + luồng nhận ReceiveLoop\nsự kiện OnMessageReceived / OnDisconnected", C_LOGIC, E_LOGIC, 8.5)
box(52, 39.5, 38, 6.5, "MessageProcessor.cs\nđịnh dạng hiển thị nội dung\nChat / Reply / Forward / Emoji", C_LOGIC, E_LOGIC, 8.5)

# Lớp dùng chung (TV2)
box(4, 25.5, 92, 10.5, "", "#FFFDF6", E_SHARE)
ax.text(6.5, 33.7, "SHAREDLIBRARY — DÙNG CHUNG (Thành viên 2: MessagePacket.cs · NetworkProtocol.cs)",
        fontsize=9.5, fontweight="bold", color=E_SHARE)
box(10, 26.5, 38, 6.5, "MessagePacket.cs\ngói tin: Type · Sender · Content\nReplyToContent · AvatarBase64 · Timestamp", C_SHARE, E_SHARE, 8.5)
box(52, 26.5, 38, 6.5, "NetworkProtocol.cs\nJSON + Length-prefix framing\nSendPacket() / ReceivePacket()", C_SHARE, E_SHARE, 8.5)

# Mạng + Server (TV1)
box(24, 11.5, 52, 9.5, "MẠNG TCP/IP  (cổng 8888)\ndữ liệu avatar truyền kèm gói tin dưới dạng Base64", C_NET, E_NET, 9, True)
box(30, 2, 40, 6, "CHAT SERVER (Thành viên 1)\nTcpListener đa luồng — chuyển tiếp Broadcast", C_OUT, E_OUT, 9)

# Mũi tên
arrow(50, 85.3, 50, 81.9, "tương tác", ly=1.4)
arrow(30, 51.3, 26, 46.4, "nút Gửi/Reply/Forward", color=E_GUI, lx=-11, ly=0.8)
arrow(74, 38.3, 70, 33.4, "MessagePacket", color=E_LOGIC, lx=-9, ly=0.8)
arrow(36, 25.3, 38, 21.4, "gửi gói tin JSON + Length-prefix", color=E_SHARE, lx=-15, ly=0.8)
arrow(62, 21.4, 64, 25.3, "", color=E_SHARE, style="<|-", ls="--")
arrow(50, 11.3, 50, 8.2, "", color=E_NET)

# luồng dữ liệu trả lời (bên phải)
arrow(97, 64, 97, 20, "", color="#888888", ls="--", rad=-0.32)
ax.text(96.5, 41, "luồng nhận: Server → ReceivePacket → ChatController\n→ Invoke(an toàn luồng) → MessageBubble + cập nhật Contact",
        rotation=90, ha="center", va="center", fontsize=8.3, color="#666666", style="italic")

out = os.path.join(os.path.dirname(__file__), "SoDo_Client.png")
plt.tight_layout()
plt.savefig(out, bbox_inches="tight", facecolor="white")
print("Saved:", out)
