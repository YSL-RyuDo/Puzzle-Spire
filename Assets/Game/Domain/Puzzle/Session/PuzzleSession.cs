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

            if (!turnResult.WasResolved)
            {
                return false;
            }

            if (!HasMovesRemaining)
            {
                throw new InvalidOperationException(
                    "남아 있는 이동 횟수가 없습니다.");
            }

            RemainingMoves--;

            RemainingMovesChanged?.Invoke(
                RemainingMoves);

            return true;
        }
    }
}