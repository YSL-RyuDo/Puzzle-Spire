using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct GravityMove : IEquatable<GravityMove>
    {
        public Block Block { get; }

        public GridPosition From { get; }

        public GridPosition To { get; }

        public GravityMove(
            Block block,
            GridPosition from,
            GridPosition to)
        {
            Block = block
                ?? throw new ArgumentNullException(nameof(block));

            From = from;
            To = to;
        }

        public bool Equals(GravityMove other)
        {
            return ReferenceEquals(Block, other.Block)
                && From.Equals(other.From)
                && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            return obj is GravityMove other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Block, From, To);
        }

        public static bool operator ==(
            GravityMove left,
            GravityMove right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GravityMove left,
            GravityMove right)
        {
            return !left.Equals(right);
        }
    }
}