using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Cell
    {
        public GridPosition Position { get; }

        public CellType Type { get; }

        public Block Block { get; private set; }

        public bool IsPlayable =>
            Type == CellType.Playable;

        public bool HasBlock =>
            Block != null;

        public bool IsEmpty =>
            IsPlayable && Block == null;

        public Cell(
            GridPosition position,
            CellType type = CellType.Playable)
        {
            ValidateType(type);

            Position = position;
            Type = type;
        }

        public Cell(
            GridPosition position,
            Block initialBlock)
            : this(
                position,
                CellType.Playable)
        {
            PlaceBlock(initialBlock);
        }

        public void PlaceBlock(Block block)
        {
            if (!IsPlayable)
            {
                throw new InvalidOperationException(
                    $"Void Cell에는 블록을 배치할 수 없습니다: {Position}");
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (HasBlock)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}에는 이미 블록이 있습니다.");
            }

            Block = block;
        }

        public Block RemoveBlock()
        {
            if (!IsPlayable)
            {
                throw new InvalidOperationException(
                    $"Void Cell에서는 블록을 제거할 수 없습니다: {Position}");
            }

            if (!HasBlock)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}에는 블록이 없습니다.");
            }

            Block removedBlock = Block;
            Block = null;

            return removedBlock;
        }

        private static void ValidateType(CellType type)
        {
            if (!Enum.IsDefined(typeof(CellType), type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "정의되지 않은 Cell 종류입니다.");
            }
        }
    }
}