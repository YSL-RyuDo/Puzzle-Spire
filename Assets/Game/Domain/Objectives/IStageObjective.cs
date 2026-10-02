namespace PuzzleSpire.Domain.Puzzle
{
    public interface IStageObjective
    {
        string ObjectiveId { get; }

        int TargetAmount { get; }

        int CurrentAmount { get; }

        int RemainingAmount { get; }

        bool IsCompleted { get; }

        bool Apply(PuzzleTurnResult turnResult);

        void Reset();
    }
}