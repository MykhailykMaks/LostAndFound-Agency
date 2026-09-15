using System;
using System.Security.Cryptography;

namespace LostAndFoundAgency.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private const string Prefix = "PBKDF2";

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            if (!storedHash.StartsWith($"{Prefix}$", StringComparison.Ordinal))
            {
                return CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(password),
                    System.Text.Encoding.UTF8.GetBytes(storedHash));
            }

            var parts = storedHash.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        public static bool NeedsRehash(string storedHash)
        {
            return string.IsNullOrWhiteSpace(storedHash)
                || !storedHash.StartsWith($"{Prefix}$", StringComparison.Ordinal);
        }
    }
}
