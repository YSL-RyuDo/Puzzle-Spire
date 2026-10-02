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
        private PuzzleTurnResult _pendingTurnResult;

        private bool _isAnimating;

        public bool IsInitialized { get; private set; }
        public Board Board => _board;

        public bool CanAcceptInput =>
            IsInitialized
            && !_isAnimating
            && _pendingTurnResult == null
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

            _pendingTurnResult = null;
            _isAnimating = false;
            IsInitialized = true;
        }

        public void SelectBlock(GridPosition position)
        {
            if (!CanAcceptInput)
            {
                return;
            }

            if (!_board.IsPlayable(position)
                || _board.IsEmpty(position))
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

                _pendingTurnResult = result;

                _boardView.PlayTurnAnimation(
                    result,
                    HandleTurnAnimationCompleted);
            }
            catch
            {
                _pendingTurnResult = null;
                _isAnimating = false;
                throw;
            }
        }

        private void HandleTurnAnimationCompleted()
        {
            PuzzleTurnResult completedTurn =
                _pendingTurnResult;

            _pendingTurnResult = null;
            _isAnimating = false;

            if (completedTurn == null)
            {
                Debug.LogError(
                    "턴 애니메이션은 완료됐지만 " +
                    "처리할 PuzzleTurnResult가 없습니다.");

                return;
            }

            if (!completedTurn.WasResolved)
            {
                return;
            }

            // 1. 제거된 블록을 목표에 먼저 반영
            _objectiveSystem.Apply(completedTurn);

            // 2. 정상적으로 처리된 턴의 이동 횟수 차감
            _puzzleSession.TryConsumeMove(completedTurn);

            Debug.Log(
                $"TURN COMPLETE | " +
                $"REMOVED {completedTurn.RemovedCount} | " +
                $"MOVES {_puzzleSession.RemainingMoves}");

            // 3. 마지막 이동에서 목표를 달성할 수 있으므로
            // 반드시 실패보다 클리어를 먼저 검사
            if (_objectiveSystem.AllCompleted)
            {
                _stateMachine.TransitionTo(
                    PuzzleState.Cleared);

                Debug.Log("STAGE CLEAR");

                return;
            }

            // 4. 목표 미달성 + 이동 횟수 0이면 실패
            if (!_puzzleSession.HasMovesRemaining)
            {
                _stateMachine.TransitionTo(
                    PuzzleState.Failed);

                Debug.Log("STAGE FAILED");

                return;
            }

            // 5. 아직 게임이 끝나지 않았으면 다음 입력 대기
            if (_stateMachine.CurrentState
                != PuzzleState.WaitingForInput)
            {
                _stateMachine.TransitionTo(
                    PuzzleState.WaitingForInput);
            }
        }

        private void OnDisable()
        {
            _pendingTurnResult = null;
            _isAnimating = false;
        }
    }
}