using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleStateMachine
    {
        public PuzzleState CurrentState { get; private set; }

        public bool CanAcceptInput =>
            CurrentState == PuzzleState.WaitingForInput;

        public event Action<PuzzleState> StateChanged;

        public PuzzleStateMachine(
            PuzzleState initialState = PuzzleState.Initializing)
        {
            ValidateState(initialState);

            CurrentState = initialState;
        }

        public void TransitionTo(PuzzleState nextState)
        {
            ValidateState(nextState);

            if (CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            StateChanged?.Invoke(CurrentState);
        }

        private static void ValidateState(PuzzleState state)
        {
            if (!Enum.IsDefined(typeof(PuzzleState), state))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "정의되지 않은 퍼즐 상태입니다.");
            }
        }
    }
}