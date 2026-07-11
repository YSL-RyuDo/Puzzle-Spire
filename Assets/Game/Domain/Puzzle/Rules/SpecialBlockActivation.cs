using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct SpecialBlockActivation
    {
        public GridPosition Position { get; }

        public BlockType Type { get; }

        public BlockColor Color { get; }

        public int ActivationOrder { get; }

        public SpecialBlockActivation(
            GridPosition position,
            BlockType type,
            BlockColor color,
            int activationOrder)
        {
            if (!Enum.IsDefined(typeof(BlockType), type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown block type.");
            }

            if (type == BlockType.Normal)
            {
                throw new ArgumentException(
                    "Normal blocks cannot be activated.",
                    nameof(type));
            }

            if (!Enum.IsDefined(typeof(BlockColor), color))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "Unknown block color.");
            }

            if (activationOrder < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activationOrder));
            }

            Position = position;
            Type = type;
            Color = color;
            ActivationOrder = activationOrder;
        }
    }
}