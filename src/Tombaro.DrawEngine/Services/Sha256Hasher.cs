using System.Security.Cryptography;
using System.Text;

namespace Tombaro.DrawEngine.Services
{
    /// <summary>
    /// Computes SHA-256 hashes used throughout the draw engine for seed derivation and ticket ordering.
    /// </summary>
    public sealed class Sha256Hasher
    {
        /// <summary>
        /// Computes the SHA-256 hash of <paramref name="input"/> and returns it as an uppercase hexadecimal string.
        /// </summary>
        /// <param name="input">The UTF-8 string to hash.</param>
        /// <returns>A 64-character uppercase hex string representing the SHA-256 hash.</returns>
        public string HashToHex(string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}