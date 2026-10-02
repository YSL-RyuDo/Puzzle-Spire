namespace PuzzleSpire.Domain.Puzzle
{
    public interface ICellObstacle
    {
        string DefinitionId { get; }

        CellObstacleType Type { get; }

        ObstacleLayer Layer { get; }
    }
}