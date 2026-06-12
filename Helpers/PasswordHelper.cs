using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace WebERP.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // 128-bit salt
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split('.');
                if (parts.Length != 2) return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                string hash = parts[1];

                string checkHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                return hash == checkHash;
            }
            catch
            {
                return false;
            }
        }
    }
}
