using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace SpikeApi.Common
{
    public static class PasswordHasher
    {
        internal static string HashUserPassword(string password, string saltedKey)
        {
            //Steps

            //1. Generate salt of the bytes
            byte[] salt = Encoding.UTF8.GetBytes(saltedKey);

            //2. Selecting the hashing algo and generate derivation key.
            byte[] keyDerivation = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 100000, numBytesRequested: 256 / 8);

            //3. Generate hashed password from derivation key
            return Convert.ToBase64String(keyDerivation);
        }
    }
}
