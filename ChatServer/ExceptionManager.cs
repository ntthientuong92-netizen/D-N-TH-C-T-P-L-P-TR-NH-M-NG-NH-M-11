using System;

namespace ChatServer
{
    public static class ExceptionManager
    {
        public static void HandleException(Exception ex, string context = "")
        {
            string errorMsg = string.IsNullOrEmpty(context) 
                ? $"[Error] {ex.Message}" 
                : $"[Error in {context}] {ex.Message}";
            
            ServerLogger.Log(errorMsg);
        }
    }
}