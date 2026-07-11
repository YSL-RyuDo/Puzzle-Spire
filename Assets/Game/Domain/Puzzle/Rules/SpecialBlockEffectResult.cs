using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockEffectResult
    {
        public IReadOnlyList<GridPosition>
            AffectedPositions
        { get; }

        public IReadOnlyList<SpecialBlockActivation>
            ActivatedSpecialBlocks
        { get; }

        public SpecialBlockEffectResult(
            IReadOnlyList<GridPosition> affectedPositions,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks)
        {
            AffectedPositions =
                CopyToArray(
                    affectedPositions,
                    nameof(affectedPositions));

            ActivatedSpecialBlocks =
                CopyToArray(
                    activatedSpecialBlocks,
                    nameof(activatedSpecialBlocks));
        }

        private static T[] CopyToArray<T>(
            IReadOnlyList<T> source,
            string parameterName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            T[] result = new T[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                result[i] = source[i];
            }

            return result;
        }
    }
}