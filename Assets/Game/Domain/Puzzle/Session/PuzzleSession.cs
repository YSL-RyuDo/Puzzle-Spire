using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleSession
    {
        public int MoveLimit { get; }

        public int RemainingMoves { get; private set; }

        public int UsedMoves => MoveLimit - RemainingMoves;

        public bool HasMovesRemaining => RemainingMoves > 0;

        public event Action<int> RemainingMovesChanged;

        public PuzzleSession(int moveLimit)
        {
            if (moveLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveLimit),
                    moveLimit,
                    "이동 횟수 제한은 1 이상이어야 합니다.");
            }

            MoveLimit = moveLimit;
            RemainingMoves = moveLimit;
        }

        public bool TryConsumeMove(PuzzleTurnResult turnResult)
        {
            if (turnResult == null)
            {
                throw new ArgumentNullException(
                    nameof(turnResult));
            }

            // 실제 블록 제거가 발생하지 않은 클릭은
            // 이동 횟수를 소모하지 않습니다.
            if (!turnResult.WasResolved)
            {
                return false;
            }

            // 이동 횟수가 이미 0이면 예외를 발생시키지 않고
            // 단순히 소모 실패로 처리합니다.
            if (!HasMovesRemaining)
            {
                return false;
            }

            RemainingMoves--;

            RemainingMovesChanged?.Invoke(
                RemainingMoves);

            return true;
        }
    }
}