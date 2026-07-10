using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class BlockSpawnSystem
    {
        private readonly IBlockFactory _blockFactory;

        public BlockSpawnSystem(IBlockFactory blockFactory)
        {
            _blockFactory =
                blockFactory
                ?? throw new ArgumentNullException(
                    nameof(blockFactory));
        }

        public IReadOnlyList<BlockSpawn> FillEmptyCells(
            Board board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<BlockSpawn> spawnedBlocks =
                new List<BlockSpawn>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    GridPosition position =
                        new GridPosition(x, y);

                    if (!board.IsEmpty(position))
                    {
                        continue;
                    }

                    Block newBlock = _blockFactory.Create();

                    if (newBlock == null)
                    {
                        throw new InvalidOperationException(
                            "블록 팩토리가 null을 반환했습니다.");
                    }

                    board.PlaceBlock(position, newBlock);

                    spawnedBlocks.Add(
                        new BlockSpawn(
                            newBlock,
                            position));
                }
            }

            return spawnedBlocks;
        }
    }
}