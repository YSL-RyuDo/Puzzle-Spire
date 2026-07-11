using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCreationRule
    {
        public const int RocketMinimumCount = 5;
        public const int BombMinimumCount = 7;
        public const int ColorBombMinimumCount = 9;

        private readonly Random _random;

        public SpecialBlockCreationRule()
            : this(new Random())
        {
        }

        public SpecialBlockCreationRule(
            int randomSeed)
            : this(new Random(randomSeed))
        {
        }

        private SpecialBlockCreationRule(
            Random random)
        {
            _random = random
                ?? throw new ArgumentNullException(
                    nameof(random));
        }

        public bool TryGetSpecialBlockType(
            IReadOnlyList<GridPosition> connectedPositions,
            GridPosition selectedPosition,
            out BlockType specialBlockType)
        {
            if (connectedPositions == null)
            {
                throw new ArgumentNullException(
                    nameof(connectedPositions));
            }

            HashSet<GridPosition> uniquePositions =
                new HashSet<GridPosition>(
                    connectedPositions);

            if (!uniquePositions.Contains(
                    selectedPosition))
            {
                throw new ArgumentException(
                    "Selected position is not in the connected group.",
                    nameof(selectedPosition));
            }

            int connectedCount =
                uniquePositions.Count;

            if (connectedCount
                < RocketMinimumCount)
            {
                specialBlockType =
                    BlockType.Normal;

                return false;
            }

            if (connectedCount
                >= ColorBombMinimumCount)
            {
                specialBlockType =
                    BlockType.ColorBomb;

                return true;
            }

            if (connectedCount
                >= BombMinimumCount)
            {
                specialBlockType =
                    BlockType.Bomb;

                return true;
            }

            specialBlockType =
                _random.Next(0, 2) == 0
                    ? BlockType.HorizontalRocket
                    : BlockType.VerticalRocket;

            return true;
        }
    }
}