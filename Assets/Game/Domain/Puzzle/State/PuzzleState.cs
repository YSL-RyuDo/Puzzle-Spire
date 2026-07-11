namespace PuzzleSpire.Domain.Puzzle
{
    public enum PuzzleState
    {
        Initializing = 0,
        WaitingForInput = 1,
        FindingConnectedBlocks = 2,
        RemovingBlocks = 3,
        ActivatingSpecialBlocks = 4,
        ApplyingGravity = 5,
        SpawningBlocks = 6,
        CheckingObjectives = 7,
        Cleared = 8,
        Failed = 9,
        Paused = 10
    }
}