using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleTurnResolver
    {
        private readonly ConnectedBlockFinder
            _connectedBlockFinder;

        private readonly BlockRemovalSystem
            _blockRemovalSystem;

        private readonly GravitySystem
            _gravitySystem;

        private readonly BlockSpawnSystem
            _blockSpawnSystem;

        private readonly PuzzleStateMachine
            _stateMachine;

        private readonly SpecialBlockCreationRule
            _specialBlockCreationRule;

        private readonly SpecialBlockEffectResolver
            _specialBlockEffectResolver;

        private readonly SpecialBlockCombinationResolver
            _specialBlockCombinationResolver;

        private readonly SpecialBlockCombinationEffectResolver
            _specialBlockCombinationEffectResolver;

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
                new SpecialBlockCombinationEffectResolver(
                    specialBlockEffectResolver))
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
            SpecialBlockCombinationResolver
                specialBlockCombinationResolver,
            SpecialBlockCombinationEffectResolver
                specialBlockCombinationEffectResolver)
        {
            _connectedBlockFinder =
                connectedBlockFinder
                ?? throw new ArgumentNullException(
                    nameof(connectedBlockFinder));

            _blockRemovalSystem =
                blockRemovalSystem
                ?? throw new ArgumentNullException(
                    nameof(blockRemovalSystem));

            _gravitySystem =
                gravitySystem
                ?? throw new ArgumentNullException(
                    nameof(gravitySystem));

            _blockSpawnSystem =
                blockSpawnSystem
                ?? throw new ArgumentNullException(
                    nameof(blockSpawnSystem));

            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _specialBlockCreationRule =
                specialBlockCreationRule
                ?? throw new ArgumentNullException(
                    nameof(specialBlockCreationRule));

            _specialBlockEffectResolver =
                specialBlockEffectResolver
                ?? throw new ArgumentNullException(
                    nameof(specialBlockEffectResolver));

            _specialBlockCombinationResolver =
                specialBlockCombinationResolver
                ?? throw new ArgumentNullException(
                    nameof(specialBlockCombinationResolver));

            _specialBlockCombinationEffectResolver =
                specialBlockCombinationEffectResolver
                ?? throw new ArgumentNullException(
                    nameof(
                        specialBlockCombinationEffectResolver));
        }

        public PuzzleTurnResult Resolve(
            Board board,
            GridPosition selectedPosition)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            if (!_stateMachine.CanAcceptInput)
            {
                throw new InvalidOperationException(
                    $"Input is not allowed in state: " +
                    $"{_stateMachine.CurrentState}");
            }

            if (!board.IsPlayable(selectedPosition))
            {
                return PuzzleTurnResult.CreateNoChange();
            }

            Block selectedBlock =
                board.GetBlock(selectedPosition);

            if (selectedBlock == null)
            {
                return PuzzleTurnResult.CreateNoChange();
            }

            try
            {
                _stateMachine.TransitionTo(
                    PuzzleState.FindingConnectedBlocks);

                bool createsSpecialBlock = false;

                BlockType createdSpecialBlockType =
                    BlockType.Normal;

                SpecialBlockEffectResult effectResult;

                if (selectedBlock.Type != BlockType.Normal)
                {
                    bool hasCombination =
                        _specialBlockCombinationResolver
                            .TryResolve(
                                board,
                                selectedPosition,
                                out SpecialBlockCombination
                                    combination);

                    if (hasCombination)
                    {
                        effectResult =
                            _specialBlockCombinationEffectResolver
                                .Resolve(
                                    board,
                                    combination);
                    }
                    else
                    {
                        effectResult =
                            _specialBlockEffectResolver.Resolve(
                                board,
                                new[]
                                {
                                    selectedPosition
                                });
                    }
                }
                else
                {
                    IReadOnlyList<GridPosition>
                        connectedPositions =
                            _connectedBlockFinder.Find(
                                board,
                                selectedPosition);

                    if (connectedPositions.Count
                        < _blockRemovalSystem.MinimumGroupSize)
                    {
                        _stateMachine.TransitionTo(
                            PuzzleState.WaitingForInput);

                        return PuzzleTurnResult
                            .CreateNoChange();
                    }

                    createsSpecialBlock =
                        _specialBlockCreationRule
                            .TryGetSpecialBlockType(
                                connectedPositions,
                                selectedPosition,
                                out createdSpecialBlockType);

                    IReadOnlyList<GridPosition>
                        initialAffectedPositions =
                            CreatePositionsToAffect(
                                connectedPositions,
                                selectedPosition,
                                createsSpecialBlock);

                    GridPosition? protectedPosition =
                        createsSpecialBlock
                            ? selectedPosition
                            : null;

                    effectResult =
                        _specialBlockEffectResolver.Resolve(
                            board,
                            initialAffectedPositions,
                            protectedPosition);
                }

                if (effectResult
                    .ActivatedSpecialBlocks.Count > 0)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.ActivatingSpecialBlocks);
                }

                IReadOnlyList<RemovedBlockInfo>
                    removedBlockInfos =
                        CaptureRemovedBlocks(
                            board,
                            effectResult.AffectedPositions);

                _stateMachine.TransitionTo(
                    PuzzleState.RemovingBlocks);

                IReadOnlyList<GridPosition>
                    removedPositions =
                        RemoveBlocks(
                            board,
                            effectResult.AffectedPositions);

                SpecialBlockCreation createdSpecialBlock =
                    null;

                if (createsSpecialBlock)
                {
                    createdSpecialBlock =
                        ReplaceSelectedBlockWithSpecial(
                            board,
                            selectedPosition,
                            selectedBlock,
                            createdSpecialBlockType);
                }

                _stateMachine.TransitionTo(
                    PuzzleState.ApplyingGravity);

                IReadOnlyList<GravityMove>
                    gravityMoves =
                        _gravitySystem.Apply(board);

                _stateMachine.TransitionTo(
                    PuzzleState.SpawningBlocks);

                IReadOnlyList<BlockSpawn>
                    spawnedBlocks =
                        _blockSpawnSystem.FillEmptyCells(
                            board);

                _stateMachine.TransitionTo(
                    PuzzleState.CheckingObjectives);

                _stateMachine.TransitionTo(
                    PuzzleState.WaitingForInput);

                return new PuzzleTurnResult(
                    true,
                    removedPositions,
                    removedBlockInfos,
                    gravityMoves,
                    spawnedBlocks,
                    effectResult.ActivatedSpecialBlocks,
                    createdSpecialBlock);
            }
            catch
            {
                if (_stateMachine.CurrentState
                    != PuzzleState.WaitingForInput)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.WaitingForInput);
                }

                throw;
            }
        }

        private static IReadOnlyList<GridPosition>
            CreatePositionsToAffect(
                IReadOnlyList<GridPosition> connectedPositions,
                GridPosition selectedPosition,
                bool preserveSelectedPosition)
        {
            List<GridPosition> positions =
                new List<GridPosition>(
                    connectedPositions.Count);

            for (int i = 0;
                 i < connectedPositions.Count;
                 i++)
            {
                GridPosition position =
                    connectedPositions[i];

                if (preserveSelectedPosition
                    && position == selectedPosition)
                {
                    continue;
                }

                positions.Add(position);
            }

            return positions;
        }

        private static IReadOnlyList<GridPosition>
            RemoveBlocks(
                Board board,
                IReadOnlyList<GridPosition> positions)
        {
            List<GridPosition> removedPositions =
                new List<GridPosition>(
                    positions.Count);

            for (int i = 0;
                 i < positions.Count;
                 i++)
            {
                GridPosition position =
                    positions[i];

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

        private static SpecialBlockCreation
            ReplaceSelectedBlockWithSpecial(
                Board board,
                GridPosition selectedPosition,
                Block selectedBlock,
                BlockType specialBlockType)
        {
            board.RemoveBlock(selectedPosition);

            Block specialBlock =
                new Block(
                    selectedBlock.Color,
                    specialBlockType);

            board.PlaceBlock(
                selectedPosition,
                specialBlock);

            return new SpecialBlockCreation(
                selectedPosition,
                specialBlock);
        }

        private static IReadOnlyList<RemovedBlockInfo>
            CaptureRemovedBlocks(
                Board board,
                IReadOnlyList<GridPosition> positions)
        {
            List<RemovedBlockInfo> removedBlocks =
                new List<RemovedBlockInfo>(
                    positions.Count);

            for (int i = 0;
                 i < positions.Count;
                 i++)
            {
                GridPosition position =
                    positions[i];

                Block block =
                    board.GetBlock(position);

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