using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Cell
    {
        public GridPosition Position { get; }

        public Block Block { get; private set; }

        public bool IsEmpty => Block == null;

        public Cell(GridPosition position)
        {
            Position = position;
        }

        public Cell(
            GridPosition position,
            Block initialBlock)
        {
            Position = position;
            PlaceBlock(initialBlock);
        }

        public void PlaceBlock(Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (!IsEmpty)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}에는 이미 블록이 있습니다.");
            }

            Block = block;
        }

        public Block RemoveBlock()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}는 이미 비어 있습니다.");
            }

            Block removedBlock = Block;
            Block = null;

            return removedBlock;
        }
    }
}