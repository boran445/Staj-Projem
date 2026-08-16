using System;
using System.Security.Cryptography;

namespace DevExtremeMvcApp1.Services
{
    public class PasswordService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public string CreateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        public string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
            {
                return Convert.ToBase64String(deriveBytes.GetBytes(KeySize));
            }
        }

        public bool VerifyPassword(string password, string salt, string expectedHash)
        {
            string actualHash = HashPassword(password, salt);
            return string.Equals(actualHash, expectedHash, StringComparison.Ordinal);
        }
    }
}
