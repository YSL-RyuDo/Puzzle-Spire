using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockEffectResult
    {
        public IReadOnlyList<GridPosition> AffectedPositions { get; }
        public IReadOnlyList<SpecialBlockActivation> ActivatedSpecialBlocks { get; }
        public IReadOnlyList<SpecialBlockTransformation> Transformations { get; }
        public IReadOnlyList<SpecialBlockMerge> Merges { get; }
        public IReadOnlyList<ObstacleImpactEvent> ObstacleImpactEvents { get; }

        public SpecialBlockEffectResult(
            IReadOnlyList<GridPosition> affectedPositions,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks)
            : this(
                affectedPositions,
                activatedSpecialBlocks,
                Array.Empty<SpecialBlockTransformation>(),
                Array.Empty<SpecialBlockMerge>(),
                Array.Empty<ObstacleImpactEvent>())
        {
        }

        public SpecialBlockEffectResult(
            IReadOnlyList<GridPosition> affectedPositions,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformations)
            : this(
                affectedPositions,
                activatedSpecialBlocks,
                transformations,
                Array.Empty<SpecialBlockMerge>(),
                Array.Empty<ObstacleImpactEvent>())
        {
        }

        public SpecialBlockEffectResult(
            IReadOnlyList<GridPosition> affectedPositions,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformations,
            IReadOnlyList<SpecialBlockMerge> merges)
            : this(
                affectedPositions,
                activatedSpecialBlocks,
                transformations,
                merges,
                Array.Empty<ObstacleImpactEvent>())
        {
        }

        public SpecialBlockEffectResult(
            IReadOnlyList<GridPosition> affectedPositions,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformations,
            IReadOnlyList<SpecialBlockMerge> merges,
            IReadOnlyList<ObstacleImpactEvent> obstacleImpactEvents)
        {
            AffectedPositions = CopyToArray(affectedPositions, nameof(affectedPositions));
            ActivatedSpecialBlocks = CopyToArray(activatedSpecialBlocks, nameof(activatedSpecialBlocks));
            Transformations = CopyToArray(transformations, nameof(transformations));
            Merges = CopyToArray(merges, nameof(merges));
            ObstacleImpactEvents = CopyToArray(obstacleImpactEvents, nameof(obstacleImpactEvents));
        }

        private static T[] CopyToArray<T>(IReadOnlyList<T> source, string parameterName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var result = new T[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                result[i] = source[i];
            }

            return result;
        }
    }
}