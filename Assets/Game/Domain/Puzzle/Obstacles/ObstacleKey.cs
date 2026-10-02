using System;

namespace PuzzleSpire.Domain.Puzzle
{
    public readonly struct ObstacleKey :
        IEquatable<ObstacleKey>
    {
        public GridPosition Position { get; }

        public ObstacleLayer Layer { get; }

        public ObstacleKey(
            GridPosition position,
            ObstacleLayer layer)
        {
            Position = position;
            Layer = layer;
        }

        public bool Equals(
            ObstacleKey other)
        {
            return Position == other.Position
                && Layer == other.Layer;
        }

        public override bool Equals(
            object obj)
        {
            return obj is ObstacleKey other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Position,
                Layer);
        }
    }
}