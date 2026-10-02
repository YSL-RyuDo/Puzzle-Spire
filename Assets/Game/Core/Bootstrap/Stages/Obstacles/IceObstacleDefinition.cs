using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    [CreateAssetMenu(
        fileName = "IceObstacleDefinition",
        menuName = "Puzzle Spire/Obstacles/Ice Definition")]
    public sealed class IceObstacleDefinition : CellObstacleDefinition
    {
        [SerializeField, Min(1)]
        private int _maxDurability = 1;

        public int MaxDurability => _maxDurability;
        public override CellObstacleType Type => CellObstacleType.Ice;
        public override ObstacleLayer Layer => ObstacleLayer.Overlay;

        public override ICellObstacle CreateRuntimeObstacle()
        {
            ValidateOrThrow();
            return new IceObstacle(DefinitionId, _maxDurability);
        }

        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (_maxDurability <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 얼음 내구도는 1 이상이어야 합니다.");
            }
        }
    }
}