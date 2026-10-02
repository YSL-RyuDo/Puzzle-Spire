using System;

namespace PuzzleSpire.Domain.Puzzle
{
    [Flags]
    public enum ObstacleImpactKind
    {
        None = 0,
        Direct = 1 << 0,
        Adjacent = 1 << 1,
        SameCellBlockRemoved = 1 << 2
    }
}