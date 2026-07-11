using System;
using System.Collections.Generic;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class PuzzleTurnResolverTests
    {
        [Test]
        public void Resolve_RemovesAppliesGravityAndSpawnsBlocks()
        {
            Board board = CreateFullTestBoard();

            Block fallingBlock =
                board.GetBlock(new GridPosition(0, 2));

            SequenceBlockFactory factory =
                new SequenceBlockFactory(
                    BlockColor.Yellow,
                    BlockColor.Purple);

            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.WaitingForInput);

            PuzzleTurnResolver resolver =
                CreateResolver(factory, stateMachine);

            List<PuzzleState> changedStates =
                new List<PuzzleState>();

            stateMachine.StateChanged +=
                state => changedStates.Add(state);

            PuzzleTurnResult result =
                resolver.Resolve(
                    board,
                    new GridPosition(0, 0));

            Assert.That(result.WasResolved, Is.True);
            Assert.That(result.RemovedCount, Is.EqualTo(2));
            Assert.That(result.GravityMoves.Count,Is.EqualTo(1));
            Assert.That(result.SpawnedBlocks.Count,Is.EqualTo(2));

            Assert.That(
                board.GetBlock(new GridPosition(0, 0)),
                Is.SameAs(fallingBlock));

            Assert.That(
                board.IsEmpty(new GridPosition(0, 1)),
                Is.False);

            Assert.That(
                board.IsEmpty(new GridPosition(0, 2)),
                Is.False);

            Assert.That(
                stateMachine.CurrentState,
                Is.EqualTo(PuzzleState.WaitingForInput));

            Assert.That(changedStates.Count,Is.EqualTo(6));

            Assert.That(
                changedStates[0],
                Is.EqualTo(
                    PuzzleState.FindingConnectedBlocks));

            Assert.That(
                changedStates[1],
                Is.EqualTo(
                    PuzzleState.RemovingBlocks));

            Assert.That(
                changedStates[2],
                Is.EqualTo(
                    PuzzleState.ApplyingGravity));

            Assert.That(
                changedStates[3],
                Is.EqualTo(
                    PuzzleState.SpawningBlocks));

            Assert.That(
                changedStates[4],
                Is.EqualTo(
                    PuzzleState.CheckingObjectives));

            Assert.That(
                changedStates[5],
                Is.EqualTo(
                    PuzzleState.WaitingForInput));
        }

        [Test]
        public void Resolve_WithSingleBlock_DoesNotChangeBoard()
        {
            Board board = new Board(1, 1);

            Block originalBlock =
                new Block(BlockColor.Red);

            board.PlaceBlock(
                new GridPosition(0, 0),
                originalBlock);

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Blue);

            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.WaitingForInput);

            PuzzleTurnResolver resolver =
                CreateResolver(factory, stateMachine);

            PuzzleTurnResult result =
                resolver.Resolve(
                    board,
                    new GridPosition(0, 0));

            Assert.That(result.WasResolved, Is.False);
            Assert.That(result.RemovedPositions, Is.Empty);
            Assert.That(result.GravityMoves, Is.Empty);
            Assert.That(result.SpawnedBlocks, Is.Empty);

            Assert.That(
                board.GetBlock(new GridPosition(0, 0)),
                Is.SameAs(originalBlock));

            Assert.That(factory.CreateCount, Is.EqualTo(0));

            Assert.That(
                stateMachine.CurrentState,
                Is.EqualTo(PuzzleState.WaitingForInput));
        }

        [Test]
        public void Resolve_WhenNotWaitingForInput_ThrowsException()
        {
            Board board = new Board(1, 1);

            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Red);

            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.Initializing);

            PuzzleTurnResolver resolver =
                CreateResolver(factory, stateMachine);

            Assert.Throws<InvalidOperationException>(
                () => resolver.Resolve(
                    board,
                    new GridPosition(0, 0)));
        }

        [Test]
        public void Resolve_WithNullBoard_ThrowsException()
        {
            SequenceBlockFactory factory =
                new SequenceBlockFactory(BlockColor.Red);

            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.WaitingForInput);

            PuzzleTurnResolver resolver =
                CreateResolver(factory, stateMachine);

            Assert.Throws<ArgumentNullException>(
                () => resolver.Resolve(
                    null,
                    new GridPosition(0, 0)));
        }

        private static Board CreateFullTestBoard()
        {
            Board board = new Board(2, 3);

            board.PlaceBlock(
                new GridPosition(0, 0),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(0, 1),
                new Block(BlockColor.Red));

            board.PlaceBlock(
                new GridPosition(0, 2),
                new Block(BlockColor.Blue));

            board.PlaceBlock(
                new GridPosition(1, 0),
                new Block(BlockColor.Green));

            board.PlaceBlock(
                new GridPosition(1, 1),
                new Block(BlockColor.Yellow));

            board.PlaceBlock(
                new GridPosition(1, 2),
                new Block(BlockColor.Purple));

            return board;
        }

        private static PuzzleTurnResolver CreateResolver(
            IBlockFactory blockFactory,
            PuzzleStateMachine stateMachine)
        {
            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            BlockRemovalSystem removalSystem =
                new BlockRemovalSystem(finder);

            GravitySystem gravitySystem =
                new GravitySystem();

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(blockFactory);

            return new PuzzleTurnResolver(
                finder,
                removalSystem,
                gravitySystem,
                spawnSystem,
                stateMachine);
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
    }
}