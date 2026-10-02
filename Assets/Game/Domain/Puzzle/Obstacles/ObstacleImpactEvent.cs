using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class ObstacleImpactEvent
    {
        public ObstacleImpactKind Kind { get; }
        public IReadOnlyList<GridPosition> Positions { get; }

        public ObstacleImpactEvent(
            ObstacleImpactKind kind,
            IReadOnlyList<GridPosition> positions)
        {
            if (kind == ObstacleImpactKind.None)
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Impact kind cannot be None.");
            }

            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            Kind = kind;
            Positions = CopyDistinctPositions(positions);
        }

        private static GridPosition[] CopyDistinctPositions(IReadOnlyList<GridPosition> source)
        {
            var result = new List<GridPosition>(source.Count);
            var added = new HashSet<GridPosition>();

            for (int i = 0; i < source.Count; i++)
            {
                if (added.Add(source[i]))
                {
                    result.Add(source[i]);
                }
            }

            return result.ToArray();
        }
    }
}