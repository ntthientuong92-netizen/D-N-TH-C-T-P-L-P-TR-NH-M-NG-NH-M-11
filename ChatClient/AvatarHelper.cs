using System;
using System.Drawing;
using System.IO;

namespace ChatClient
{
    public static class AvatarHelper
    {
        public static string EncodeImageToBase64(string filePath)
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(imageBytes);
        }

        public static Image DecodeBase64ToImage(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(bytes))
            using (Image temp = Image.FromStream(ms))
            {
                return new Bitmap(temp);
            }
        }

        public static bool TryDecodeBase64ToImage(string base64, out Image? image)
        {
            try
            {
                image = DecodeBase64ToImage(base64);
                return true;
            }
            catch
            {
                image = null;
                return false;
            }
        }
    }
}