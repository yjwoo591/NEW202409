using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ForexAITradingSystem.Utilities
{
    public static class Encryptor
    {
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes("YourSaltHere");
        private const string Password = "YourEncryptionPassword";

        public static string Encrypt(string plainText)
        {
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(Password, Salt);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }
                    encrypted = ms.ToArray();
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(Password, Salt);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}