using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.Security
{
    public static class PasswordManager
    {
        private static Random random = new Random();

        private static string _storedSalt = "0Xin54hFmmX93ljqMUqOzeqhCf8Cpeur";

        public static string Generate(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string Hash(string password)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(_storedSalt), 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            Debug.WriteLine($"Хэш для {password} = {Convert.ToBase64String(hash)}");
            return Convert.ToBase64String(hash);
        }
    }
}
