using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class BlockRemovalSystem
    {
        private readonly ConnectedBlockFinder _connectedBlockFinder;

        public int MinimumGroupSize { get; }

        public BlockRemovalSystem(
            ConnectedBlockFinder connectedBlockFinder,
            int minimumGroupSize = 2)
        {
            _connectedBlockFinder =
                connectedBlockFinder
                ?? throw new ArgumentNullException(
                    nameof(connectedBlockFinder));

            if (minimumGroupSize < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumGroupSize),
                    minimumGroupSize,
                    "최소 제거 개수는 2 이상이어야 합니다.");
            }

            MinimumGroupSize = minimumGroupSize;
        }

        public IReadOnlyList<GridPosition> Remove(
            Board board,
            GridPosition startPosition)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            IReadOnlyList<GridPosition> connectedPositions =
                _connectedBlockFinder.Find(
                    board,
                    startPosition);

            return Remove(board, connectedPositions);
        }

        public IReadOnlyList<GridPosition> Remove(
            Board board,
            IReadOnlyList<GridPosition> positions)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            HashSet<GridPosition> visitedPositions =
                new HashSet<GridPosition>();

            List<GridPosition> validPositions =
                new List<GridPosition>();

            for (int i = 0; i < positions.Count; i++)
            {
                GridPosition position = positions[i];

                if (!board.IsInside(position))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(positions),
                        position,
                        "보드 범위를 벗어난 좌표가 포함되어 있습니다.");
                }

                if (!visitedPositions.Add(position))
                {
                    continue;
                }

                if (board.IsEmpty(position))
                {
                    continue;
                }

                validPositions.Add(position);
            }

            if (validPositions.Count < MinimumGroupSize)
            {
                return Array.Empty<GridPosition>();
            }

            List<GridPosition> removedPositions =
                new List<GridPosition>(validPositions.Count);

            for (int i = 0; i < validPositions.Count; i++)
            {
                GridPosition position = validPositions[i];

                board.RemoveBlock(position);
                removedPositions.Add(position);
            }

            return removedPositions;
        }
    }
}