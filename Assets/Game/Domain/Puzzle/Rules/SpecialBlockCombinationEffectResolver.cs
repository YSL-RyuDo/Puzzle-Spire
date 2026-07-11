using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombinationEffectResolver
    {
        private readonly SpecialBlockEffectResolver
            _specialBlockEffectResolver;

        public SpecialBlockCombinationEffectResolver(
            SpecialBlockEffectResolver specialBlockEffectResolver)
        {
            _specialBlockEffectResolver =
                specialBlockEffectResolver
                ?? throw new ArgumentNullException(
                    nameof(specialBlockEffectResolver));
        }

        public SpecialBlockEffectResult Resolve(
            Board board,
            SpecialBlockCombination combination)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            if (combination == null)
            {
                throw new ArgumentNullException(
                    nameof(combination));
            }

            ValidateCombinationBlocks(
                board,
                combination);

            List<GridPosition> affectedPositions =
                new List<GridPosition>();

            HashSet<GridPosition> affectedSet =
                new HashSet<GridPosition>();

            List<SpecialBlockActivation> activations =
                new List<SpecialBlockActivation>();

            HashSet<GridPosition> activationSet =
                new HashSet<GridPosition>();

            AddActivation(
                combination.PrimaryPosition,
                combination.PrimaryBlockType,
                combination.PrimaryColor,
                activations,
                activationSet);

            AddActivation(
                combination.SecondaryPosition,
                combination.SecondaryBlockType,
                combination.SecondaryColor,
                activations,
                activationSet);

            AddAffected(
                board,
                combination.PrimaryPosition,
                affectedPositions,
                affectedSet);

            AddAffected(
                board,
                combination.SecondaryPosition,
                affectedPositions,
                affectedSet);

            switch (combination.Type)
            {
                case SpecialBlockCombinationType
                    .RocketRocket:

                    AddHorizontalLine(
                        board,
                        combination.PrimaryPosition.Y,
                        affectedPositions,
                        affectedSet);

                    AddVerticalLine(
                        board,
                        combination.PrimaryPosition.X,
                        affectedPositions,
                        affectedSet);

                    break;

                case SpecialBlockCombinationType
                    .RocketBomb:

                    AddThreeRowsAndColumns(
                        board,
                        combination.PrimaryPosition,
                        affectedPositions,
                        affectedSet);

                    break;

                case SpecialBlockCombinationType
                    .BombBomb:

                    AddSquareArea(
                        board,
                        combination.PrimaryPosition,
                        radius: 3,
                        affectedPositions,
                        affectedSet);

                    break;

                case SpecialBlockCombinationType
                    .ColorBombRocket:

                    AddColorBombRocketEffect(
                        board,
                        combination,
                        affectedPositions,
                        affectedSet,
                        activations,
                        activationSet);

                    break;

                case SpecialBlockCombinationType
                    .ColorBombBomb:

                    AddColorBombBombEffect(
                        board,
                        combination,
                        affectedPositions,
                        affectedSet,
                        activations,
                        activationSet);

                    break;

                case SpecialBlockCombinationType
                    .ColorBombColorBomb:

                    AddEntireBoard(
                        board,
                        affectedPositions,
                        affectedSet);

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(combination.Type),
                        combination.Type,
                        "Unknown combination type.");
            }

            GridPosition[] protectedPositions =
            {
                combination.PrimaryPosition,
                combination.SecondaryPosition
            };

            SpecialBlockEffectResult chainResult =
                _specialBlockEffectResolver.Resolve(
                    board,
                    affectedPositions,
                    protectedPositions);

            for (int i = 0;
                 i < chainResult.AffectedPositions.Count;
                 i++)
            {
                AddAffected(
                    board,
                    chainResult.AffectedPositions[i],
                    affectedPositions,
                    affectedSet);
            }

            for (int i = 0;
                 i < chainResult
                     .ActivatedSpecialBlocks.Count;
                 i++)
            {
                SpecialBlockActivation activation =
                    chainResult
                        .ActivatedSpecialBlocks[i];

                AddActivation(
                    activation.Position,
                    activation.Type,
                    activation.Color,
                    activations,
                    activationSet);
            }

            return new SpecialBlockEffectResult(
                affectedPositions,
                activations);
        }

        private static void AddColorBombRocketEffect(
            Board board,
            SpecialBlockCombination combination,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet)
        {
            List<GridPosition> targetPositions =
                CollectMatchingColorPositions(
                    board,
                    combination.TargetColor,
                    combination);

            for (int i = 0;
                 i < targetPositions.Count;
                 i++)
            {
                GridPosition position =
                    targetPositions[i];

                Block block =
                    board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                if (block.Type == BlockType.Normal)
                {
                    AddActivation(
                        position,
                        combination.RocketType,
                        block.Color,
                        activations,
                        activationSet);
                }

                if (combination.RocketType
                    == BlockType.HorizontalRocket)
                {
                    AddHorizontalLine(
                        board,
                        position.Y,
                        affectedPositions,
                        affectedSet);
                }
                else
                {
                    AddVerticalLine(
                        board,
                        position.X,
                        affectedPositions,
                        affectedSet);
                }
            }
        }

        private static void AddColorBombBombEffect(
            Board board,
            SpecialBlockCombination combination,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet)
        {
            List<GridPosition> targetPositions =
                CollectMatchingColorPositions(
                    board,
                    combination.TargetColor,
                    combination);

            for (int i = 0;
                 i < targetPositions.Count;
                 i++)
            {
                GridPosition position =
                    targetPositions[i];

                Block block =
                    board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                if (block.Type == BlockType.Normal)
                {
                    AddActivation(
                        position,
                        BlockType.Bomb,
                        block.Color,
                        activations,
                        activationSet);
                }

                AddSquareArea(
                    board,
                    position,
                    radius: 1,
                    affectedPositions,
                    affectedSet);
            }
        }

        private static List<GridPosition>
            CollectMatchingColorPositions(
                Board board,
                BlockColor targetColor,
                SpecialBlockCombination combination)
        {
            List<GridPosition> positions =
                new List<GridPosition>();

            for (int x = 0;
                 x < board.Width;
                 x++)
            {
                for (int y = 0;
                     y < board.Height;
                     y++)
                {
                    GridPosition position =
                        new GridPosition(x, y);

                    if (!board.IsPlayable(position))
                    {
                        continue;
                    }

                    if (combination.ContainsPosition(position))
                    {
                        continue;
                    }

                    Block block =
                        board.GetBlock(position);

                    if (block != null
                        && block.Color == targetColor)
                    {
                        positions.Add(position);
                    }
                }
            }

            return positions;
        }

        private static void AddThreeRowsAndColumns(
            Board board,
            GridPosition center,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            for (int offset = -1;
                 offset <= 1;
                 offset++)
            {
                AddHorizontalLine(
                    board,
                    center.Y + offset,
                    affectedPositions,
                    affectedSet);

                AddVerticalLine(
                    board,
                    center.X + offset,
                    affectedPositions,
                    affectedSet);
            }
        }

        private static void AddHorizontalLine(
            Board board,
            int y,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            for (int x = 0;
                 x < board.Width;
                 x++)
            {
                AddAffected(
                    board,
                    new GridPosition(x, y),
                    affectedPositions,
                    affectedSet);
            }
        }

        private static void AddVerticalLine(
            Board board,
            int x,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            for (int y = 0;
                 y < board.Height;
                 y++)
            {
                AddAffected(
                    board,
                    new GridPosition(x, y),
                    affectedPositions,
                    affectedSet);
            }
        }

        private static void AddSquareArea(
            Board board,
            GridPosition center,
            int radius,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            for (int offsetX = -radius;
                 offsetX <= radius;
                 offsetX++)
            {
                for (int offsetY = -radius;
                     offsetY <= radius;
                     offsetY++)
                {
                    AddAffected(
                        board,
                        new GridPosition(
                            center.X + offsetX,
                            center.Y + offsetY),
                        affectedPositions,
                        affectedSet);
                }
            }
        }

        private static void AddEntireBoard(
            Board board,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            for (int x = 0;
                 x < board.Width;
                 x++)
            {
                for (int y = 0;
                     y < board.Height;
                     y++)
                {
                    AddAffected(
                        board,
                        new GridPosition(x, y),
                        affectedPositions,
                        affectedSet);
                }
            }
        }

        private static void AddAffected(
            Board board,
            GridPosition position,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            if (!board.IsPlayable(position))
            {
                return;
            }

            if (board.GetBlock(position) == null)
            {
                return;
            }

            if (!affectedSet.Add(position))
            {
                return;
            }

            affectedPositions.Add(position);
        }

        private static void AddActivation(
            GridPosition position,
            BlockType type,
            BlockColor color,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet)
        {
            if (!activationSet.Add(position))
            {
                return;
            }

            activations.Add(
                new SpecialBlockActivation(
                    position,
                    type,
                    color,
                    activations.Count));
        }

        private static void ValidateCombinationBlocks(
            Board board,
            SpecialBlockCombination combination)
        {
            Block primaryBlock =
                board.GetBlock(
                    combination.PrimaryPosition);

            Block secondaryBlock =
                board.GetBlock(
                    combination.SecondaryPosition);

            if (primaryBlock == null
                || secondaryBlock == null)
            {
                throw new InvalidOperationException(
                    "Combination blocks no longer exist.");
            }

            if (primaryBlock.Type
                    != combination.PrimaryBlockType
                || secondaryBlock.Type
                    != combination.SecondaryBlockType)
            {
                throw new InvalidOperationException(
                    "Combination block types have changed.");
            }
        }
    }
}