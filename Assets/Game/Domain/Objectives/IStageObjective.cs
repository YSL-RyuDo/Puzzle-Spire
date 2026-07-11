namespace PuzzleSpire.Domain.Puzzle
{
    public interface IStageObjective
    {
        string ObjectiveId { get; }

        int CurrentAmount { get; }

        int TargetAmount { get; }

        int RemainingAmount { get; }

        bool IsCompleted { get; }

        bool Apply(PuzzleTurnResult turnResult);
    }
}