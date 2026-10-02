using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class Cell
    {
        private readonly ICellObstacle[]
            _obstacles =
                new ICellObstacle[
                    (int)ObstacleLayer.Count];

        public GridPosition Position { get; }

        public CellType Type { get; }

        public Block Block { get; private set; }

        public bool IsPlayable =>
            Type == CellType.Playable;

        public bool HasBlock =>
            Block != null;

        public bool HasBlockingObstacle =>
            HasObstacle(
                ObstacleLayer.Blocking);

        public bool CanContainBlock =>
            IsPlayable
            && !HasBlockingObstacle;

        public bool IsEmpty =>
            CanContainBlock
            && !HasBlock;

        public Cell(
            GridPosition position,
            CellType type = CellType.Playable)
        {
            ValidateCellType(type);

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

        public ICellObstacle GetObstacle(
            ObstacleLayer layer)
        {
            ValidateObstacleLayer(layer);

            return _obstacles[(int)layer];
        }

        public bool HasObstacle(
            ObstacleLayer layer)
        {
            return GetObstacle(layer) != null;
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
                throw new ArgumentNullException(
                    nameof(block));
            }

            if (!CanContainBlock)
            {
                throw new InvalidOperationException(
                    $"Blocking 장애물이 있는 칸에는 " +
                    $"블록을 배치할 수 없습니다: {Position}");
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

            Block removedBlock =
                Block;

            Block = null;

            return removedBlock;
        }

        public void PlaceObstacle(
            ICellObstacle obstacle)
        {
            if (!IsPlayable)
            {
                throw new InvalidOperationException(
                    $"Void Cell에는 장애물을 배치할 수 없습니다: {Position}");
            }

            if (obstacle == null)
            {
                throw new ArgumentNullException(
                    nameof(obstacle));
            }

            ObstacleLayer layer =
                obstacle.Layer;

            ValidateObstacleLayer(layer);

            if (_obstacles[(int)layer] != null)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}의 {layer} 레이어에는 " +
                    "이미 장애물이 있습니다.");
            }

            if (layer == ObstacleLayer.Blocking
                && HasBlock)
            {
                throw new InvalidOperationException(
                    $"블록이 있는 칸에는 Blocking 장애물을 " +
                    $"배치할 수 없습니다: {Position}");
            }

            _obstacles[(int)layer] =
                obstacle;
        }

        public ICellObstacle RemoveObstacle(
            ObstacleLayer layer)
        {
            ValidateObstacleLayer(layer);

            ICellObstacle obstacle =
                _obstacles[(int)layer];

            if (obstacle == null)
            {
                throw new InvalidOperationException(
                    $"위치 {Position}의 {layer} 레이어에는 " +
                    "장애물이 없습니다.");
            }

            _obstacles[(int)layer] =
                null;

            return obstacle;
        }

        private static void ValidateCellType(
            CellType type)
        {
            if (!Enum.IsDefined(
                    typeof(CellType),
                    type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "정의되지 않은 Cell 종류입니다.");
            }
        }

        private static void ValidateObstacleLayer(
            ObstacleLayer layer)
        {
            if (layer < ObstacleLayer.Floor
                || layer >= ObstacleLayer.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(layer),
                    layer,
                    "정의되지 않은 장애물 레이어입니다.");
            }
        }
    }
}