using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class GravitySystem
    {
        public IReadOnlyList<GravityMove> Apply(Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<GravityMove> moves = new List<GravityMove>();

            for (int x = 0; x < board.Width; x++)
            {
                ApplyToColumn(board, x, moves);
            }

            return moves;
        }

        private static void ApplyToColumn(
            Board board,
            int x,
            List<GravityMove> moves)
        {
            int targetY = 0;

            for (int sourceY = 0;
                 sourceY < board.Height;
                 sourceY++)
            {
                GridPosition sourcePosition =
                    new GridPosition(x, sourceY);

                Block block = board.GetBlock(sourcePosition);

                if (block == null)
                {
                    continue;
                }

                if (sourceY != targetY)
                {
                    GridPosition targetPosition =
                        new GridPosition(x, targetY);

                    Block movedBlock =
                        board.RemoveBlock(sourcePosition);

                    board.PlaceBlock(
                        targetPosition,
                        movedBlock);

                    moves.Add(
                        new GravityMove(
                            movedBlock,
                            sourcePosition,
                            targetPosition));
                }

                targetY++;
            }
        }
    }
}