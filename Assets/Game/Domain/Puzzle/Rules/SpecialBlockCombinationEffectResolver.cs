using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombinationEffectResolver
    {
        private readonly SpecialBlockEffectResolver _specialBlockEffectResolver;
        private readonly Random _random;

        public SpecialBlockCombinationEffectResolver(
            SpecialBlockEffectResolver specialBlockEffectResolver)
            : this(specialBlockEffectResolver, new Random())
        {
        }

        public SpecialBlockCombinationEffectResolver(
            SpecialBlockEffectResolver specialBlockEffectResolver,
            int randomSeed)
            : this(specialBlockEffectResolver, new Random(randomSeed))
        {
        }

        private SpecialBlockCombinationEffectResolver(
            SpecialBlockEffectResolver specialBlockEffectResolver,
            Random random)
        {
            _specialBlockEffectResolver = specialBlockEffectResolver
                ?? throw new ArgumentNullException(nameof(specialBlockEffectResolver));

            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public SpecialBlockEffectResult Resolve(
            Board board,
            SpecialBlockCombination combination)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (combination == null)
            {
                throw new ArgumentNullException(nameof(combination));
            }

            ValidateCombinationBlocks(board, combination);

            var affectedPositions = new List<GridPosition>();
            var affectedSet = new HashSet<GridPosition>();
            var activations = new List<SpecialBlockActivation>();
            var activationSet = new HashSet<GridPosition>();
            var transformations = new List<SpecialBlockTransformation>();
            var transformationSet = new HashSet<GridPosition>();
            var merges = new List<SpecialBlockMerge>();
            var protectedPositions = new HashSet<GridPosition>();
            var impactEvents = new List<ObstacleImpactEvent>();

            for (int i = 0; i < combination.Members.Count; i++)
            {
                SpecialBlockMember member = combination.Members[i];

                protectedPositions.Add(member.Position);
                AddActivation(
                    member.Position,
                    member.Type,
                    member.Color,
                    activations,
                    activationSet);

                AddAffected(
                    board,
                    member.Position,
                    affectedPositions,
                    affectedSet);

                if (member.Position != combination.PrimaryPosition)
                {
                    merges.Add(
                        new SpecialBlockMerge(
                            member.Position,
                            combination.PrimaryPosition,
                            member.Type,
                            member.Color,
                            merges.Count));
                }
            }

            switch (combination.Type)
            {
                case SpecialBlockCombinationType.RocketRocket:
                    AddRocketRocketImpactEvents(
                        board,
                        combination.PrimaryPosition,
                        affectedPositions,
                        affectedSet,
                        impactEvents);
                    break;

                case SpecialBlockCombinationType.RocketBomb:
                    AddRocketBombImpactEvents(
                        board,
                        combination.PrimaryPosition,
                        affectedPositions,
                        affectedSet,
                        impactEvents);
                    break;

                case SpecialBlockCombinationType.BombBomb:
                    AddImpactEffect(
                        board,
                        CollectSquare(board, combination.PrimaryPosition, 3),
                        affectedPositions,
                        affectedSet,
                        impactEvents);
                    break;

                case SpecialBlockCombinationType.ColorBombRocket:
                    AddColorBombRocketEffect(
                        board,
                        combination,
                        affectedPositions,
                        affectedSet,
                        activations,
                        activationSet,
                        transformations,
                        transformationSet,
                        protectedPositions,
                        impactEvents);
                    break;

                case SpecialBlockCombinationType.ColorBombBomb:
                    AddColorBombBombEffect(
                        board,
                        combination,
                        affectedPositions,
                        affectedSet,
                        activations,
                        activationSet,
                        transformations,
                        transformationSet,
                        protectedPositions,
                        impactEvents);
                    break;

                case SpecialBlockCombinationType.ColorBombColorBomb:
                    AddImpactEffect(
                        board,
                        CollectEntireBoard(board),
                        affectedPositions,
                        affectedSet,
                        impactEvents);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(combination.Type),
                        combination.Type,
                        "Unknown combination type.");
            }

            SpecialBlockEffectResult chainResult =
                _specialBlockEffectResolver.Resolve(
                    board,
                    affectedPositions,
                    protectedPositions);

            MergeChainResult(
                board,
                chainResult,
                affectedPositions,
                affectedSet,
                activations,
                activationSet,
                impactEvents);

            return new SpecialBlockEffectResult(
                affectedPositions,
                activations,
                transformations,
                merges,
                impactEvents);
        }

        private void AddColorBombRocketEffect(
            Board board,
            SpecialBlockCombination combination,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet,
            List<SpecialBlockTransformation> transformations,
            HashSet<GridPosition> transformationSet,
            HashSet<GridPosition> protectedPositions,
            List<ObstacleImpactEvent> impactEvents)
        {
            GridPosition colorBombPosition = GetColorBombPosition(combination);

            List<GridPosition> targetPositions = CollectMatchingColorPositions(
                board,
                combination.TargetColor,
                combination,
                colorBombPosition);

            if (!combination.RocketType.IsRocket())
            {
                throw new InvalidOperationException(
                    "ColorBomb + Rocket requires a valid rocket type.");
            }

            for (int i = 0; i < targetPositions.Count; i++)
            {
                GridPosition position = targetPositions[i];
                Block block = board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                BlockType rocketType = GetRandomRocketType();

                AddTransformation(
                    position,
                    block.Type,
                    rocketType,
                    block.Color,
                    transformations,
                    transformationSet,
                    protectedPositions);

                AddActivation(
                    position,
                    rocketType,
                    block.Color,
                    activations,
                    activationSet);

                AddAffected(board, position, affectedPositions, affectedSet);

                List<GridPosition> eventPositions = rocketType == BlockType.HorizontalRocket
                    ? CollectHorizontalLine(board, position.Y)
                    : CollectVerticalLine(board, position.X);

                AddImpactEffect(
                    board,
                    eventPositions,
                    affectedPositions,
                    affectedSet,
                    impactEvents);
            }
        }

        private static void AddColorBombBombEffect(
            Board board,
            SpecialBlockCombination combination,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet,
            List<SpecialBlockTransformation> transformations,
            HashSet<GridPosition> transformationSet,
            HashSet<GridPosition> protectedPositions,
            List<ObstacleImpactEvent> impactEvents)
        {
            GridPosition colorBombPosition = GetColorBombPosition(combination);

            List<GridPosition> targetPositions = CollectMatchingColorPositions(
                board,
                combination.TargetColor,
                combination,
                colorBombPosition);

            for (int i = 0; i < targetPositions.Count; i++)
            {
                GridPosition position = targetPositions[i];
                Block block = board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                AddTransformation(
                    position,
                    block.Type,
                    BlockType.Bomb,
                    block.Color,
                    transformations,
                    transformationSet,
                    protectedPositions);

                AddActivation(
                    position,
                    BlockType.Bomb,
                    block.Color,
                    activations,
                    activationSet);

                AddAffected(board, position, affectedPositions, affectedSet);

                AddImpactEffect(
                    board,
                    CollectSquare(board, position, 1),
                    affectedPositions,
                    affectedSet,
                    impactEvents);
            }
        }

        private BlockType GetRandomRocketType()
        {
            return _random.Next(0, 2) == 0
                ? BlockType.HorizontalRocket
                : BlockType.VerticalRocket;
        }

        private static void MergeChainResult(
            Board board,
            SpecialBlockEffectResult chainResult,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet,
            List<ObstacleImpactEvent> impactEvents)
        {
            for (int i = 0; i < chainResult.AffectedPositions.Count; i++)
            {
                AddAffected(
                    board,
                    chainResult.AffectedPositions[i],
                    affectedPositions,
                    affectedSet);
            }

            for (int i = 0; i < chainResult.ActivatedSpecialBlocks.Count; i++)
            {
                SpecialBlockActivation activation =
                    chainResult.ActivatedSpecialBlocks[i];

                AddActivation(
                    activation.Position,
                    activation.Type,
                    activation.Color,
                    activations,
                    activationSet);
            }

            for (int i = 0; i < chainResult.ObstacleImpactEvents.Count; i++)
            {
                impactEvents.Add(chainResult.ObstacleImpactEvents[i]);
            }
        }

        private static void AddImpactEffect(
            Board board,
            IReadOnlyList<GridPosition> eventPositions,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<ObstacleImpactEvent> impactEvents)
        {
            if (eventPositions.Count == 0)
            {
                return;
            }

            impactEvents.Add(
                new ObstacleImpactEvent(
                    ObstacleImpactKind.Direct,
                    eventPositions));

            for (int i = 0; i < eventPositions.Count; i++)
            {
                AddAffected(
                    board,
                    eventPositions[i],
                    affectedPositions,
                    affectedSet);
            }
        }

        private static void AddRocketRocketImpactEvents(
            Board board,
            GridPosition center,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<ObstacleImpactEvent> impactEvents)
        {
            AddImpactEffect(
                board,
                CollectHorizontalLine(board, center.Y),
                affectedPositions,
                affectedSet,
                impactEvents);

            AddImpactEffect(
                board,
                CollectVerticalLine(board, center.X),
                affectedPositions,
                affectedSet,
                impactEvents);
        }

        private static void AddRocketBombImpactEvents(
            Board board,
            GridPosition center,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<ObstacleImpactEvent> impactEvents)
        {
            for (int offset = -1; offset <= 1; offset++)
            {
                AddImpactEffect(
                    board,
                    CollectHorizontalLine(board, center.Y + offset),
                    affectedPositions,
                    affectedSet,
                    impactEvents);

                AddImpactEffect(
                    board,
                    CollectVerticalLine(board, center.X + offset),
                    affectedPositions,
                    affectedSet,
                    impactEvents);
            }
        }

        private static List<GridPosition> CollectHorizontalLine(Board board, int y)
        {
            var result = new List<GridPosition>(board.Width);

            for (int x = 0; x < board.Width; x++)
            {
                AddIfPlayable(board, new GridPosition(x, y), result);
            }

            return result;
        }

        private static List<GridPosition> CollectVerticalLine(Board board, int x)
        {
            var result = new List<GridPosition>(board.Height);

            for (int y = 0; y < board.Height; y++)
            {
                AddIfPlayable(board, new GridPosition(x, y), result);
            }

            return result;
        }

        private static List<GridPosition> CollectSquare(
            Board board,
            GridPosition center,
            int radius)
        {
            var result = new List<GridPosition>();

            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                for (int offsetY = -radius; offsetY <= radius; offsetY++)
                {
                    AddIfPlayable(
                        board,
                        new GridPosition(
                            center.X + offsetX,
                            center.Y + offsetY),
                        result);
                }
            }

            return result;
        }

        private static List<GridPosition> CollectEntireBoard(Board board)
        {
            var result = new List<GridPosition>(board.Width * board.Height);

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    AddIfPlayable(board, new GridPosition(x, y), result);
                }
            }

            return result;
        }

        private static void AddIfPlayable(
            Board board,
            GridPosition position,
            List<GridPosition> result)
        {
            if (board.IsPlayable(position))
            {
                result.Add(position);
            }
        }

        private static void AddDistinct(
            List<GridPosition> target,
            IReadOnlyList<GridPosition> source)
        {
            var added = new HashSet<GridPosition>(target);
            AddDistinct(target, added, source);
        }

        private static void AddDistinct(
            List<GridPosition> target,
            HashSet<GridPosition> added,
            IReadOnlyList<GridPosition> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (added.Add(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        private static GridPosition GetColorBombPosition(
            SpecialBlockCombination combination)
        {
            for (int i = 0; i < combination.Members.Count; i++)
            {
                SpecialBlockMember member = combination.Members[i];

                if (member.Type == BlockType.ColorBomb)
                {
                    return member.Position;
                }
            }

            throw new InvalidOperationException(
                "The combination does not contain a ColorBomb.");
        }

        private static List<GridPosition> CollectMatchingColorPositions(
            Board board,
            BlockColor targetColor,
            SpecialBlockCombination combination,
            GridPosition origin)
        {
            var positions = new List<GridPosition>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position = new GridPosition(x, y);

                    if (!board.IsPlayable(position) ||
                        combination.ContainsPosition(position))
                    {
                        continue;
                    }

                    Block block = board.GetBlock(position);

                    if (block == null ||
                        !block.ParticipatesInColorMatch ||
                        block.Color != targetColor)
                    {
                        continue;
                    }

                    positions.Add(position);
                }
            }

            positions.Sort(
                (left, right) =>
                {
                    int distanceComparison =
                        GetManhattanDistance(left, origin)
                            .CompareTo(GetManhattanDistance(right, origin));

                    if (distanceComparison != 0)
                    {
                        return distanceComparison;
                    }

                    int yComparison = left.Y.CompareTo(right.Y);

                    return yComparison != 0
                        ? yComparison
                        : left.X.CompareTo(right.X);
                });

            return positions;
        }

        private static int GetManhattanDistance(
            GridPosition first,
            GridPosition second)
        {
            return Math.Abs(first.X - second.X)
                + Math.Abs(first.Y - second.Y);
        }

        private static void AddTransformation(
            GridPosition position,
            BlockType originalType,
            BlockType transformedType,
            BlockColor color,
            List<SpecialBlockTransformation> transformations,
            HashSet<GridPosition> transformationSet,
            HashSet<GridPosition> protectedPositions)
        {
            if (!transformationSet.Add(position))
            {
                return;
            }

            transformations.Add(
                new SpecialBlockTransformation(
                    position,
                    originalType,
                    transformedType,
                    color,
                    transformations.Count));

            protectedPositions.Add(position);
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

            Cell cell = board.GetCell(position);

            if (!cell.HasBlock && !cell.HasAnyObstacle())
            {
                return;
            }

            if (affectedSet.Add(position))
            {
                affectedPositions.Add(position);
            }
        }

        private static void AddActivation(
            GridPosition position,
            BlockType type,
            BlockColor color,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet)
        {
            if (activationSet.Add(position))
            {
                activations.Add(
                    new SpecialBlockActivation(
                        position,
                        type,
                        color,
                        activations.Count));
            }
        }

        private static void ValidateCombinationBlocks(
            Board board,
            SpecialBlockCombination combination)
        {
            for (int i = 0; i < combination.Members.Count; i++)
            {
                SpecialBlockMember member = combination.Members[i];
                Block block = board.GetBlock(member.Position);

                if (block == null)
                {
                    throw new InvalidOperationException(
                        "Combination blocks no longer exist.");
                }

                if (block.Type != member.Type)
                {
                    throw new InvalidOperationException(
                        "Combination block types have changed.");
                }
            }
        }
    }
}