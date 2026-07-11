using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockEffectResolver
    {
        public SpecialBlockEffectResult Resolve(
            Board board,
            IReadOnlyList<GridPosition> initialPositions,
            GridPosition? protectedPosition = null)
        {
            HashSet<GridPosition> protectedPositions =
                new HashSet<GridPosition>();

            if (protectedPosition.HasValue)
            {
                protectedPositions.Add(
                    protectedPosition.Value);
            }

            return ResolveInternal(
                board,
                initialPositions,
                protectedPositions);
        }

        public SpecialBlockEffectResult Resolve(
            Board board,
            IReadOnlyList<GridPosition> initialPositions,
            IReadOnlyCollection<GridPosition> protectedPositions)
        {
            if (protectedPositions == null)
            {
                throw new ArgumentNullException(
                    nameof(protectedPositions));
            }

            HashSet<GridPosition> protectedPositionSet =
                new HashSet<GridPosition>(
                    protectedPositions);

            return ResolveInternal(
                board,
                initialPositions,
                protectedPositionSet);
        }

        private static SpecialBlockEffectResult ResolveInternal(
            Board board,
            IReadOnlyList<GridPosition> initialPositions,
            HashSet<GridPosition> protectedPositions)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            if (initialPositions == null)
            {
                throw new ArgumentNullException(
                    nameof(initialPositions));
            }

            List<GridPosition> affectedPositions =
                new List<GridPosition>();

            HashSet<GridPosition> affectedPositionSet =
                new HashSet<GridPosition>();

            Queue<GridPosition> activationQueue =
                new Queue<GridPosition>();

            HashSet<GridPosition> queuedPositions =
                new HashSet<GridPosition>();

            HashSet<GridPosition> activatedPositions =
                new HashSet<GridPosition>();

            List<SpecialBlockActivation> activations =
                new List<SpecialBlockActivation>();

            for (int i = 0;
                 i < initialPositions.Count;
                 i++)
            {
                GridPosition position =
                    initialPositions[i];

                TryAddAffectedPosition(
                    board,
                    position,
                    protectedPositions,
                    affectedPositions,
                    affectedPositionSet);

                TryQueueSpecialBlock(
                    board,
                    position,
                    protectedPositions,
                    activationQueue,
                    queuedPositions,
                    activatedPositions);
            }

            while (activationQueue.Count > 0)
            {
                GridPosition specialPosition =
                    activationQueue.Dequeue();

                if (!activatedPositions.Add(
                        specialPosition))
                {
                    continue;
                }

                Block specialBlock =
                    board.GetBlock(specialPosition);

                if (specialBlock == null
                    || specialBlock.Type == BlockType.Normal)
                {
                    continue;
                }

                activations.Add(
                    new SpecialBlockActivation(
                        specialPosition,
                        specialBlock.Type,
                        specialBlock.Color,
                        activations.Count));

                List<GridPosition> effectPositions =
                    CollectEffectPositions(
                        board,
                        specialPosition,
                        specialBlock);

                for (int i = 0;
                     i < effectPositions.Count;
                     i++)
                {
                    GridPosition effectPosition =
                        effectPositions[i];

                    TryAddAffectedPosition(
                        board,
                        effectPosition,
                        protectedPositions,
                        affectedPositions,
                        affectedPositionSet);

                    TryQueueSpecialBlock(
                        board,
                        effectPosition,
                        protectedPositions,
                        activationQueue,
                        queuedPositions,
                        activatedPositions);
                }
            }

            return new SpecialBlockEffectResult(
                affectedPositions,
                activations);
        }

        private static List<GridPosition>
            CollectEffectPositions(
                Board board,
                GridPosition origin,
                Block specialBlock)
        {
            List<GridPosition> positions =
                new List<GridPosition>();

            switch (specialBlock.Type)
            {
                case BlockType.HorizontalRocket:
                    for (int x = 0;
                         x < board.Width;
                         x++)
                    {
                        positions.Add(
                            new GridPosition(
                                x,
                                origin.Y));
                    }

                    break;

                case BlockType.VerticalRocket:
                    for (int y = 0;
                         y < board.Height;
                         y++)
                    {
                        positions.Add(
                            new GridPosition(
                                origin.X,
                                y));
                    }

                    break;

                case BlockType.Bomb:
                    for (int offsetX = -1;
                         offsetX <= 1;
                         offsetX++)
                    {
                        for (int offsetY = -1;
                             offsetY <= 1;
                             offsetY++)
                        {
                            positions.Add(
                                new GridPosition(
                                    origin.X + offsetX,
                                    origin.Y + offsetY));
                        }
                    }

                    break;

                case BlockType.ColorBomb:
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

                            Block block =
                                board.GetBlock(position);

                            if (block != null
                                && block.Color
                                == specialBlock.Color)
                            {
                                positions.Add(position);
                            }
                        }
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(specialBlock.Type),
                        specialBlock.Type,
                        "Unknown special block type.");
            }

            return positions;
        }

        private static void TryAddAffectedPosition(
            Board board,
            GridPosition position,
            HashSet<GridPosition> protectedPositions,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedPositionSet)
        {
            if (protectedPositions.Contains(position))
            {
                return;
            }

            if (!board.IsPlayable(position))
            {
                return;
            }

            if (board.GetBlock(position) == null)
            {
                return;
            }

            if (!affectedPositionSet.Add(position))
            {
                return;
            }

            affectedPositions.Add(position);
        }

        private static void TryQueueSpecialBlock(
            Board board,
            GridPosition position,
            HashSet<GridPosition> protectedPositions,
            Queue<GridPosition> activationQueue,
            HashSet<GridPosition> queuedPositions,
            HashSet<GridPosition> activatedPositions)
        {
            if (protectedPositions.Contains(position))
            {
                return;
            }

            if (!board.IsPlayable(position))
            {
                return;
            }

            if (activatedPositions.Contains(position))
            {
                return;
            }

            Block block =
                board.GetBlock(position);

            if (block == null
                || block.Type == BlockType.Normal)
            {
                return;
            }

            if (!queuedPositions.Add(position))
            {
                return;
            }

            activationQueue.Enqueue(position);
        }
    }
}