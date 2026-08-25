using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ChatClient
{
    public static class ResourceCleanup
    {
        private static readonly List<IDisposable> trackedResources = new List<IDisposable>();
        private static readonly object lockObj = new object();
        private static System.Threading.Timer autoCleanupTimer;
        private const string LogFilePath = "cleanup_log.txt";

        static ResourceCleanup()
        {
            autoCleanupTimer = new System.Threading.Timer(AutoCleanupTask, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        private static void AutoCleanupTask(object state)
        {
            LogToFile("--- Bắt đầu dọn dẹp định kỳ (Auto Cleanup) ---");
            long memoryBefore = GC.GetTotalMemory(false);
            
            //  gom rac
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            long memoryAfter = GC.GetTotalMemory(true);
            long freedMemory = memoryBefore - memoryAfter;
            
            if (freedMemory > 0)
            {
                LogToFile($"[AutoCleanup] Đã thu hồi: {freedMemory / 1024} KB RAM.");
            }
        }

        // Theo dõi một tài nguyên để giải phóng sau này
        public static void Track(IDisposable resource)
        {
            if (resource == null) return;
            lock (lockObj)
            {
                if (!trackedResources.Contains(resource))
                {
                    trackedResources.Add(resource);
                    LogToFile($"[Track] Đưa vào theo dõi tài nguyên: {resource.GetType().Name}");
                }
            }
        }

        // dong va giai phong bo nho 
        public static void SafeClose(IDisposable resource, string name = "")
        {
            try
            {
                if (resource != null)
                {
                    resource.Dispose();
                    lock (lockObj)
                    {
                        trackedResources.Remove(resource);
                    }
                    LogToFile($"[SafeClose] Đã đóng tài nguyên: {name} ({resource.GetType().Name})");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"[Cleanup Error] {name}: {ex.Message}");
            }
        }

        // Giai phong bo nho tai nguyen khi thoat 
        public static void DisposeAll()
        {
            LogToFile(" BẮT ĐẦU GIẢI PHÓNG TOÀN BỘ TÀI NGUYÊN ");
            long memoryBefore = GC.GetTotalMemory(false);
            int count = 0;

            lock (lockObj)
            {
                foreach (var res in trackedResources)
                {
                    try
                    {
                        if (res != null)
                        {
                            res.Dispose();
                            LogToFile($"[DisposeAll] Đã giải phóng: {res.GetType().Name}");
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogToFile($"[Global Cleanup Error] {res?.GetType().Name}: {ex.Message}");
                    }
                }
                trackedResources.Clear();
            }

            autoCleanupTimer?.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            long memoryAfter = GC.GetTotalMemory(true);
            LogToFile($"=== HOÀN TẤT. Giải phóng {count} đối tượng. Tiết kiệm: {(memoryBefore - memoryAfter) / 1024} KB RAM ===");
        }

        private static void LogToFile(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logMessage);
                Console.WriteLine(logMessage.Trim());
            }
            catch
            {
            }
        }
    }
}
