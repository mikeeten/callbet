using BCrypt.Net;

namespace callbet.Application.Services
{
    public class CryptoService
    {
        public string HashPassword(string plainText)
        {
            // workFactor = 12 → slows brute force
            return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);
        }

        public bool VerifyPassword(string plainText, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hashed);
        }
    }
}
