using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Board
    {
        private readonly Cell[,] _cells;

        public int Width { get; }

        public int Height { get; }

        public Board(
            int width,
            int height)
            : this(
                width,
                height,
                Array.Empty<GridPosition>())
        {
        }

        public Board(
            int width,
            int height,
            IReadOnlyCollection<GridPosition>
                voidPositions)
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

            if (voidPositions == null)
            {
                throw new ArgumentNullException(
                    nameof(voidPositions));
            }

            Width = width;
            Height = height;

            _cells =
                new Cell[width, height];

            InitializeCells(
                voidPositions);
        }

        public bool IsInside(
            GridPosition position)
        {
            return position.X >= 0
                && position.X < Width
                && position.Y >= 0
                && position.Y < Height;
        }

        public bool IsPlayable(
            GridPosition position)
        {
            if (!IsInside(position))
            {
                return false;
            }

            return _cells[
                position.X,
                position.Y].IsPlayable;
        }

        public CellType GetCellType(
            GridPosition position)
        {
            return GetCell(position).Type;
        }

        public Cell GetCell(
            GridPosition position)
        {
            ValidatePosition(position);

            return _cells[
                position.X,
                position.Y];
        }

        public Block GetBlock(
            GridPosition position)
        {
            return GetCell(position).Block;
        }

        public bool IsEmpty(
            GridPosition position)
        {
            return GetCell(position).IsEmpty;
        }

        public void PlaceBlock(
            GridPosition position,
            Block block)
        {
            GetCell(position)
                .PlaceBlock(block);
        }

        public Block RemoveBlock(
            GridPosition position)
        {
            return GetCell(position)
                .RemoveBlock();
        }

        public ICellObstacle GetObstacle(
            GridPosition position,
            ObstacleLayer layer)
        {
            return GetCell(position)
                .GetObstacle(layer);
        }

        public bool HasObstacle(
            GridPosition position,
            ObstacleLayer layer)
        {
            return GetCell(position)
                .HasObstacle(layer);
        }

        public void PlaceObstacle(
            GridPosition position,
            ICellObstacle obstacle)
        {
            GetCell(position)
                .PlaceObstacle(obstacle);
        }

        public ICellObstacle RemoveObstacle(
            GridPosition position,
            ObstacleLayer layer)
        {
            return GetCell(position)
                .RemoveObstacle(layer);
        }

        private void InitializeCells(
            IReadOnlyCollection<GridPosition>
                voidPositions)
        {
            HashSet<GridPosition>
                voidPositionSet =
                    new HashSet<GridPosition>();

            foreach (GridPosition position
                     in voidPositions)
            {
                if (!IsInside(position))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(voidPositions),
                        position,
                        "Void 좌표가 보드 범위를 벗어났습니다.");
                }

                voidPositionSet.Add(
                    position);
            }

            for (int x = 0;
                 x < Width;
                 x++)
            {
                for (int y = 0;
                     y < Height;
                     y++)
                {
                    GridPosition position =
                        new GridPosition(
                            x,
                            y);

                    CellType cellType =
                        voidPositionSet.Contains(
                            position)
                            ? CellType.Void
                            : CellType.Playable;

                    _cells[x, y] =
                        new Cell(
                            position,
                            cellType);
                }
            }
        }

        private void ValidatePosition(
            GridPosition position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    position,
                    $"좌표 {position}는 " +
                    $"{Width}×{Height} 보드 범위를 벗어났습니다.");
            }
        }
    }
}