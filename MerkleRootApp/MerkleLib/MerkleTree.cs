using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MerkleRootApp.MerkleLib
{
    public static class TaggedHash
    {
        public static byte[] Compute(string tag, byte[] message)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));
                byte[] data = tagHash.Concat(tagHash).Concat(message).ToArray();
                return sha256.ComputeHash(data);
            }
        }
    }

    public static class MerkleTree
    {
        private const string HashTag = "Bitcoin_Transaction";

        public static byte[] ComputeMerkleRoot(IEnumerable<string> inputs)
        {
            var leaves = inputs.Select(s => TaggedHash.Compute(HashTag, Encoding.UTF8.GetBytes(s))).ToList();
            return ComputeMerkleRootFromHashes(leaves);
        }

        private static byte[] ComputeMerkleRootFromHashes(List<byte[]> hashes)
        {
            if (hashes.Count == 0)
                return new byte[32]; // empty root

            while (hashes.Count > 1)
            {
                if (hashes.Count % 2 == 1)
                    hashes.Add(hashes.Last()); // Bitcoin-style duplicate last if odd

                var newLevel = new List<byte[]>();

                for (int i = 0; i < hashes.Count; i += 2)
                {
                    byte[] concat = hashes[i].Concat(hashes[i + 1]).ToArray();
                    newLevel.Add(TaggedHash.Compute(HashTag, concat));
                }

                hashes = newLevel;
            }

            return hashes[0];
        }

        public static string ToHexString(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
