using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class BlockSpawnSystemTests
    {
        [Test]
        public void FillEmptyCells_FillsEntireEmptyBoard()
        {
            Board board = new Board(3, 3);

            SequenceBlockFactory factory =
                new SequenceBlockFactory(
                    BlockColor.Red,
                    BlockColor.Blue,
                    BlockColor.Green);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var spawnedBlocks =
                spawnSystem.FillEmptyCells(board);

            Assert.That(
                spawnedBlocks,
                Has.Count.EqualTo(9));

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Assert.That(
                        board.IsEmpty(
                            new GridPosition(x, y)),
                        Is.False);
                }
            }
        }

        [Test]
        public void FillEmptyCells_PreservesExistingBlock()
        {
            Board board = new Board(2, 2);

            GridPosition existingPosition =
                new GridPosition(0, 0);

            Block existingBlock =
                new Block(BlockColor.Purple);

            board.PlaceBlock(
                existingPosition,
                existingBlock);

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Red);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var spawnedBlocks =
                spawnSystem.FillEmptyCells(board);

            Assert.That(
                spawnedBlocks,
                Has.Count.EqualTo(3));

            Assert.That(
                board.GetBlock(existingPosition),
                Is.SameAs(existingBlock));
        }

        [Test]
        public void FillEmptyCells_ReturnsSpawnedBlockInformation()
        {
            Board board = new Board(1, 1);

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Yellow);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var spawnedBlocks =
                spawnSystem.FillEmptyCells(board);

            Assert.That(
                spawnedBlocks,
                Has.Count.EqualTo(1));

            BlockSpawn spawn = spawnedBlocks[0];

            Assert.That(
                spawn.Position,
                Is.EqualTo(new GridPosition(0, 0)));

            Assert.That(
                spawn.Block,
                Is.SameAs(
                    board.GetBlock(
                        new GridPosition(0, 0))));

            Assert.That(
                spawn.Block.Color,
                Is.EqualTo(BlockColor.Yellow));
        }

        [Test]
        public void FillEmptyCells_OnFullBoard_ReturnsEmptyResult()
        {
            Board board = new Board(2, 2);

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    board.PlaceBlock(
                        new GridPosition(x, y),
                        new Block(BlockColor.Red));
                }
            }

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Blue);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var spawnedBlocks =
                spawnSystem.FillEmptyCells(board);

            Assert.That(spawnedBlocks, Is.Empty);
            Assert.That(factory.CreateCount, Is.EqualTo(0));
        }

        [Test]
        public void FillEmptyCells_CreatesOnlyForEmptyCells()
        {
            Board board = new Board(2, 3);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(1, 0),
                new Block(BlockColor.Blue));

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Green);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            var spawnedBlocks =
                spawnSystem.FillEmptyCells(board);

            Assert.That(
                spawnedBlocks,
                Has.Count.EqualTo(4));

            Assert.That(factory.CreateCount, Is.EqualTo(4));
        }

        [Test]
        public void FillEmptyCells_WithNullBoard_ThrowsException()
        {
            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Red);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            Assert.Throws<ArgumentNullException>(
                () => spawnSystem.FillEmptyCells(null));
        }

        [Test]
        public void Constructor_WithNullFactory_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BlockSpawnSystem(null));
        }

        [Test]
        public void FillEmptyCells_WhenFactoryReturnsNull_ThrowsException()
        {
            Board board = new Board(1, 1);

            NullBlockFactory factory =
                new NullBlockFactory();

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(factory);

            Assert.Throws<InvalidOperationException>(
                () => spawnSystem.FillEmptyCells(board));
        }

        [Test]
        public void RandomBlockFactory_CreatesAllowedColorOnly()
        {
            BlockColor[] availableColors =
            {
                BlockColor.Red,
                BlockColor.Blue
            };

            RandomBlockFactory factory =
                new RandomBlockFactory(
                    availableColors,
                    randomSeed: 1234);

            for (int i = 0; i < 20; i++)
            {
                Block block = factory.Create();

                bool isAllowedColor =
                    block.Color == BlockColor.Red
                    || block.Color == BlockColor.Blue;

                Assert.That(isAllowedColor, Is.True);
                Assert.That(
                    block.Type,
                    Is.EqualTo(BlockType.Normal));
            }
        }

        private sealed class SequenceBlockFactory
            : IBlockFactory
        {
            private readonly Queue<BlockColor> _colors;
            private readonly BlockColor _fallbackColor;

            public int CreateCount { get; private set; }

            public SequenceBlockFactory(
                params BlockColor[] colors)
            {
                if (colors == null || colors.Length == 0)
                {
                    throw new ArgumentException(
                        "색상이 하나 이상 필요합니다.",
                        nameof(colors));
                }

                _colors =
                    new Queue<BlockColor>(colors);

                _fallbackColor =
                    colors[colors.Length - 1];
            }

            public Block Create()
            {
                CreateCount++;

                BlockColor color =
                    _colors.Count > 0
                        ? _colors.Dequeue()
                        : _fallbackColor;

                return new Block(color);
            }
        }

        private sealed class NullBlockFactory
            : IBlockFactory
        {
            public Block Create()
            {
                return null;
            }
        }
    }
}