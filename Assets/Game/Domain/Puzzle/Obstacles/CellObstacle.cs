using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public abstract class CellObstacle
    {
        public CellObstacleType Type { get; }

        public int MaxDurability { get; }

        public int CurrentDurability
        {
            get;
            private set;
        }

        public bool IsDestroyed =>
            CurrentDurability <= 0;

        protected CellObstacle(
            CellObstacleType type,
            int durability)
        {
            if (!Enum.IsDefined(
                    typeof(CellObstacleType),
                    type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "정의되지 않은 장애물 종류입니다.");
            }

            if (durability <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durability),
                    durability,
                    "장애물 내구도는 1 이상이어야 합니다.");
            }

            Type = type;
            MaxDurability = durability;
            CurrentDurability = durability;
        }

        public void TakeDamage(int amount = 1)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    amount,
                    "피해량은 1 이상이어야 합니다.");
            }

            if (IsDestroyed)
            {
                return;
            }

            CurrentDurability =
                Math.Max(
                    0,
                    CurrentDurability - amount);
        }
    }
}