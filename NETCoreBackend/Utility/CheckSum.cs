using System.Security.Cryptography;

namespace NETCoreBackend.Utility
{
    public static class CheckSum
    {
        public static string SHA256CheckSum(string filePath)
        {
            using (SHA256 SHA256 = System.Security.Cryptography.SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }
    }
}
