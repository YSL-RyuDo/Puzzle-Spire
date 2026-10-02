using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class PuzzleSessionTests
    {
        [Test]
        public void Constructor_SetsMoveLimitAndRemainingMoves()
        {
            PuzzleSession session =
                new PuzzleSession(20);

            Assert.That(
                session.MoveLimit,
                Is.EqualTo(20));

            Assert.That(
                session.RemainingMoves,
                Is.EqualTo(20));

            Assert.That(
                session.UsedMoves,
                Is.EqualTo(0));

            Assert.That(
                session.HasMovesRemaining,
                Is.True);
        }

        [Test]
        public void TryConsumeMove_WithResolvedTurn_DecreasesMove()
        {
            PuzzleSession session =
                new PuzzleSession(20);

            PuzzleTurnResult result =
                CreateResolvedTurnResult();

            bool consumed =
                session.TryConsumeMove(result);

            Assert.That(
                consumed,
                Is.True);

            Assert.That(
                session.RemainingMoves,
                Is.EqualTo(19));

            Assert.That(
                session.UsedMoves,
                Is.EqualTo(1));
        }

        [Test]
        public void TryConsumeMove_WithNoChangeTurn_DoesNotDecreaseMove()
        {
            PuzzleSession session =
                new PuzzleSession(20);

            PuzzleTurnResult result =
                PuzzleTurnResult.CreateNoChange();

            bool consumed =
                session.TryConsumeMove(result);

            Assert.That(
                consumed,
                Is.False);

            Assert.That(
                session.RemainingMoves,
                Is.EqualTo(20));

            Assert.That(
                session.UsedMoves,
                Is.EqualTo(0));
        }

        [Test]
        public void TryConsumeMove_RaisesRemainingMovesChangedEvent()
        {
            PuzzleSession session =
                new PuzzleSession(5);

            int receivedRemainingMoves =
                -1;

            session.RemainingMovesChanged +=
                remainingMoves =>
                    receivedRemainingMoves =
                        remainingMoves;

            session.TryConsumeMove(
                CreateResolvedTurnResult());

            Assert.That(
                receivedRemainingMoves,
                Is.EqualTo(4));
        }

        [Test]
        public void TryConsumeMove_WhenNoMovesRemain_ReturnsFalse()
        {
            PuzzleSession session =
                new PuzzleSession(1);

            PuzzleTurnResult result =
                CreateResolvedTurnResult();

            bool firstConsumed =
                session.TryConsumeMove(result);

            bool secondConsumed =
                session.TryConsumeMove(result);

            Assert.That(
                firstConsumed,
                Is.True);

            Assert.That(
                secondConsumed,
                Is.False);

            Assert.That(
                session.RemainingMoves,
                Is.EqualTo(0));

            Assert.That(
                session.UsedMoves,
                Is.EqualTo(1));

            Assert.That(
                session.HasMovesRemaining,
                Is.False);
        }

        [Test]
        public void Constructor_WithInvalidMoveLimit_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PuzzleSession(0));
        }

        [Test]
        public void TryConsumeMove_WithNullResult_ThrowsException()
        {
            PuzzleSession session =
                new PuzzleSession(20);

            Assert.Throws<ArgumentNullException>(
                () => session.TryConsumeMove(null));
        }

        private static PuzzleTurnResult
            CreateResolvedTurnResult()
        {
            GridPosition[] removedPositions =
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0)
            };

            return new PuzzleTurnResult(
                true,
                removedPositions,
                Array.Empty<GravityMove>(),
                Array.Empty<BlockSpawn>());
        }
    }
}