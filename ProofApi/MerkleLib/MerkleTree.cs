using System;
using System.Text;
namespace ProofApi.MerkleLib
{
    public class MerkleNode
    {
        public byte[] Hash { get; set; }
        public MerkleNode? Left { get; set; }
        public MerkleNode? Right { get; set; }
        public MerkleNode? Parent { get; set; }
        public bool IsLeft { get; set; }
        public string Label { get; set; } = "";
    }

    public class MerkleTree
    {
        private const string LeafTag = "ProofOfReserve_Leaf";
        private const string BranchTag = "ProofOfReserve_Branch";
        private List<MerkleNode> _leaves = new();
        public MerkleNode Root { get; private set; }
        public MerkleTree(IEnumerable<string> leafData)
        {
            _leaves = leafData.Select(data => new MerkleNode
            {
                Hash = TaggedHash.Compute(LeafTag, Encoding.UTF8.GetBytes(data)),
                Label = data
            }).ToList();

            Root = BuildTree(_leaves);
        }

        private MerkleNode BuildTree(List<MerkleNode> leaves)
        {
            if (leaves.Count == 0)
                throw new Exception("Empty tree");

            while (leaves.Count % 2 != 0)
                leaves.Add(leaves.Last());

            var level = leaves;
            while (level.Count > 1)
            {
                var nextLevel = new List<MerkleNode>();
                for (int i = 0; i < level.Count; i += 2)
                {
                    var left = level[i];
                    var right = level[i + 1];
                    byte[] concat = left.Hash.Concat(right.Hash).ToArray();
                    byte[] parentHash = TaggedHash.Compute(BranchTag, concat);

                    var parent = new MerkleNode
                    {
                        Hash = parentHash,
                        Left = left,
                        Right = right
                    };

                    left.Parent = parent;
                    right.Parent = parent;
                    left.IsLeft = true;
                    right.IsLeft = false;

                    nextLevel.Add(parent);
                }

                if (nextLevel.Count % 2 != 0 && nextLevel.Count > 1)
                    nextLevel.Add(nextLevel.Last());

                level = nextLevel;
            }

            return level[0];
        }

        public byte[] GetRootHash() => Root.Hash;

        public (int Balance, List<(string node, int position)> Proof) GetProof(int userId, Dictionary<int, int> userData)
        {
            string leafLabel = $"({userId},{userData[userId]})";

            var leaf = _leaves.FirstOrDefault(l => l.Label == leafLabel)
                ?? throw new Exception("User not found");

            var path = new List<(string, int)>(); // (hex, position)

            var current = leaf;
            while (current.Parent != null)
            {
                var sibling = current.IsLeft ? current.Parent.Right : current.Parent.Left;
                int position = sibling == current.Parent.Left ? 0 : 1;
                path.Add((TaggedHash.ToHexString(sibling.Hash), position));
                current = current.Parent;
            }

            return (userData[userId], path);
        }
    }
}