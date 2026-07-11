using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    public sealed class PuzzleController : MonoBehaviour
    {
        private Board _board;
        private BoardView _boardView;
        private PuzzleTurnResolver _turnResolver;
        private PuzzleStateMachine _stateMachine;
        private PuzzleSession _puzzleSession;
        private ObjectiveSystem _objectiveSystem;

        private bool _isAnimating;

        public bool IsInitialized { get; private set; }
        public Board Board => _board;

        public bool CanAcceptInput =>
            IsInitialized
            && !_isAnimating
            && _stateMachine != null
            && _stateMachine.CanAcceptInput
            && _puzzleSession != null
            && _puzzleSession.HasMovesRemaining
            && _objectiveSystem != null
            && !_objectiveSystem.AllCompleted;

        public void Initialize(
            Board board,
            BoardView boardView,
            PuzzleTurnResolver turnResolver,
            PuzzleStateMachine stateMachine,
            PuzzleSession puzzleSession,
            ObjectiveSystem objectiveSystem)
        {
            _board = board
                ?? throw new ArgumentNullException(
                    nameof(board));

            _boardView = boardView
                ?? throw new ArgumentNullException(
                    nameof(boardView));

            _turnResolver = turnResolver
                ?? throw new ArgumentNullException(
                    nameof(turnResolver));

            _stateMachine = stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _puzzleSession = puzzleSession
                ?? throw new ArgumentNullException(
                    nameof(puzzleSession));

            _objectiveSystem = objectiveSystem
                ?? throw new ArgumentNullException(
                    nameof(objectiveSystem));

            _isAnimating = false;
            IsInitialized = true;
        }

        public void SelectBlock(GridPosition position)
        {
            if (!CanAcceptInput)
            {
                return;
            }

            if (!_board.IsPlayable(position) || _board.IsEmpty(position))
            {
                return;
            }

            _isAnimating = true;

            try
            {
                PuzzleTurnResult result =
                    _turnResolver.Resolve(
                        _board,
                        position);

                if (!result.WasResolved)
                {
                    _isAnimating = false;
                    return;
                }

                _objectiveSystem.Apply(result);
                _puzzleSession.TryConsumeMove(result);

                Debug.Log(
                    $"제거 {result.RemovedCount}, " +
                    $"남은 이동 " +
                    $"{_puzzleSession.RemainingMoves}");

                _boardView.PlayTurnAnimation(
                    result,
                    HandleTurnAnimationCompleted);
            }
            catch
            {
                _isAnimating = false;
                throw;
            }
        }

        private void HandleTurnAnimationCompleted()
        {
            _isAnimating = false;

            if (_objectiveSystem.AllCompleted)
            {
                _stateMachine.TransitionTo(
                    PuzzleState.Cleared);

                Debug.Log("스테이지 클리어");
                return;
            }

            if (!_puzzleSession.HasMovesRemaining)
            {
                _stateMachine.TransitionTo(
                    PuzzleState.Failed);

                Debug.Log("스테이지 실패");
            }
        }

        private void OnDisable()
        {
            _isAnimating = false;
        }
    }
}