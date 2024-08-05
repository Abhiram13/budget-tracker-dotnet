using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using BudgetTracker.Defination;

namespace BudgetTracker.Security
{
    public static class Hash
    {
        private static string HashPassword(byte[] salt, string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8)
            );
        }

        public static HashDetails GenerateHashedPassword(string password)
        {
            byte[] salt = new byte[128 / 8];

            // TODO: RNGCryptoServiceProvider is deprecated, use alternative
            using (var rngCsp = new RNGCryptoServiceProvider()) { rngCsp.GetNonZeroBytes(salt); };
            string hashed = HashPassword(salt, password);
            return new HashDetails()
            {
                Password = hashed,
                Salt = Convert.ToBase64String(salt),
            };
        }

        private static string CreateHashPasswordfromSalt(string salt, string password)
        {
            return HashPassword(Convert.FromBase64String(salt), password);
        }

        /// <summary>Compares given password with the one in DB and return boolean if matches</summary>
        /// <param name="salt">Taken from DB "salt" property</param>
        /// <param name="dbPassword">Taken from DB "password" property</param>
        /// <param name="password">User provided password</param>
        public static bool Compare(string salt, string password, string dbPassword)
        {
            string hashedPassword = CreateHashPasswordfromSalt(salt, password);
            return hashedPassword == dbPassword;
        }
    }
}