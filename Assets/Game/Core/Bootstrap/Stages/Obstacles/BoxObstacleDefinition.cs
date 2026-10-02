using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    [CreateAssetMenu(
        fileName = "BoxObstacleDefinition",
        menuName =
            "Puzzle Spire/Obstacles/Box Definition")]
    public sealed class BoxObstacleDefinition :
        CellObstacleDefinition
    {
        [SerializeField]
        [Min(1)]
        private int _maxDurability = 2;

        public int MaxDurability =>
            _maxDurability;

        public override CellObstacleType Type =>
            CellObstacleType.Box;

        public override ObstacleLayer Layer =>
            ObstacleLayer.Blocking;

        public override ICellObstacle
            CreateRuntimeObstacle()
        {
            ValidateOrThrow();

            return new BoxObstacle(
                DefinitionId,
                _maxDurability);
        }

        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (_maxDurability <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 상자 내구도는 1 이상이어야 합니다.");
            }
        }
    }
}