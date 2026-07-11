using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct BlockSpawn : IEquatable<BlockSpawn>
    {
        public Block Block { get; }

        public GridPosition Position { get; }

        public BlockSpawn(
            Block block,
            GridPosition position)
        {
            Block = block
                ?? throw new ArgumentNullException(nameof(block));

            Position = position;
        }

        public bool Equals(BlockSpawn other)
        {
            return ReferenceEquals(Block, other.Block)
                && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockSpawn other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Block, Position);
        }

        public static bool operator ==(
            BlockSpawn left,
            BlockSpawn right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BlockSpawn left,
            BlockSpawn right)
        {
            return !left.Equals(right);
        }
    }
}