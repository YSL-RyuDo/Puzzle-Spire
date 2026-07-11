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

            List<GravityMove> moves =
                new List<GravityMove>();

            for (int x = 0; x < board.Width; x++)
            {
                ApplyToColumn(board, x, moves);
            }

            return moves;
        }

        private static void ApplyToColumn(
            Board board,
            int columnX,
            List<GravityMove> moves)
        {
            List<GridPosition> playablePositions =
                CollectPlayablePositions(
                    board,
                    columnX);

            List<BlockEntry> blocks =
                CollectBlocks(
                    board,
                    playablePositions);

            RemoveBlocksFromOriginalPositions(
                board,
                blocks);

            PlaceBlocksAtLowestPlayablePositions(
                board,
                playablePositions,
                blocks,
                moves);
        }

        private static List<GridPosition>
            CollectPlayablePositions(
                Board board,
                int columnX)
        {
            List<GridPosition> playablePositions =
                new List<GridPosition>();

            // Y = 0부터 시작하므로 아래쪽부터 수집됩니다.
            for (int y = 0; y < board.Height; y++)
            {
                GridPosition position =
                    new GridPosition(columnX, y);

                Cell cell =
                    board.GetCell(position);

                if (!cell.IsPlayable)
                {
                    continue;
                }

                playablePositions.Add(position);
            }

            return playablePositions;
        }

        private static List<BlockEntry> CollectBlocks(
            Board board,
            IReadOnlyList<GridPosition> playablePositions)
        {
            List<BlockEntry> blocks =
                new List<BlockEntry>();

            for (int i = 0;
                 i < playablePositions.Count;
                 i++)
            {
                GridPosition position =
                    playablePositions[i];

                Block block =
                    board.GetBlock(position);

                if (block == null)
                {
                    continue;
                }

                blocks.Add(
                    new BlockEntry(
                        block,
                        position));
            }

            return blocks;
        }

        private static void
            RemoveBlocksFromOriginalPositions(
                Board board,
                IReadOnlyList<BlockEntry> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockEntry entry = blocks[i];

                board.RemoveBlock(
                    entry.OriginalPosition);
            }
        }

        private static void
            PlaceBlocksAtLowestPlayablePositions(
                Board board,
                IReadOnlyList<GridPosition> playablePositions,
                IReadOnlyList<BlockEntry> blocks,
                List<GravityMove> moves)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockEntry entry = blocks[i];

                GridPosition targetPosition =
                    playablePositions[i];

                // 안전장치
                if (!board.IsPlayable(targetPosition))
                {
                    throw new InvalidOperationException(
                        $"Gravity attempted to place a block " +
                        $"on a Void Cell: {targetPosition}");
                }

                board.PlaceBlock(
                    targetPosition,
                    entry.Block);

                if (entry.OriginalPosition == targetPosition)
                {
                    continue;
                }

                moves.Add(
                    new GravityMove(
                        entry.Block,
                        entry.OriginalPosition,
                        targetPosition));
            }
        }

        private readonly struct BlockEntry
        {
            public Block Block { get; }

            public GridPosition OriginalPosition { get; }

            public BlockEntry(
                Block block,
                GridPosition originalPosition)
            {
                Block = block
                    ?? throw new ArgumentNullException(
                        nameof(block));

                OriginalPosition = originalPosition;
            }
        }
    }
}