using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class GravitySystem
    {
        public IReadOnlyList<GravityMove> Apply(
            Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board));
            }

            List<GravityMove> moves =
                new List<GravityMove>();

            for (int x = 0;
                 x < board.Width;
                 x++)
            {
                ApplyToColumn(
                    board,
                    x,
                    moves);
            }

            return moves;
        }

        private static void ApplyToColumn(
            Board board,
            int columnX,
            List<GravityMove> moves)
        {
            List<GridPosition> segment =
                new List<GridPosition>(
                    board.Height);

            for (int y = 0;
                 y < board.Height;
                 y++)
            {
                GridPosition position =
                    new GridPosition(
                        columnX,
                        y);

                Cell cell =
                    board.GetCell(position);

                if (!cell.IsPlayable)
                {
                    continue;
                }

                if (!cell.CanContainBlock)
                {
                    CompactSegment(
                        board,
                        segment,
                        moves);

                    segment.Clear();
                    continue;
                }

                segment.Add(position);
            }

            CompactSegment(
                board,
                segment,
                moves);
        }

        private static void CompactSegment(
            Board board,
            IReadOnlyList<GridPosition>
                positions,
            List<GravityMove> moves)
        {
            if (positions.Count == 0)
            {
                return;
            }

            List<BlockEntry> blocks =
                new List<BlockEntry>(
                    positions.Count);

            for (int i = 0;
                 i < positions.Count;
                 i++)
            {
                GridPosition position =
                    positions[i];

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

            for (int i = 0;
                 i < blocks.Count;
                 i++)
            {
                board.RemoveBlock(
                    blocks[i].OriginalPosition);
            }

            for (int i = 0;
                 i < blocks.Count;
                 i++)
            {
                BlockEntry entry =
                    blocks[i];

                GridPosition destination =
                    positions[i];

                board.PlaceBlock(
                    destination,
                    entry.Block);

                if (entry.OriginalPosition
                    == destination)
                {
                    continue;
                }

                moves.Add(
                    new GravityMove(
                        entry.Block,
                        entry.OriginalPosition,
                        destination));
            }
        }

        private readonly struct BlockEntry
        {
            public Block Block { get; }

            public GridPosition OriginalPosition
            {
                get;
            }

            public BlockEntry(
                Block block,
                GridPosition originalPosition)
            {
                Block =
                    block
                    ?? throw new ArgumentNullException(
                        nameof(block));

                OriginalPosition =
                    originalPosition;
            }
        }
    }
}