using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct RemovedBlockInfo :
        IEquatable<RemovedBlockInfo>
    {
        public GridPosition Position { get; }

        public BlockColor Color { get; }

        public BlockType Type { get; }

        public bool ParticipatesInColorObjective =>
            Type == BlockType.Normal;

        public RemovedBlockInfo(
            GridPosition position,
            BlockColor color,
            BlockType type)
        {
            if (!Enum.IsDefined(typeof(BlockColor), color))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "정의되지 않은 블록 색상입니다.");
            }

            if (!Enum.IsDefined(typeof(BlockType), type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "정의되지 않은 블록 종류입니다.");
            }

            Position = position;
            Color = color;
            Type = type;
        }

        public bool Equals(RemovedBlockInfo other)
        {
            return Position.Equals(other.Position)
                && Color == other.Color
                && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return obj is RemovedBlockInfo other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Position,
                Color,
                Type);
        }

        public static bool operator ==(
            RemovedBlockInfo left,
            RemovedBlockInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            RemovedBlockInfo left,
            RemovedBlockInfo right)
        {
            return !left.Equals(right);
        }
    }
}