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

        public PuzzleTurnResolver(
            ConnectedBlockFinder connectedBlockFinder,
            BlockRemovalSystem blockRemovalSystem,
            GravitySystem gravitySystem,
            BlockSpawnSystem blockSpawnSystem,
            PuzzleStateMachine stateMachine)
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
                    $"현재 상태에서는 입력할 수 없습니다: " +
                    $"{_stateMachine.CurrentState}");
            }

            try
            {
                _stateMachine.TransitionTo(
                    PuzzleState.FindingConnectedBlocks);

                IReadOnlyList<GridPosition> connectedPositions =
                    _connectedBlockFinder.Find(
                        board,
                        selectedPosition);

                if (connectedPositions.Count <
                    _blockRemovalSystem.MinimumGroupSize)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.WaitingForInput);

                    return PuzzleTurnResult.CreateNoChange();
                }

                _stateMachine.TransitionTo(
                    PuzzleState.RemovingBlocks);

                IReadOnlyList<GridPosition> removedPositions =
                    _blockRemovalSystem.Remove(
                        board,
                        connectedPositions);

                _stateMachine.TransitionTo(
                    PuzzleState.ApplyingGravity);

                IReadOnlyList<GravityMove> gravityMoves =
                    _gravitySystem.Apply(board);

                _stateMachine.TransitionTo(
                    PuzzleState.SpawningBlocks);

                IReadOnlyList<BlockSpawn> spawnedBlocks =
                    _blockSpawnSystem.FillEmptyCells(board);

                _stateMachine.TransitionTo(
                    PuzzleState.CheckingObjectives);

                _stateMachine.TransitionTo(
                    PuzzleState.WaitingForInput);

                return new PuzzleTurnResult(
                    true,
                    removedPositions,
                    gravityMoves,
                    spawnedBlocks);
            }
            catch
            {
                if (_stateMachine.CurrentState !=
                    PuzzleState.WaitingForInput)
                {
                    _stateMachine.TransitionTo(
                        PuzzleState.WaitingForInput);
                }

                throw;
            }
        }
    }
}