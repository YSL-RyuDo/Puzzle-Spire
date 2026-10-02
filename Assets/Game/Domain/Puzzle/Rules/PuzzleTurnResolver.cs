using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleTurnResolver
    {
        private readonly ConnectedBlockFinder _connectedBlockFinder;
        private readonly BlockRemovalSystem _blockRemovalSystem;
        private readonly GravitySystem _gravitySystem;
        private readonly BlockSpawnSystem _blockSpawnSystem;
        private readonly PuzzleStateMachine _stateMachine;
        private readonly SpecialBlockCreationRule _specialBlockCreationRule;
        private readonly SpecialBlockEffectResolver _specialBlockEffectResolver;
        private readonly SpecialBlockCombinationResolver _specialBlockCombinationResolver;
        private readonly SpecialBlockCombinationEffectResolver _specialBlockCombinationEffectResolver;
        private readonly ColorBombSpecialGroupEffectResolver _colorBombSpecialGroupEffectResolver;
        private readonly IObstacleImpactSystem _obstacleImpactSystem;

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine)
            : this(
                connectedBlockFinder,
                blockRemovalSystem,
                gravitySystem,
                blockSpawnSystem,
                stateMachine,
                new SpecialBlockCreationRule(),
                new SpecialBlockEffectResolver())
        {
        }

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine,
            SpecialBlockCreationRule specialBlockCreationRule)
            : this(
                connectedBlockFinder,
                blockRemovalSystem,
                gravitySystem,
                blockSpawnSystem,
                stateMachine,
                specialBlockCreationRule,
                new SpecialBlockEffectResolver())
        {
        }

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine,
            SpecialBlockCreationRule specialBlockCreationRule,
            SpecialBlockEffectResolver specialBlockEffectResolver)
            : this(
                connectedBlockFinder,
                blockRemovalSystem,
                gravitySystem,
                blockSpawnSystem,
                stateMachine,
                specialBlockCreationRule,
                specialBlockEffectResolver,
                new SpecialBlockCombinationResolver(),
                new SpecialBlockCombinationEffectResolver(specialBlockEffectResolver))
        {
        }

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine,
            SpecialBlockCreationRule specialBlockCreationRule,
            SpecialBlockEffectResolver specialBlockEffectResolver,
            SpecialBlockCombinationResolver specialBlockCombinationResolver,
            SpecialBlockCombinationEffectResolver specialBlockCombinationEffectResolver)
            : this(
                connectedBlockFinder,
                blockRemovalSystem,
                gravitySystem,
                blockSpawnSystem,
                stateMachine,
                specialBlockCreationRule,
                specialBlockEffectResolver,
                specialBlockCombinationResolver,
                specialBlockCombinationEffectResolver,
                new ColorBombSpecialGroupEffectResolver(specialBlockEffectResolver),
                new ObstacleImpactSystem())
        {
        }

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine,
            SpecialBlockCreationRule specialBlockCreationRule,
            SpecialBlockEffectResolver specialBlockEffectResolver,
            SpecialBlockCombinationResolver specialBlockCombinationResolver,
            SpecialBlockCombinationEffectResolver specialBlockCombinationEffectResolver,
            ColorBombSpecialGroupEffectResolver colorBombSpecialGroupEffectResolver,
            IObstacleImpactSystem obstacleImpactSystem)
        {
            _connectedBlockFinder = connectedBlockFinder
                ?? throw new ArgumentNullException(nameof(connectedBlockFinder));

            _blockRemovalSystem = blockRemovalSystem
                ?? throw new ArgumentNullException(nameof(blockRemovalSystem));

            _gravitySystem = gravitySystem
                ?? throw new ArgumentNullException(nameof(gravitySystem));

            _blockSpawnSystem = blockSpawnSystem
                ?? throw new ArgumentNullException(nameof(blockSpawnSystem));

            _stateMachine = stateMachine
                ?? throw new ArgumentNullException(nameof(stateMachine));

            _specialBlockCreationRule = specialBlockCreationRule
                ?? throw new ArgumentNullException(nameof(specialBlockCreationRule));

            _specialBlockEffectResolver = specialBlockEffectResolver
                ?? throw new ArgumentNullException(nameof(specialBlockEffectResolver));

            _specialBlockCombinationResolver = specialBlockCombinationResolver
                ?? throw new ArgumentNullException(nameof(specialBlockCombinationResolver));

            _specialBlockCombinationEffectResolver = specialBlockCombinationEffectResolver
                ?? throw new ArgumentNullException(nameof(specialBlockCombinationEffectResolver));

            _colorBombSpecialGroupEffectResolver = colorBombSpecialGroupEffectResolver
                ?? throw new ArgumentNullException(nameof(colorBombSpecialGroupEffectResolver));

            _obstacleImpactSystem = obstacleImpactSystem
                ?? throw new ArgumentNullException(nameof(obstacleImpactSystem));
        }

        public PuzzleTurnResult Resolve(
            Board board,
            GridPosition selectedPosition)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!_stateMachine.CanAcceptInput)
            {
                throw new InvalidOperationException(
                    $"Input is not allowed in state: {_stateMachine.CurrentState}");
            }

            if (!board.IsPlayable(selectedPosition))
            {
                return PuzzleTurnResult.CreateNoChange();
            }

            Block selectedBlock = board.GetBlock(selectedPosition);

            if (selectedBlock == null)
            {
                return PuzzleTurnResult.CreateNoChange();
            }

            try
            {
                _stateMachine.TransitionTo(PuzzleState.FindingConnectedBlocks);

                bool createsSpecialBlock = false;
                BlockType createdSpecialBlockType = BlockType.Normal;
                IReadOnlyList<GridPosition> normalMatchPositions = null;
                SpecialBlockEffectResult effectResult;

                if (selectedBlock.Type != BlockType.Normal)
                {
                    effectResult = ResolveSpecialBlockTurn(board, selectedPosition);
                }
                else
                {
                    IReadOnlyList<GridPosition> connectedPositions =
                        _connectedBlockFinder.Find(board, selectedPosition);

                    if (connectedPositions.Count < _blockRemovalSystem.MinimumGroupSize)
                    {
                        _stateMachine.TransitionTo(PuzzleState.WaitingForInput);
                        return PuzzleTurnResult.CreateNoChange();
                    }

                    normalMatchPositions = connectedPositions;

                    createsSpecialBlock = _specialBlockCreationRule.TryGetSpecialBlockType(
                        connectedPositions,
                        selectedPosition,
                        out createdSpecialBlockType);

                    IReadOnlyList<GridPosition> initialAffectedPositions =
                        CreatePositionsToAffect(
                            connectedPositions,
                            selectedPosition,
                            createsSpecialBlock);

                    GridPosition? protectedPosition =
                        createsSpecialBlock ? selectedPosition : null;

                    effectResult = _specialBlockEffectResolver.Resolve(
                        board,
                        initialAffectedPositions,
                        protectedPosition);
                }

                if (effectResult.ActivatedSpecialBlocks.Count > 0)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.ActivatingSpecialBlocks);
                }

                IReadOnlyList<RemovedBlockInfo> removedBlockInfos =
                    CaptureRemovedBlocks(
                        board,
                        effectResult.AffectedPositions);

                _stateMachine.TransitionTo(PuzzleState.RemovingBlocks);

                IReadOnlyList<GridPosition> removedPositions =
                    RemoveBlocks(
                        board,
                        effectResult.AffectedPositions);

                IReadOnlyList<GridPosition> adjacentImpactSources =
                    normalMatchPositions ?? removedPositions;

                IReadOnlyList<ObstacleStateChange> obstacleChanges =
                    _obstacleImpactSystem.ApplyImpacts(
                        board,
                        effectResult.ObstacleImpactEvents,
                        adjacentImpactSources);

                SpecialBlockCreation createdSpecialBlock = null;

                if (createsSpecialBlock)
                {
                    createdSpecialBlock = ReplaceSelectedBlockWithSpecial(
                        board,
                        selectedPosition,
                        selectedBlock,
                        createdSpecialBlockType);
                }

                _stateMachine.TransitionTo(PuzzleState.ApplyingGravity);
                IReadOnlyList<GravityMove> gravityMoves =
                    _gravitySystem.Apply(board);

                _stateMachine.TransitionTo(PuzzleState.SpawningBlocks);
                IReadOnlyList<BlockSpawn> spawnedBlocks =
                    _blockSpawnSystem.FillEmptyCells(board);

                _stateMachine.TransitionTo(PuzzleState.CheckingObjectives);
                _stateMachine.TransitionTo(PuzzleState.WaitingForInput);

                return new PuzzleTurnResult(
                    true,
                    removedPositions,
                    removedBlockInfos,
                    gravityMoves,
                    spawnedBlocks,
                    effectResult.Transformations,
                    effectResult.ActivatedSpecialBlocks,
                    effectResult.Merges,
                    createdSpecialBlock,
                    obstacleChanges);
            }
            catch
            {
                if (_stateMachine.CurrentState != PuzzleState.WaitingForInput)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.WaitingForInput);
                }

                throw;
            }
        }

        private SpecialBlockEffectResult ResolveSpecialBlockTurn(
            Board board,
            GridPosition selectedPosition)
        {
            if (_colorBombSpecialGroupEffectResolver.TryResolve(
                    board,
                    selectedPosition,
                    out SpecialBlockEffectResult colorBombGroupResult))
            {
                return colorBombGroupResult;
            }

            if (_specialBlockCombinationResolver.TryResolve(
                    board,
                    selectedPosition,
                    out SpecialBlockCombination combination))
            {
                return _specialBlockCombinationEffectResolver.Resolve(
                    board,
                    combination);
            }

            return _specialBlockEffectResolver.Resolve(
                board,
                new[] { selectedPosition });
        }

        private static IReadOnlyList<GridPosition> CreatePositionsToAffect(
            IReadOnlyList<GridPosition> connectedPositions,
            GridPosition selectedPosition,
            bool preserveSelectedPosition)
        {
            var positions = new List<GridPosition>(connectedPositions.Count);

            for (int i = 0; i < connectedPositions.Count; i++)
            {
                GridPosition position = connectedPositions[i];

                if (preserveSelectedPosition && position == selectedPosition)
                {
                    continue;
                }

                positions.Add(position);
            }

            return positions;
        }

        private static IReadOnlyList<GridPosition> RemoveBlocks(
            Board board,
            IReadOnlyList<GridPosition> positions)
        {
            var removedPositions = new List<GridPosition>(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                GridPosition position = positions[i];

                if (!board.IsPlayable(position))
                {
                    continue;
                }

                if (board.GetBlock(position) == null)
                {
                    continue;
                }

                board.RemoveBlock(position);
                removedPositions.Add(position);
            }

            return removedPositions;
        }

        private static SpecialBlockCreation ReplaceSelectedBlockWithSpecial(
            Board board,
            GridPosition selectedPosition,
            Block selectedBlock,
            BlockType specialBlockType)
        {
            board.RemoveBlock(selectedPosition);

            var specialBlock = new Block(
                selectedBlock.Color,
                specialBlockType);

            board.PlaceBlock(selectedPosition, specialBlock);

            return new SpecialBlockCreation(
                selectedPosition,
                specialBlock);
        }

        private static IReadOnlyList<RemovedBlockInfo> CaptureRemovedBlocks(
            Board board,
            IReadOnlyList<GridPosition> positions)
        {
            var removedBlocks = new List<RemovedBlockInfo>(positions.Count);

            for (int i = 0; i < positions.Count; i++)
            {
                GridPosition position = positions[i];

                if (!board.IsPlayable(position))
                {
                    continue;
                }

                Block block = board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                removedBlocks.Add(
                    new RemovedBlockInfo(
                        position,
                        block.Color,
                        block.Type));
            }

            return removedBlocks;
        }
    }
}