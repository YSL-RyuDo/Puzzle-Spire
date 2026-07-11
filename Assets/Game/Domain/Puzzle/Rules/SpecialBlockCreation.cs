using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCreation
    {
        public GridPosition Position { get; }

        public Block Block { get; }

        public BlockType Type =>
            Block.Type;

        public SpecialBlockCreation(
            GridPosition position,
            Block block)
        {
            Block = block
                ?? throw new ArgumentNullException(
                    nameof(block));

            if (block.Type == BlockType.Normal)
            {
                throw new ArgumentException(
                    "The created block must be a special block.",
                    nameof(block));
            }

            Position = position;
        }
    }
}