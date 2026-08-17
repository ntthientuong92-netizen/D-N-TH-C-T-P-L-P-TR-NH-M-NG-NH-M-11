using System;

namespace ChatClient
{
    public static class ResourceCleanup
    {
        public static void SafeClose(IDisposable resource, string name = "")
        {
            try
            {
                resource?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cleanup Error] {name}: {ex.Message}");
            }
        }
    }
}