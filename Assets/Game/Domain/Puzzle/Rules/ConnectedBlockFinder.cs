using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class ConnectedBlockFinder
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(0, 1),
            new GridPosition(0, -1),
            new GridPosition(-1, 0),
            new GridPosition(1, 0)
        };

        public IReadOnlyList<GridPosition> Find(
            Board board,
            GridPosition startPosition)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!board.IsPlayable(startPosition))
            {
                return Array.Empty<GridPosition>();
            }

            Block startBlock = board.GetBlock(startPosition);

            if (startBlock == null
                || startBlock.Type != BlockType.Normal)
            {
                return Array.Empty<GridPosition>();
            }

            BlockColor targetColor = startBlock.Color;

            Queue<GridPosition> searchQueue =
                new Queue<GridPosition>();

            HashSet<GridPosition> visitedPositions =
                new HashSet<GridPosition>();

            List<GridPosition> connectedPositions =
                new List<GridPosition>();

            searchQueue.Enqueue(startPosition);
            visitedPositions.Add(startPosition);

            while (searchQueue.Count > 0)
            {
                GridPosition currentPosition =
                    searchQueue.Dequeue();

                connectedPositions.Add(currentPosition);

                for (int i = 0; i < Directions.Length; i++)
                {
                    GridPosition direction = Directions[i];

                    GridPosition nextPosition =
                        new GridPosition(
                            currentPosition.X + direction.X,
                            currentPosition.Y + direction.Y);

                    if (!board.IsPlayable(nextPosition))
                    {
                        continue;
                    }

                    if (visitedPositions.Contains(nextPosition))
                    {
                        continue;
                    }

                    Block nextBlock = board.GetBlock(nextPosition);

                    if (nextBlock == null
                        || nextBlock.Type != BlockType.Normal
                        || nextBlock.Color != targetColor)
                    {
                        continue;
                    }

                    visitedPositions.Add(nextPosition);
                    searchQueue.Enqueue(nextPosition);
                }
            }

            return connectedPositions;
        }
    }
}