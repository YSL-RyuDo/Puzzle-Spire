using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Board
    {
        private readonly Cell[,] _cells;

        public int Width { get; }

        public int Height { get; }

        public Board(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    width,
                    "보드 너비는 1 이상이어야 합니다.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    height,
                    "보드 높이는 1 이상이어야 합니다.");
            }

            Width = width;
            Height = height;
            _cells = new Cell[width, height];

            InitializeCells();
        }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0
                && position.X < Width
                && position.Y >= 0
                && position.Y < Height;
        }

        public Cell GetCell(GridPosition position)
        {
            ValidatePosition(position);

            return _cells[position.X, position.Y];
        }

        public Block GetBlock(GridPosition position)
        {
            return GetCell(position).Block;
        }

        public bool IsEmpty(GridPosition position)
        {
            return GetCell(position).IsEmpty;
        }

        public void PlaceBlock(
            GridPosition position,
            Block block)
        {
            GetCell(position).PlaceBlock(block);
        }

        public Block RemoveBlock(GridPosition position)
        {
            return GetCell(position).RemoveBlock();
        }

        private void InitializeCells()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    GridPosition position = new GridPosition(x, y);
                    _cells[x, y] = new Cell(position);
                }
            }
        }

        private void ValidatePosition(GridPosition position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    position,
                    $"좌표 {position}는 {Width}×{Height} 보드 범위를 벗어났습니다.");
            }
        }
    }
}