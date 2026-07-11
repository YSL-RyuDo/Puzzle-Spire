using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleTurnResult
    {
        public bool WasResolved { get; }

        public IReadOnlyList<GridPosition>
            RemovedPositions
        { get; }

        public IReadOnlyList<RemovedBlockInfo>
            RemovedBlocks
        { get; }

        public IReadOnlyList<GravityMove>
            GravityMoves
        { get; }

        public IReadOnlyList<BlockSpawn>
            SpawnedBlocks
        { get; }

        public IReadOnlyList<SpecialBlockActivation>
            ActivatedSpecialBlocks
        { get; }

        public SpecialBlockCreation
            CreatedSpecialBlock
        { get; }

        public bool HasCreatedSpecialBlock =>
            CreatedSpecialBlock != null;

        public bool HasActivatedSpecialBlocks =>
            ActivatedSpecialBlocks.Count > 0;

        public int RemovedCount =>
            RemovedPositions.Count;

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks)
            : this(
                wasResolved,
                removedPositions,
                Array.Empty<RemovedBlockInfo>(),
                gravityMoves,
                spawnedBlocks,
                Array.Empty<SpecialBlockActivation>(),
                null)
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks)
            : this(
                wasResolved,
                removedPositions,
                removedBlocks,
                gravityMoves,
                spawnedBlocks,
                Array.Empty<SpecialBlockActivation>(),
                null)
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            SpecialBlockCreation createdSpecialBlock)
            : this(
                wasResolved,
                removedPositions,
                removedBlocks,
                gravityMoves,
                spawnedBlocks,
                Array.Empty<SpecialBlockActivation>(),
                createdSpecialBlock)
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            SpecialBlockCreation createdSpecialBlock)
        {
            WasResolved = wasResolved;

            RemovedPositions =
                CopyToArray(
                    removedPositions,
                    nameof(removedPositions));

            RemovedBlocks =
                CopyToArray(
                    removedBlocks,
                    nameof(removedBlocks));

            GravityMoves =
                CopyToArray(
                    gravityMoves,
                    nameof(gravityMoves));

            SpawnedBlocks =
                CopyToArray(
                    spawnedBlocks,
                    nameof(spawnedBlocks));

            ActivatedSpecialBlocks =
                CopyToArray(
                    activatedSpecialBlocks,
                    nameof(activatedSpecialBlocks));

            CreatedSpecialBlock =
                createdSpecialBlock;
        }

        public static PuzzleTurnResult CreateNoChange()
        {
            return new PuzzleTurnResult(
                false,
                Array.Empty<GridPosition>(),
                Array.Empty<RemovedBlockInfo>(),
                Array.Empty<GravityMove>(),
                Array.Empty<BlockSpawn>(),
                Array.Empty<SpecialBlockActivation>(),
                null);
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