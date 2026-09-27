using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AccuBooks_ERP.Helper
{
    public static class SecurityHelper
    {
        private const int WorkFactor = 11;
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("AccuBooksERP#SecureKey2026!78901"); // 32 bytes for AES-256
        private static readonly byte[] EncryptionIV = Encoding.UTF8.GetBytes("AccuBooksInitVec"); // 16 bytes

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = EncryptionIV;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.Key = EncryptionKey;
                aes.IV = EncryptionIV;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GenerateRandomPassword(int length = 10)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%";
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            foreach (var b in bytes)
            {
                sb.Append(validChars[b % validChars.Length]);
            }
            return sb.ToString();
        }
    }
}
