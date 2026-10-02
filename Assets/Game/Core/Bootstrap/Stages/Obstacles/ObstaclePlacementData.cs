using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    [Serializable]
    public sealed class ObstaclePlacementData
    {
        [SerializeField]
        [Min(0)]
        private int _x;

        [SerializeField]
        [Min(0)]
        private int _y;

        [SerializeField]
        private CellObstacleDefinition _definition;

        public GridPosition Position =>
            new GridPosition(
                _x,
                _y);

        public CellObstacleDefinition Definition =>
            _definition;

        public void ValidateOrThrow(
            int boardWidth,
            int boardHeight)
        {
            if (_definition == null)
            {
                throw new InvalidOperationException(
                    $"({_x}, {_y}) 장애물 Definition이 없습니다.");
            }

            if (_x < 0
                || _x >= boardWidth
                || _y < 0
                || _y >= boardHeight)
            {
                throw new InvalidOperationException(
                    $"장애물 좌표 ({_x}, {_y})가 " +
                    "보드 범위를 벗어났습니다.");
            }

            _definition.ValidateOrThrow();
        }
    }
}