using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class PuzzleTurnResult
    {
        public bool WasResolved { get; }
        public IReadOnlyList<GridPosition> RemovedPositions { get; }
        public IReadOnlyList<RemovedBlockInfo> RemovedBlocks { get; }
        public IReadOnlyList<GravityMove> GravityMoves { get; }
        public IReadOnlyList<BlockSpawn> SpawnedBlocks { get; }
        public IReadOnlyList<SpecialBlockTransformation> TransformedSpecialBlocks { get; }
        public IReadOnlyList<SpecialBlockActivation> ActivatedSpecialBlocks { get; }
        public IReadOnlyList<SpecialBlockMerge> MergedSpecialBlocks { get; }
        public IReadOnlyList<ObstacleStateChange> ObstacleChanges { get; }
        public SpecialBlockCreation CreatedSpecialBlock { get; }

        public int RemovedCount => RemovedPositions.Count;
        public bool HasCreatedSpecialBlock => CreatedSpecialBlock != null;
        public bool HasTransformations => TransformedSpecialBlocks.Count > 0;
        public bool HasActivatedSpecialBlocks => ActivatedSpecialBlocks.Count > 0;
        public bool HasMergedSpecialBlocks => MergedSpecialBlocks.Count > 0;
        public bool HasObstacleChanges => ObstacleChanges.Count > 0;

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
                Array.Empty<SpecialBlockTransformation>(),
                Array.Empty<SpecialBlockActivation>(),
                Array.Empty<SpecialBlockMerge>(),
                null,
                Array.Empty<ObstacleStateChange>())
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
                Array.Empty<SpecialBlockTransformation>(),
                Array.Empty<SpecialBlockActivation>(),
                Array.Empty<SpecialBlockMerge>(),
                null,
                Array.Empty<ObstacleStateChange>())
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
                Array.Empty<SpecialBlockTransformation>(),
                Array.Empty<SpecialBlockActivation>(),
                Array.Empty<SpecialBlockMerge>(),
                createdSpecialBlock,
                Array.Empty<ObstacleStateChange>())
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
            : this(
                wasResolved,
                removedPositions,
                removedBlocks,
                gravityMoves,
                spawnedBlocks,
                Array.Empty<SpecialBlockTransformation>(),
                activatedSpecialBlocks,
                Array.Empty<SpecialBlockMerge>(),
                createdSpecialBlock,
                Array.Empty<ObstacleStateChange>())
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformedSpecialBlocks,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            SpecialBlockCreation createdSpecialBlock)
            : this(
                wasResolved,
                removedPositions,
                removedBlocks,
                gravityMoves,
                spawnedBlocks,
                transformedSpecialBlocks,
                activatedSpecialBlocks,
                Array.Empty<SpecialBlockMerge>(),
                createdSpecialBlock,
                Array.Empty<ObstacleStateChange>())
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformedSpecialBlocks,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            IReadOnlyList<SpecialBlockMerge> mergedSpecialBlocks,
            SpecialBlockCreation createdSpecialBlock)
            : this(
                wasResolved,
                removedPositions,
                removedBlocks,
                gravityMoves,
                spawnedBlocks,
                transformedSpecialBlocks,
                activatedSpecialBlocks,
                mergedSpecialBlocks,
                createdSpecialBlock,
                Array.Empty<ObstacleStateChange>())
        {
        }

        public PuzzleTurnResult(
            bool wasResolved,
            IReadOnlyList<GridPosition> removedPositions,
            IReadOnlyList<RemovedBlockInfo> removedBlocks,
            IReadOnlyList<GravityMove> gravityMoves,
            IReadOnlyList<BlockSpawn> spawnedBlocks,
            IReadOnlyList<SpecialBlockTransformation> transformedSpecialBlocks,
            IReadOnlyList<SpecialBlockActivation> activatedSpecialBlocks,
            IReadOnlyList<SpecialBlockMerge> mergedSpecialBlocks,
            SpecialBlockCreation createdSpecialBlock,
            IReadOnlyList<ObstacleStateChange> obstacleChanges)
        {
            WasResolved = wasResolved;
            RemovedPositions = CopyToArray(removedPositions, nameof(removedPositions));
            RemovedBlocks = CopyToArray(removedBlocks, nameof(removedBlocks));
            GravityMoves = CopyToArray(gravityMoves, nameof(gravityMoves));
            SpawnedBlocks = CopyToArray(spawnedBlocks, nameof(spawnedBlocks));
            TransformedSpecialBlocks = CopyToArray(transformedSpecialBlocks, nameof(transformedSpecialBlocks));
            ActivatedSpecialBlocks = CopyToArray(activatedSpecialBlocks, nameof(activatedSpecialBlocks));
            MergedSpecialBlocks = CopyToArray(mergedSpecialBlocks, nameof(mergedSpecialBlocks));
            ObstacleChanges = CopyToArray(obstacleChanges, nameof(obstacleChanges));
            CreatedSpecialBlock = createdSpecialBlock;
        }

        public static PuzzleTurnResult CreateNoChange()
        {
            return new PuzzleTurnResult(
                false,
                Array.Empty<GridPosition>(),
                Array.Empty<RemovedBlockInfo>(),
                Array.Empty<GravityMove>(),
                Array.Empty<BlockSpawn>(),
                Array.Empty<SpecialBlockTransformation>(),
                Array.Empty<SpecialBlockActivation>(),
                Array.Empty<SpecialBlockMerge>(),
                null,
                Array.Empty<ObstacleStateChange>());
        }

        private static T[] CopyToArray<T>(IReadOnlyList<T> source, string parameterName)
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