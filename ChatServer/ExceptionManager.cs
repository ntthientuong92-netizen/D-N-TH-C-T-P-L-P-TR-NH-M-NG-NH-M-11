using System;
using System.Net.Sockets;

namespace ChatServer
{
    public static class ExceptionManager
    {
        public static void HandleException(Exception ex, string context = "")
        {
            
            string errorType = ex switch
            {
                SocketException => "Lỗi Socket (mất kết nối đột ngột)",
                System.IO.IOException => "Lỗi IO (client đã đóng kết nối)",
                ObjectDisposedException => "Đối tượng đã bị giải phóng (dùng lại kết nối đã đóng)",
                _ => "Lỗi không xác định"
            };

            string errorMsg = string.IsNullOrEmpty(context)
                ? $"[Error - {errorType}] {ex.Message}"
                : $"[Error in {context} - {errorType}] {ex.Message}";

            ServerLogger.Log(errorMsg);
        }
    }
}