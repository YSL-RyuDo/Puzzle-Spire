using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public abstract class DamageableCellObstacle :
        IDamageableObstacle
    {
        public string DefinitionId { get; }

        public abstract CellObstacleType Type { get; }

        public abstract ObstacleLayer Layer { get; }

        public abstract ObstacleImpactKind
            AcceptedImpacts
        { get; }

        public int MaxDurability { get; }

        public int CurrentDurability
        {
            get;
            private set;
        }

        public bool IsDestroyed =>
            CurrentDurability <= 0;

        protected DamageableCellObstacle(
            string definitionId,
            int maxDurability)
        {
            if (string.IsNullOrWhiteSpace(
                    definitionId))
            {
                throw new ArgumentException(
                    "장애물 Definition ID가 필요합니다.",
                    nameof(definitionId));
            }

            if (maxDurability <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxDurability),
                    maxDurability,
                    "최대 내구도는 1 이상이어야 합니다.");
            }

            DefinitionId = definitionId;
            MaxDurability = maxDurability;
            CurrentDurability = maxDurability;
        }

        public ObstacleDamageResult TakeDamage(
            int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "피해량은 1 이상이어야 합니다.");
            }

            int previousDurability =
                CurrentDurability;

            if (IsDestroyed)
            {
                return new ObstacleDamageResult(
                    amount,
                    0,
                    previousDurability,
                    CurrentDurability);
            }

            int appliedDamage =
                Math.Min(
                    amount,
                    CurrentDurability);

            CurrentDurability -=
                appliedDamage;

            return new ObstacleDamageResult(
                amount,
                appliedDamage,
                previousDurability,
                CurrentDurability);
        }
    }
}