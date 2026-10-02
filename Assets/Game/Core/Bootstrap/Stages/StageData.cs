using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    [CreateAssetMenu(
        fileName = "StageData",
        menuName = "Puzzle Spire/Stages/Stage Data")]
    public sealed class StageData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _stageId = "stage_001";

        [Header("Board")]
        [SerializeField, Min(1)] private int _width = 9;
        [SerializeField, Min(1)] private int _height = 9;
        [SerializeField] private int _randomSeed = 1001;
        [SerializeField] private string[] _layoutRows = Array.Empty<string>();

        [Header("Available Colors")]
        [SerializeField]
        private BlockColor[] _availableColors =
        {
            BlockColor.Red,
            BlockColor.Blue,
            BlockColor.Green,
            BlockColor.Yellow
        };

        [Header("Stage Rules")]
        [SerializeField, Min(1)] private int _moveLimit = 20;
        [SerializeField] private bool _useColorObjective = true;
        [SerializeField] private BlockColor _targetColor = BlockColor.Red;
        [SerializeField, Min(1)] private int _targetAmount = 10;
        [SerializeField, Min(0)] private int _boxTargetAmount;
        [SerializeField, Min(0)] private int _iceTargetAmount;

        [Header("Obstacle Placements")]
        [SerializeField]
        private ObstaclePlacementData[] _obstaclePlacements =
            Array.Empty<ObstaclePlacementData>();

        public string StageId => _stageId;
        public int Width => _width;
        public int Height => _height;
        public int RandomSeed => _randomSeed;
        public int MoveLimit => _moveLimit;
        public bool UseColorObjective => _useColorObjective;
        public BlockColor TargetColor => _targetColor;
        public int TargetAmount => _targetAmount;
        public int BoxTargetAmount => _boxTargetAmount;
        public int IceTargetAmount => _iceTargetAmount;

        public IReadOnlyList<ObstaclePlacementData> ObstaclePlacements =>
            _obstaclePlacements;

        public BlockColor[] CreateAvailableColors()
        {
            var result = new BlockColor[_availableColors.Length];
            Array.Copy(_availableColors, result, _availableColors.Length);
            return result;
        }

        public IReadOnlyList<GridPosition> CreateBoxPositions()
        {
            return CreateObstaclePositions(CellObstacleType.Box);
        }

        public IReadOnlyList<GridPosition> CreateIcePositions()
        {
            return CreateObstaclePositions(CellObstacleType.Ice);
        }

        public IReadOnlyList<GridPosition> CreateObstaclePositions(
            CellObstacleType type)
        {
            var positions = new List<GridPosition>();

            for (int i = 0; i < _obstaclePlacements.Length; i++)
            {
                ObstaclePlacementData placement = _obstaclePlacements[i];

                if (placement?.Definition != null &&
                    placement.Definition.Type == type)
                {
                    positions.Add(placement.Position);
                }
            }

            return positions;
        }

        public IReadOnlyList<GridPosition> CreateVoidPositions()
        {
            var positions = new List<GridPosition>();

            for (int row = 0; row < _layoutRows.Length; row++)
            {
                string rowValue = _layoutRows[row];
                int y = _height - 1 - row;

                for (int x = 0; x < rowValue.Length; x++)
                {
                    char value = rowValue[x];

                    if (value == 'X' || value == 'x')
                    {
                        positions.Add(new GridPosition(x, y));
                    }
                }
            }

            return positions;
        }

        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_stageId))
            {
                throw new InvalidOperationException(
                    $"{name}: Stage ID가 비어 있습니다.");
            }

            if (_width <= 0 || _height <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 보드 크기는 1 이상이어야 합니다.");
            }

            if (_moveLimit <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 이동 횟수는 1 이상이어야 합니다.");
            }

            if (_availableColors == null || _availableColors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 사용 가능한 색상이 없습니다.");
            }

            if (_useColorObjective && _targetAmount <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 색상 목표 수는 1 이상이어야 합니다.");
            }

            ValidateLayout();
            ValidateObstaclePlacements();
            ValidateObjectiveCounts();
        }

        private void ValidateLayout()
        {
            if (_layoutRows == null || _layoutRows.Length != _height)
            {
                throw new InvalidOperationException(
                    $"{name}: Layout Rows 수가 Height와 일치하지 않습니다.");
            }

            for (int row = 0; row < _layoutRows.Length; row++)
            {
                string rowValue = _layoutRows[row];

                if (rowValue == null || rowValue.Length != _width)
                {
                    throw new InvalidOperationException(
                        $"{name}: Layout Row {row}의 길이가 Width와 일치하지 않습니다.");
                }

                for (int x = 0; x < rowValue.Length; x++)
                {
                    char value = rowValue[x];

                    if (value != 'O' && value != 'o' &&
                        value != 'X' && value != 'x')
                    {
                        throw new InvalidOperationException(
                            $"{name}: Layout에는 O와 X만 사용할 수 있습니다. " +
                            $"Row: {row}, X: {x}, Value: {value}");
                    }
                }
            }
        }

        private void ValidateObstaclePlacements()
        {
            if (_obstaclePlacements == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Obstacle Placements가 null입니다.");
            }

            var keys = new HashSet<ObstacleKey>();

            for (int i = 0; i < _obstaclePlacements.Length; i++)
            {
                ObstaclePlacementData placement = _obstaclePlacements[i];

                if (placement == null)
                {
                    throw new InvalidOperationException(
                        $"{name}: Obstacle Placement {i}가 null입니다.");
                }

                placement.ValidateOrThrow(_width, _height);

                if (!IsLayoutPositionPlayable(placement.Position))
                {
                    throw new InvalidOperationException(
                        $"{name}: 장애물 위치 {placement.Position}가 Void Cell입니다.");
                }

                var key = new ObstacleKey(
                    placement.Position,
                    placement.Definition.Layer);

                if (!keys.Add(key))
                {
                    throw new InvalidOperationException(
                        $"{name}: {key.Position}의 {key.Layer} 레이어에 " +
                        "장애물이 중복 배치됐습니다.");
                }
            }
        }

        private void ValidateObjectiveCounts()
        {
            int boxCount = CountObstacleType(CellObstacleType.Box);
            int iceCount = CountObstacleType(CellObstacleType.Ice);

            if (_boxTargetAmount > boxCount)
            {
                throw new InvalidOperationException(
                    $"{name}: 상자 목표 수({_boxTargetAmount})가 " +
                    $"배치 수({boxCount})보다 큽니다.");
            }

            if (_iceTargetAmount > iceCount)
            {
                throw new InvalidOperationException(
                    $"{name}: 얼음 목표 수({_iceTargetAmount})가 " +
                    $"배치 수({iceCount})보다 큽니다.");
            }

            if (!_useColorObjective &&
                _boxTargetAmount <= 0 &&
                _iceTargetAmount <= 0)
            {
                throw new InvalidOperationException(
                    $"{name}: 스테이지 목표가 하나도 없습니다.");
            }
        }

        private int CountObstacleType(CellObstacleType type)
        {
            int count = 0;

            for (int i = 0; i < _obstaclePlacements.Length; i++)
            {
                ObstaclePlacementData placement = _obstaclePlacements[i];

                if (placement?.Definition != null &&
                    placement.Definition.Type == type)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsLayoutPositionPlayable(GridPosition position)
        {
            int row = _height - 1 - position.Y;
            char value = _layoutRows[row][position.X];
            return value != 'X' && value != 'x';
        }
    }
}