using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Block
    {
        public BlockColor Color { get; }

        public BlockType Type { get; }

        public bool IsSpecial => Type != BlockType.Normal;

        // Rockets and bombs are independent special blocks. Only normal
        // blocks participate in color matching and color-bomb targeting.
        public bool ParticipatesInColorMatch =>
            Type == BlockType.Normal;

        public Block(
            BlockColor color,
            BlockType type = BlockType.Normal)
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

            Color = color;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Color} {Type}";
        }
    }
}