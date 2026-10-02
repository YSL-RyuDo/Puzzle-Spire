using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public interface IObstacleImpactSystem
    {
        IReadOnlyList<ObstacleStateChange> ApplyImpacts(
            Board board,
            IReadOnlyList<ObstacleImpactEvent> directImpactEvents,
            IReadOnlyList<GridPosition> removedBlockPositions);
    }
}