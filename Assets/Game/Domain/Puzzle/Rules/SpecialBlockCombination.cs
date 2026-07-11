using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombination
    {
        public SpecialBlockCombinationType Type { get; }

        public GridPosition PrimaryPosition { get; }

        public GridPosition SecondaryPosition { get; }

        public BlockType PrimaryBlockType { get; }

        public BlockType SecondaryBlockType { get; }

        public BlockColor PrimaryColor { get; }

        public BlockColor SecondaryColor { get; }

        public BlockColor TargetColor
        {
            get
            {
                if (PrimaryBlockType == BlockType.ColorBomb
                    && SecondaryBlockType != BlockType.ColorBomb)
                {
                    return SecondaryColor;
                }

                if (SecondaryBlockType == BlockType.ColorBomb
                    && PrimaryBlockType != BlockType.ColorBomb)
                {
                    return PrimaryColor;
                }

                return PrimaryColor;
            }
        }

        public BlockType RocketType
        {
            get
            {
                if (IsRocket(PrimaryBlockType))
                {
                    return PrimaryBlockType;
                }

                if (IsRocket(SecondaryBlockType))
                {
                    return SecondaryBlockType;
                }

                return BlockType.Normal;
            }
        }

        public SpecialBlockCombination(
            SpecialBlockCombinationType type,
            GridPosition primaryPosition,
            Block primaryBlock,
            GridPosition secondaryPosition,
            Block secondaryBlock)
        {
            if (primaryBlock == null)
            {
                throw new ArgumentNullException(
                    nameof(primaryBlock));
            }

            if (secondaryBlock == null)
            {
                throw new ArgumentNullException(
                    nameof(secondaryBlock));
            }

            if (primaryBlock.Type == BlockType.Normal)
            {
                throw new ArgumentException(
                    "Primary block must be special.",
                    nameof(primaryBlock));
            }

            if (secondaryBlock.Type == BlockType.Normal)
            {
                throw new ArgumentException(
                    "Secondary block must be special.",
                    nameof(secondaryBlock));
            }

            int distance =
                Math.Abs(
                    primaryPosition.X
                    - secondaryPosition.X)
                + Math.Abs(
                    primaryPosition.Y
                    - secondaryPosition.Y);

            if (distance != 1)
            {
                throw new ArgumentException(
                    "Special blocks must be orthogonally adjacent.");
            }

            Type = type;

            PrimaryPosition = primaryPosition;
            SecondaryPosition = secondaryPosition;

            PrimaryBlockType = primaryBlock.Type;
            SecondaryBlockType = secondaryBlock.Type;

            PrimaryColor = primaryBlock.Color;
            SecondaryColor = secondaryBlock.Color;
        }

        public bool ContainsPosition(
            GridPosition position)
        {
            return position == PrimaryPosition
                || position == SecondaryPosition;
        }

        private static bool IsRocket(
            BlockType blockType)
        {
            return blockType
                == BlockType.HorizontalRocket
                || blockType
                == BlockType.VerticalRocket;
        }
    }
}