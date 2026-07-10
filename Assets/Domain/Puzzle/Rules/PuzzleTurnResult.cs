using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleTurnResult
    {
        public bool WasResolved { get; }

        public IReadOnlyList<GridPosition> RemovedPositions { get; }

        public IReadOnlyList<GravityMove> GravityMoves { get; }

        public IReadOnlyList<BlockSpawn> SpawnedBlocks { get; }

        public int RemovedCount => RemovedPositions.Count;

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks)
        {
            WasResolved = wasResolved;

            RemovedPositions =
                CopyToArray(
                    removedPositions,
                    nameof(removedPositions));

            GravityMoves =
                CopyToArray(
                    gravityMoves,
                    nameof(gravityMoves));

            SpawnedBlocks =
                CopyToArray(
                    spawnedBlocks,
                    nameof(spawnedBlocks));
        }

        public static PuzzleTurnResult CreateNoChange()
        {
            return new PuzzleTurnResult(
                false,
                Array.Empty<GridPosition>(),
                Array.Empty<GravityMove>(),
                Array.Empty<BlockSpawn>());
        }

        private static T[] CopyToArray<T>(
            IReadOnlyList<T> source,
            string parameterName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(parameterName);
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