using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombinationResolver
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public bool TryResolve(
            Board board,
            GridPosition selectedPosition,
            out SpecialBlockCombination combination)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            combination = null;

            if (!board.IsPlayable(selectedPosition))
            {
                return false;
            }

            Block selectedBlock =
                board.GetBlock(selectedPosition);

            if (selectedBlock == null
                || selectedBlock.Type == BlockType.Normal)
            {
                return false;
            }

            SpecialBlockCombination bestCombination =
                null;

            int bestPriority = int.MinValue;

            for (int i = 0;
                 i < Directions.Length;
                 i++)
            {
                GridPosition direction =
                    Directions[i];

                GridPosition adjacentPosition =
                    new GridPosition(
                        selectedPosition.X + direction.X,
                        selectedPosition.Y + direction.Y);

                if (!board.IsPlayable(adjacentPosition))
                {
                    continue;
                }

                Block adjacentBlock =
                    board.GetBlock(adjacentPosition);

                if (adjacentBlock == null
                    || adjacentBlock.Type
                    == BlockType.Normal)
                {
                    continue;
                }

                SpecialBlockCombinationType combinationType =
                    GetCombinationType(
                        selectedBlock.Type,
                        adjacentBlock.Type);

                int priority =
                    GetPriority(combinationType);

                if (priority <= bestPriority)
                {
                    continue;
                }

                bestPriority = priority;

                bestCombination =
                    new SpecialBlockCombination(
                        combinationType,
                        selectedPosition,
                        selectedBlock,
                        adjacentPosition,
                        adjacentBlock);
            }

            if (bestCombination == null)
            {
                return false;
            }

            combination = bestCombination;
            return true;
        }

        private static SpecialBlockCombinationType
            GetCombinationType(
                BlockType first,
                BlockType second)
        {
            bool firstColorBomb =
                first == BlockType.ColorBomb;

            bool secondColorBomb =
                second == BlockType.ColorBomb;

            bool firstBomb =
                first == BlockType.Bomb;

            bool secondBomb =
                second == BlockType.Bomb;

            bool firstRocket =
                IsRocket(first);

            bool secondRocket =
                IsRocket(second);

            if (firstColorBomb && secondColorBomb)
            {
                return SpecialBlockCombinationType
                    .ColorBombColorBomb;
            }

            if (firstColorBomb || secondColorBomb)
            {
                BlockType otherType =
                    firstColorBomb
                        ? second
                        : first;

                if (otherType == BlockType.Bomb)
                {
                    return SpecialBlockCombinationType
                        .ColorBombBomb;
                }

                return SpecialBlockCombinationType
                    .ColorBombRocket;
            }

            if (firstBomb && secondBomb)
            {
                return SpecialBlockCombinationType
                    .BombBomb;
            }

            if ((firstBomb && secondRocket)
                || (secondBomb && firstRocket))
            {
                return SpecialBlockCombinationType
                    .RocketBomb;
            }

            return SpecialBlockCombinationType
                .RocketRocket;
        }

        private static int GetPriority(
            SpecialBlockCombinationType type)
        {
            switch (type)
            {
                case SpecialBlockCombinationType
                    .ColorBombColorBomb:
                    return 600;

                case SpecialBlockCombinationType
                    .ColorBombBomb:
                    return 500;

                case SpecialBlockCombinationType
                    .ColorBombRocket:
                    return 400;

                case SpecialBlockCombinationType
                    .BombBomb:
                    return 300;

                case SpecialBlockCombinationType
                    .RocketBomb:
                    return 200;

                case SpecialBlockCombinationType
                    .RocketRocket:
                    return 100;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown combination type.");
            }
        }

        private static bool IsRocket(
            BlockType type)
        {
            return type
                == BlockType.HorizontalRocket
                || type
                == BlockType.VerticalRocket;
        }
    }
}