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
        [SerializeField] private BoardView _boardView;
        [SerializeField] private PuzzleController _puzzleController;
        [SerializeField] private PuzzleHudView _puzzleHudView;

        [Header("Stage")]
        [SerializeField] private StageData _stageData;

        private Board _board;
        private PuzzleStateMachine _stateMachine;
        private PuzzleSession _puzzleSession;
        private ObjectiveSystem _objectiveSystem;

        public Board Board => _board;
        public StageData StageData => _stageData;
        public PuzzleSession PuzzleSession => _puzzleSession;

        private void Start()
        {
            InitializePuzzle();
        }

        private void InitializePuzzle()
        {
            ValidateReferences();
            _stageData.ValidateOrThrow();

            _board = new Board(
                _stageData.Width,
                _stageData.Height,
                _stageData.CreateVoidPositions());

            PlaceStageObstacles();

            IBlockFactory blockFactory = new RandomBlockFactory(
                _stageData.CreateAvailableColors(),
                _stageData.RandomSeed);

            var spawnSystem = new BlockSpawnSystem(blockFactory);
            var finder = new ConnectedBlockFinder();
            var removalSystem = new BlockRemovalSystem(
                finder,
                minimumGroupSize: 2);
            var gravitySystem = new GravitySystem();

            _stateMachine = new PuzzleStateMachine(
                PuzzleState.Initializing);

            var turnResolver = new PuzzleTurnResolver(
                finder,
                removalSystem,
                gravitySystem,
                spawnSystem,
                _stateMachine);

            _puzzleSession = new PuzzleSession(
                _stageData.MoveLimit);

            _objectiveSystem = new ObjectiveSystem(
                CreateObjectives());

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

            Debug.Log($"Stage loaded: {_stageData.StageId}");
        }

        private List<IStageObjective> CreateObjectives()
        {
            var objectives = new List<IStageObjective>(3);

            if (_stageData.UseColorObjective)
            {
                objectives.Add(
                    new RemoveColorObjective(
                        "remove_target_color",
                        _stageData.TargetColor,
                        _stageData.TargetAmount));
            }

            if (_stageData.BoxTargetAmount > 0)
            {
                objectives.Add(
                    new RemoveBoxObjective(
                        "remove_boxes",
                        _stageData.BoxTargetAmount));
            }

            if (_stageData.IceTargetAmount > 0)
            {
                objectives.Add(
                    new RemoveIceObjective(
                        "remove_ice",
                        _stageData.IceTargetAmount));
            }

            if (objectives.Count == 0)
            {
                throw new InvalidOperationException(
                    "스테이지 목표가 하나도 없습니다.");
            }

            return objectives;
        }

        private void PlaceStageObstacles()
        {
            IReadOnlyList<ObstaclePlacementData> placements =
                _stageData.ObstaclePlacements;

            for (int i = 0; i < placements.Count; i++)
            {
                ObstaclePlacementData placement = placements[i];

                if (placement == null)
                {
                    throw new InvalidOperationException(
                        $"Obstacle placement {i}가 null입니다.");
                }

                if (placement.Definition == null)
                {
                    throw new InvalidOperationException(
                        $"Obstacle placement {i}의 Definition이 없습니다.");
                }

                ICellObstacle runtimeObstacle =
                    placement.Definition.CreateRuntimeObstacle();

                _board.PlaceObstacle(
                    placement.Position,
                    runtimeObstacle);
            }
        }

        private void ValidateReferences()
        {
            if (_boardView == null)
            {
                throw new InvalidOperationException(
                    "BoardView가 연결되지 않았습니다.");
            }

            if (_puzzleController == null)
            {
                throw new InvalidOperationException(
                    "PuzzleController가 연결되지 않았습니다.");
            }

            if (_puzzleHudView == null)
            {
                throw new InvalidOperationException(
                    "PuzzleHudView가 연결되지 않았습니다.");
            }

            if (_stageData == null)
            {
                throw new InvalidOperationException(
                    "StageData가 연결되지 않았습니다.");
            }
        }
    }
}