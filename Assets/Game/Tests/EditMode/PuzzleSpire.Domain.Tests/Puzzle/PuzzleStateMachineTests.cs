using System;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Domain.Tests.Puzzle
{
    public sealed class PuzzleStateMachineTests
    {
        [Test]
        public void Constructor_SetsInitialState()
        {
            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine();

            Assert.That(
                stateMachine.CurrentState,
                Is.EqualTo(PuzzleState.Initializing));

            Assert.That(
                stateMachine.CanAcceptInput,
                Is.False);
        }

        [Test]
        public void TransitionToWaitingForInput_AllowsInput()
        {
            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine();

            stateMachine.TransitionTo(
                PuzzleState.WaitingForInput);

            Assert.That(
                stateMachine.CurrentState,
                Is.EqualTo(PuzzleState.WaitingForInput));

            Assert.That(
                stateMachine.CanAcceptInput,
                Is.True);
        }

        [Test]
        public void TransitionTo_RaisesStateChangedEvent()
        {
            PuzzleStateMachine stateMachine =
                new PuzzleStateMachine();

            PuzzleState receivedState =
                PuzzleState.Initializing;

            stateMachine.StateChanged +=
                state => receivedState = state;

            stateMachine.TransitionTo(
                PuzzleState.RemovingBlocks);

            Assert.That(
                receivedState,
                Is.EqualTo(PuzzleState.RemovingBlocks));
        }

        [Test]
        public void Constructor_WithInvalidState_ThrowsException()
        {
            PuzzleState invalidState =
                (PuzzleState)999;

            Assert.Throws<ArgumentOutOfRangeException>(
                () => new PuzzleStateMachine(invalidState));
        }
    }
}