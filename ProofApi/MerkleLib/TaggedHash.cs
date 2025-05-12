using System.Security.Cryptography;
using System.Text;

namespace ProofApi.MerkleLib
{
    public static class TaggedHash
    {
        public static byte[] Compute(string tag, byte[] message)
        {
            using var sha256 = SHA256.Create();
            byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));
            byte[] fullMessage = tagHash.Concat(tagHash).Concat(message).ToArray();
            return sha256.ComputeHash(fullMessage);
        }

        public static string ToHexString(byte[] hash) =>
            BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}