using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public static class CellObstacleExtensions
    {
        public static bool HasAnyObstacle(this Cell cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            for (int layerIndex = 0; layerIndex < (int)ObstacleLayer.Count; layerIndex++)
            {
                if (cell.HasObstacle((ObstacleLayer)layerIndex))
                {
                    return true;
                }
            }

            return false;
        }
    }
}