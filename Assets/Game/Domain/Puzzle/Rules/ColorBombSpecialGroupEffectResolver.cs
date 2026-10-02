using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class ColorBombSpecialGroupEffectResolver
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        private readonly SpecialBlockEffectResolver _chainEffectResolver;
        private readonly Random _random;

        public ColorBombSpecialGroupEffectResolver(
            SpecialBlockEffectResolver chainEffectResolver)
            : this(chainEffectResolver, new Random())
        {
        }

        public ColorBombSpecialGroupEffectResolver(
            SpecialBlockEffectResolver chainEffectResolver,
            int randomSeed)
            : this(chainEffectResolver, new Random(randomSeed))
        {
        }

        private ColorBombSpecialGroupEffectResolver(
            SpecialBlockEffectResolver chainEffectResolver,
            Random random)
        {
            _chainEffectResolver = chainEffectResolver
                ?? throw new ArgumentNullException(nameof(chainEffectResolver));

            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public bool TryResolve(
            Board board,
            GridPosition selectedPosition,
            out SpecialBlockEffectResult effectResult)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            effectResult = null;

            if (!board.IsPlayable(selectedPosition))
            {
                return false;
            }

            Block selectedBlock = board.GetBlock(selectedPosition);

            if (selectedBlock == null || selectedBlock.Type == BlockType.Normal)
            {
                return false;
            }

            List<SpecialMember> specialMembers =
                FindConnectedSpecialMembers(board, selectedPosition);

            if (specialMembers.Count < 2)
            {
                return false;
            }

            int colorBombCount = 0;
            SpecialMember primaryColorBomb = default;
            bool hasPrimaryColorBomb = false;

            for (int i = 0; i < specialMembers.Count; i++)
            {
                SpecialMember member = specialMembers[i];

                if (member.Block.Type != BlockType.ColorBomb)
                {
                    continue;
                }

                colorBombCount++;

                if (!hasPrimaryColorBomb || member.Position == selectedPosition)
                {
                    primaryColorBomb = member;
                    hasPrimaryColorBomb = true;
                }
            }

            if (!hasPrimaryColorBomb)
            {
                return false;
            }

            var affectedPositions = new List<GridPosition>();
            var affectedSet = new HashSet<GridPosition>();
            var activations = new List<SpecialBlockActivation>();
            var activationSet = new HashSet<GridPosition>();
            var transformations = new List<SpecialBlockTransformation>();
            var transformationSet = new HashSet<GridPosition>();
            var merges = new List<SpecialBlockMerge>();
            var protectedPositions = new HashSet<GridPosition>();
            var impactEvents = new List<ObstacleImpactEvent>();

            AddSourceSpecialBlocks(
                board,
                specialMembers,
                affectedPositions,
                affectedSet,
                activations,
                activationSet,
                protectedPositions);

            AddMergeAnimations(specialMembers, selectedPosition, merges);

            if (colorBombCount >= 2)
            {
                AddImpactEffect(
                    board,
                    CollectEntireBoard(board),
                    affectedPositions,
                    affectedSet,
                    impactEvents);

                MergeChainEffects(
                    board,
                    affectedPositions,
                    affectedSet,
                    activations,
                    activationSet,
                    protectedPositions,
                    impactEvents);

                effectResult = new SpecialBlockEffectResult(
                    affectedPositions,
                    activations,
                    transformations,
                    merges,
                    impactEvents);

                return true;
            }

            List<BlockType> transformationTypes =
                CollectTransformationTypes(specialMembers);

            if (transformationTypes.Count == 0)
            {
                return false;
            }

            List<GridPosition> targetPositions = CollectTargetColorPositions(
                board,
                primaryColorBomb.Block.Color,
                primaryColorBomb.Position,
                protectedPositions);

            for (int i = 0; i < targetPositions.Count; i++)
            {
                GridPosition targetPosition = targetPositions[i];
                Block targetBlock = board.GetBlock(targetPosition);

                if (targetBlock == null)
                {
                    continue;
                }

                BlockType transformedType =
                    transformationTypes[i % transformationTypes.Count];

                if (transformedType.IsRocket())
                {
                    transformedType = GetRandomRocketType();
                }

                AddTransformation(
                    targetPosition,
                    targetBlock,
                    transformedType,
                    transformations,
                    transformationSet,
                    protectedPositions);

                AddActivation(
                    targetPosition,
                    transformedType,
                    targetBlock.Color,
                    activations,
                    activationSet);

                AddAffected(
                    board,
                    targetPosition,
                    affectedPositions,
                    affectedSet);

                AddTransformedSpecialEffect(
                    board,
                    targetPosition,
                    transformedType,
                    affectedPositions,
                    affectedSet,
                    impactEvents);
            }

            MergeChainEffects(
                board,
                affectedPositions,
                affectedSet,
                activations,
                activationSet,
                protectedPositions,
                impactEvents);

            effectResult = new SpecialBlockEffectResult(
                affectedPositions,
                activations,
                transformations,
                merges,
                impactEvents);

            return true;
        }

        private BlockType GetRandomRocketType()
        {
            return _random.Next(0, 2) == 0
                ? BlockType.HorizontalRocket
                : BlockType.VerticalRocket;
        }

        private static List<SpecialMember> FindConnectedSpecialMembers(
            Board board,
            GridPosition startPosition)
        {
            var queue = new Queue<GridPosition>();
            var visited = new HashSet<GridPosition>();
            var members = new List<SpecialMember>();

            queue.Enqueue(startPosition);
            visited.Add(startPosition);

            while (queue.Count > 0)
            {
                GridPosition currentPosition = queue.Dequeue();

                if (!board.IsPlayable(currentPosition))
                {
                    continue;
                }

                Block currentBlock = board.GetBlock(currentPosition);

                if (currentBlock == null || currentBlock.Type == BlockType.Normal)
                {
                    continue;
                }

                members.Add(new SpecialMember(currentPosition, currentBlock));

                for (int i = 0; i < Directions.Length; i++)
                {
                    GridPosition direction = Directions[i];

                    GridPosition nextPosition = new GridPosition(
                        currentPosition.X + direction.X,
                        currentPosition.Y + direction.Y);

                    if (!board.IsPlayable(nextPosition) ||
                        !visited.Add(nextPosition))
                    {
                        continue;
                    }

                    Block nextBlock = board.GetBlock(nextPosition);

                    if (nextBlock != null && nextBlock.Type != BlockType.Normal)
                    {
                        queue.Enqueue(nextPosition);
                    }
                }
            }

            return members;
        }

        private static List<BlockType> CollectTransformationTypes(
            IReadOnlyList<SpecialMember> members)
        {
            var result = new List<BlockType>();

            for (int i = 0; i < members.Count; i++)
            {
                BlockType type = members[i].Block.Type;

                switch (type)
                {
                    case BlockType.HorizontalRocket:
                    case BlockType.VerticalRocket:
                    case BlockType.Bomb:
                        result.Add(type);
                        break;

                    case BlockType.ColorBomb:
                    case BlockType.Normal:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(type),
                            type,
                            "Unknown block type.");
                }
            }

            return result;
        }

        private static List<GridPosition> CollectTargetColorPositions(
            Board board,
            BlockColor targetColor,
            GridPosition origin,
            HashSet<GridPosition> excludedPositions)
        {
            var positions = new List<GridPosition>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position = new GridPosition(x, y);

                    if (!board.IsPlayable(position) ||
                        excludedPositions.Contains(position))
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

        private static void AddMergeAnimations(
            IReadOnlyList<SpecialMember> specialMembers,
            GridPosition targetPosition,
            List<SpecialBlockMerge> merges)
        {
            for (int i = 0; i < specialMembers.Count; i++)
            {
                SpecialMember member = specialMembers[i];

                if (member.Position == targetPosition)
                {
                    continue;
                }

                merges.Add(
                    new SpecialBlockMerge(
                        member.Position,
                        targetPosition,
                        member.Block.Type,
                        member.Block.Color,
                        merges.Count));
            }
        }

        private static void AddSourceSpecialBlocks(
            Board board,
            IReadOnlyList<SpecialMember> specialMembers,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet,
            HashSet<GridPosition> protectedPositions)
        {
            for (int i = 0; i < specialMembers.Count; i++)
            {
                SpecialMember member = specialMembers[i];

                protectedPositions.Add(member.Position);

                AddAffected(
                    board,
                    member.Position,
                    affectedPositions,
                    affectedSet);

                AddActivation(
                    member.Position,
                    member.Block.Type,
                    member.Block.Color,
                    activations,
                    activationSet);
            }
        }

        private static void AddTransformation(
            GridPosition position,
            Block originalBlock,
            BlockType transformedType,
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
                    originalBlock.Type,
                    transformedType,
                    originalBlock.Color,
                    transformations.Count));

            protectedPositions.Add(position);
        }

        private static void AddTransformedSpecialEffect(
            Board board,
            GridPosition origin,
            BlockType transformedType,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<ObstacleImpactEvent> impactEvents)
        {
            IReadOnlyList<GridPosition> eventPositions;

            switch (transformedType)
            {
                case BlockType.HorizontalRocket:
                    eventPositions = CollectHorizontalLine(board, origin.Y);
                    break;

                case BlockType.VerticalRocket:
                    eventPositions = CollectVerticalLine(board, origin.X);
                    break;

                case BlockType.Bomb:
                    eventPositions = CollectSquare(board, origin, 1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(transformedType),
                        transformedType,
                        "Unsupported transformation type.");
            }

            AddImpactEffect(
                board,
                eventPositions,
                affectedPositions,
                affectedSet,
                impactEvents);
        }

        private void MergeChainEffects(
            Board board,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet,
            List<SpecialBlockActivation> activations,
            HashSet<GridPosition> activationSet,
            HashSet<GridPosition> protectedPositions,
            List<ObstacleImpactEvent> impactEvents)
        {
            SpecialBlockEffectResult chainResult =
                _chainEffectResolver.Resolve(
                    board,
                    affectedPositions,
                    protectedPositions);

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

        private static void AddEntireBoard(
            Board board,
            List<GridPosition> affectedPositions,
            HashSet<GridPosition> affectedSet)
        {
            IReadOnlyList<GridPosition> positions = CollectEntireBoard(board);

            for (int i = 0; i < positions.Count; i++)
            {
                AddAffected(
                    board,
                    positions[i],
                    affectedPositions,
                    affectedSet);
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

        private static int GetManhattanDistance(
            GridPosition first,
            GridPosition second)
        {
            return Math.Abs(first.X - second.X)
                + Math.Abs(first.Y - second.Y);
        }

        private readonly struct SpecialMember
        {
            public GridPosition Position { get; }
            public Block Block { get; }

            public SpecialMember(
                GridPosition position,
                Block block)
            {
                Position = position;
                Block = block ?? throw new ArgumentNullException(nameof(block));
            }
        }
    }
}