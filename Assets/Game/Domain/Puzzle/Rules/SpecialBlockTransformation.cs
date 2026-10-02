using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct SpecialBlockTransformation
    {
        public GridPosition Position { get; }

        public BlockType OriginalType { get; }

        public BlockType TransformedType { get; }

        public BlockColor Color { get; }

        public int TransformationOrder { get; }

        public SpecialBlockTransformation(
            GridPosition position,
            BlockType originalType,
            BlockType transformedType,
            BlockColor color,
            int transformationOrder)
        {
            if (!Enum.IsDefined(typeof(BlockType), originalType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(originalType),
                    originalType,
                    "Unknown original block type.");
            }

            if (!Enum.IsDefined(typeof(BlockType), transformedType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transformedType),
                    transformedType,
                    "Unknown transformed block type.");
            }

            if (transformedType == BlockType.Normal)
            {
                throw new ArgumentException(
                    "The transformed type must be a special block.",
                    nameof(transformedType));
            }

            if (!Enum.IsDefined(typeof(BlockColor), color))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(color),
                    color,
                    "Unknown block color.");
            }

            if (transformationOrder < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transformationOrder));
            }

            Position = position;
            OriginalType = originalType;
            TransformedType = transformedType;
            Color = color;
            TransformationOrder = transformationOrder;
        }
    }
}