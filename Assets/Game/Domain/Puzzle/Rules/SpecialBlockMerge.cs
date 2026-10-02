using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct SpecialBlockMerge
    {
        public GridPosition SourcePosition { get; }

        public GridPosition TargetPosition { get; }

        public BlockType Type { get; }

        public BlockColor Color { get; }

        public int MergeOrder { get; }

        public SpecialBlockMerge(
            GridPosition sourcePosition,
            GridPosition targetPosition,
            BlockType type,
            BlockColor color,
            int mergeOrder)
        {
            if (sourcePosition == targetPosition)
            {
                throw new ArgumentException(
                    "Source and target positions must be different.",
                    nameof(sourcePosition));
            }

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
                    "Only special blocks can be merged.",
                    nameof(type));
            }

            if (!Enum.IsDefined(typeof(BlockColor), color))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "Unknown block color.");
            }

            if (mergeOrder < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(mergeOrder));
            }

            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            Type = type;
            Color = color;
            MergeOrder = mergeOrder;
        }
    }
}
