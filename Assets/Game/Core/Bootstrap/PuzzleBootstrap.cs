using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using PuzzleSpire.Presentation.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    public sealed class PuzzleBootstrap : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private BoardView _boardView;

        [SerializeField]
        private PuzzleController _puzzleController;

        [SerializeField]
        private PuzzleHudView _puzzleHudView;

        [Header("Stage")]

        [SerializeField]
        private StageData _stageData;

        private Board _board;
        private PuzzleStateMachine _stateMachine;
        private PuzzleSession _puzzleSession;
        private ObjectiveSystem _objectiveSystem;

        public Board Board => _board;

        public StageData StageData => _stageData;

        public PuzzleSession PuzzleSession =>
            _puzzleSession;

        private void Start()
        {
            InitializePuzzle();
        }

        private void InitializePuzzle()
        {
            ValidateReferences();

            _stageData.ValidateOrThrow();

            IReadOnlyList<GridPosition> voidPositions =
                _stageData.CreateVoidPositions();

            _board =
                new Board(
                    _stageData.Width,
                    _stageData.Height,
                    voidPositions);

            BlockColor[] availableColors =
                _stageData.CreateAvailableColors();

            IBlockFactory blockFactory =
                new RandomBlockFactory(
                    availableColors,
                    _stageData.RandomSeed);

            BlockSpawnSystem spawnSystem =
                new BlockSpawnSystem(
                    blockFactory);

            ConnectedBlockFinder finder =
                new ConnectedBlockFinder();

            BlockRemovalSystem removalSystem =
                new BlockRemovalSystem(
                    finder,
                    minimumGroupSize: 2);

            GravitySystem gravitySystem =
                new GravitySystem();

            _stateMachine =
                new PuzzleStateMachine(
                    PuzzleState.Initializing);

            PuzzleTurnResolver turnResolver =
                new PuzzleTurnResolver(
                    finder,
                    removalSystem,
                    gravitySystem,
                    spawnSystem,
                    _stateMachine);

            _puzzleSession =
                new PuzzleSession(
                    _stageData.MoveLimit);

            RemoveColorObjective objective =
                new RemoveColorObjective(
                    "remove_target_color",
                    _stageData.TargetColor,
                    _stageData.TargetAmount);

            _objectiveSystem =
                new ObjectiveSystem(
                    new IStageObjective[]
                    {
                        objective
                    });

            spawnSystem.FillEmptyCells(_board);

            _boardView.Rebuild(_board);

            _puzzleController.Initialize(
                _board,
                _boardView,
                turnResolver,
                _stateMachine,
                _puzzleSession,
                _objectiveSystem);

            _puzzleHudView.Bind(
                _puzzleSession,
                _objectiveSystem,
                _stateMachine);

            _stateMachine.TransitionTo(
                PuzzleState.WaitingForInput);

            Debug.Log(
                $"Stage loaded: {_stageData.StageId}");
        }

        private void ValidateReferences()
        {
            if (_boardView == null)
            {
                throw new InvalidOperationException(
                    "BoardView is not assigned.");
            }

            if (_puzzleController == null)
            {
                throw new InvalidOperationException(
                    "PuzzleController is not assigned.");
            }

            if (_puzzleHudView == null)
            {
                throw new InvalidOperationException(
                    "PuzzleHudView is not assigned.");
            }

            if (_stageData == null)
            {
                throw new InvalidOperationException(
                    "StageData is not assigned.");
            }
        }
    }
}