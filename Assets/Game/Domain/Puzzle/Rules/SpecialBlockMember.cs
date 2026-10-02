using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct SpecialBlockMember
    {
        public GridPosition Position { get; }

        public BlockType Type { get; }

        public BlockColor Color { get; }

        public SpecialBlockMember(
            GridPosition position,
            Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            ValidateType(block.Type);
            ValidateColor(block.Color);

            Position = position;
            Type = block.Type;
            Color = block.Color;
        }

        public SpecialBlockMember(
            GridPosition position,
            BlockType type,
            BlockColor color)
        {
            ValidateType(type);
            ValidateColor(color);

            Position = position;
            Type = type;
            Color = color;
        }

        private static void ValidateType(BlockType type)
        {
            if (!Enum.IsDefined(typeof(BlockType), type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown block type.");
            }

            if (!type.IsSpecial())
            {
                throw new ArgumentException(
                    "A combination member must be special.",
                    nameof(type));
            }
        }

        private static void ValidateColor(BlockColor color)
        {
            if (!Enum.IsDefined(typeof(BlockColor), color))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "Unknown block color.");
            }
        }
    }
}
