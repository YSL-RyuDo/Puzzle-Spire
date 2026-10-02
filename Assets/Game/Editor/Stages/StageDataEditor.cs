#if UNITY_EDITOR
using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEditor;
using UnityEngine;

namespace PuzzleSpire.Bootstrap.Editor
{
    [CustomEditor(typeof(StageData))]
    public sealed class StageDataEditor : UnityEditor.Editor
    {
        private enum PaintTool
        {
            Playable,
            Void,
            Obstacle,
            EraseLayer,
            EraseAll
        }

        private const float CellSize = 36f;
        private static readonly Color PlayableColor = new Color(0.25f, 0.55f, 0.9f);
        private static readonly Color VoidColor = new Color(0.22f, 0.22f, 0.22f);
        private static readonly Color BoxColor = new Color(0.75f, 0.43f, 0.16f);
        private static readonly Color IceColor = new Color(0.35f, 0.8f, 1f);
        private static readonly Color OverlayColor = new Color(0.55f, 0.8f, 0.35f);
        private static readonly Color MultipleColor = new Color(0.65f, 0.45f, 0.85f);

        private SerializedProperty _stageId;
        private SerializedProperty _width;
        private SerializedProperty _height;
        private SerializedProperty _randomSeed;
        private SerializedProperty _availableColors;
        private SerializedProperty _moveLimit;
        private SerializedProperty _targetColor;
        private SerializedProperty _targetAmount;
        private SerializedProperty _useColorObjective;
        private SerializedProperty _boxTargetAmount;
        private SerializedProperty _iceTargetAmount;
        private SerializedProperty _layoutRows;
        private SerializedProperty _obstaclePlacements;

        private PaintTool _paintTool = PaintTool.Obstacle;
        private CellObstacleDefinition _selectedDefinition;
        private bool _showRawData;

        private void OnEnable()
        {
            _stageId = serializedObject.FindProperty("_stageId");
            _width = serializedObject.FindProperty("_width");
            _height = serializedObject.FindProperty("_height");
            _randomSeed = serializedObject.FindProperty("_randomSeed");
            _availableColors = serializedObject.FindProperty("_availableColors");
            _moveLimit = serializedObject.FindProperty("_moveLimit");
            _targetColor = serializedObject.FindProperty("_targetColor");
            _targetAmount = serializedObject.FindProperty("_targetAmount");
            _useColorObjective = serializedObject.FindProperty("_useColorObjective");
            _boxTargetAmount = serializedObject.FindProperty("_boxTargetAmount");
            _iceTargetAmount = serializedObject.FindProperty("_iceTargetAmount");
            _layoutRows = serializedObject.FindProperty("_layoutRows");
            _obstaclePlacements = serializedObject.FindProperty("_obstaclePlacements");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawIdentitySection();
            DrawBoardSection();
            DrawColorSection();
            DrawRuleSection();

            EnsureLayoutSize();
            RemoveOutOfBoundsPlacements();
            DrawVisualLayoutSection();
            DrawRawDataSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIdentitySection()
        {
            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_stageId);
            EditorGUILayout.Space(6f);
        }

        private void DrawBoardSection()
        {
            EditorGUILayout.LabelField("Board", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_width);
            EditorGUILayout.PropertyField(_height);

            if (EditorGUI.EndChangeCheck())
            {
                _width.intValue = Mathf.Max(1, _width.intValue);
                _height.intValue = Mathf.Max(1, _height.intValue);
                EnsureLayoutSize();
                RemoveOutOfBoundsPlacements();
            }

            EditorGUILayout.PropertyField(_randomSeed);
            EditorGUILayout.Space(6f);
        }

        private void DrawColorSection()
        {
            EditorGUILayout.LabelField("Available Colors", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_availableColors, true);
            EditorGUILayout.Space(6f);
        }

        private void DrawRuleSection()
        {
            EditorGUILayout.LabelField("Stage Rules", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_moveLimit);
            EditorGUILayout.PropertyField(_useColorObjective);

            using (new EditorGUI.DisabledScope(!_useColorObjective.boolValue))
            {
                EditorGUILayout.PropertyField(_targetColor);
                EditorGUILayout.PropertyField(_targetAmount);
            }

            DrawObstacleTarget(
                _boxTargetAmount,
                CellObstacleType.Box,
                "상자");

            DrawObstacleTarget(
                _iceTargetAmount,
                CellObstacleType.Ice,
                "얼음");

            EditorGUILayout.Space(8f);
        }

        private void DrawObstacleTarget(
            SerializedProperty targetProperty,
            CellObstacleType type,
            string label)
        {
            int count = CountObstacleType(type);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(targetProperty);

            if (GUILayout.Button(
                    $"현재 {label} 수({count})로 맞추기",
                    GUILayout.Width(170f)))
            {
                targetProperty.intValue = count;
            }

            EditorGUILayout.EndHorizontal();

            if (targetProperty.intValue > count)
            {
                EditorGUILayout.HelpBox(
                    $"{label} 목표 수({targetProperty.intValue})가 " +
                    $"현재 배치 수({count})보다 큽니다.",
                    MessageType.Warning);
            }
        }

        private void DrawVisualLayoutSection()
        {
            EditorGUILayout.LabelField("Visual Stage Editor", EditorStyles.boldLabel);

            _paintTool = (PaintTool)GUILayout.Toolbar(
                (int)_paintTool,
                new[] { "Playable", "Void", "Obstacle", "Erase Layer", "Erase All" });

            if (_paintTool == PaintTool.Obstacle ||
                _paintTool == PaintTool.EraseLayer)
            {
                _selectedDefinition =
                    (CellObstacleDefinition)EditorGUILayout.ObjectField(
                        "Obstacle Definition",
                        _selectedDefinition,
                        typeof(CellObstacleDefinition),
                        false);
            }

            EditorGUILayout.LabelField(
                "Obstacle: 선택 Definition의 레이어에 배치 / Erase Layer: 선택 레이어만 제거",
                EditorStyles.miniLabel);

            DrawGrid();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("모든 셀 Playable"))
            {
                SetAllPlayable();
            }

            if (GUILayout.Button("모든 장애물 제거"))
            {
                _obstaclePlacements.arraySize = 0;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8f);
        }

        private void DrawGrid()
        {
            int width = _width.intValue;
            int height = _height.intValue;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(y.ToString(), GUILayout.Width(22f));

                for (int x = 0; x < width; x++)
                {
                    DrawCellButton(row, x, y);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24f);

            for (int x = 0; x < width; x++)
            {
                GUILayout.Label(
                    x.ToString(),
                    EditorStyles.centeredGreyMiniLabel,
                    GUILayout.Width(CellSize));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCellButton(int row, int x, int y)
        {
            char layoutValue = GetLayoutValue(row, x);
            bool isVoid = layoutValue == 'X' || layoutValue == 'x';

            GetCellObstacleDisplay(
                x,
                y,
                out string obstacleLabel,
                out Color obstacleColor);

            string label = isVoid
                ? "X"
                : string.IsNullOrEmpty(obstacleLabel)
                    ? "O"
                    : obstacleLabel;

            Color color = isVoid
                ? VoidColor
                : string.IsNullOrEmpty(obstacleLabel)
                    ? PlayableColor
                    : obstacleColor;

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            if (GUILayout.Button(
                    new GUIContent(label, $"({x}, {y})"),
                    GUILayout.Width(CellSize),
                    GUILayout.Height(CellSize)))
            {
                PaintCell(row, x, y);
                GUI.FocusControl(null);
            }

            GUI.backgroundColor = previousColor;
        }

        private void GetCellObstacleDisplay(
            int x,
            int y,
            out string label,
            out Color color)
        {
            bool hasFloor = HasPlacement(x, y, ObstacleLayer.Floor);
            bool hasBlocking = HasPlacement(x, y, ObstacleLayer.Blocking);
            bool hasOverlay = HasPlacement(x, y, ObstacleLayer.Overlay);
            int count = (hasFloor ? 1 : 0) +
                        (hasBlocking ? 1 : 0) +
                        (hasOverlay ? 1 : 0);

            if (count > 1)
            {
                label = $"{(hasFloor ? "I" : "")}{(hasBlocking ? "B" : "")}{(hasOverlay ? "O" : "")}";
                color = MultipleColor;
                return;
            }

            if (hasFloor)
            {
                label = "I";
                color = IceColor;
                return;
            }

            if (hasBlocking)
            {
                label = "B";
                color = BoxColor;
                return;
            }

            if (hasOverlay)
            {
                label = "O";
                color = OverlayColor;
                return;
            }

            label = string.Empty;
            color = PlayableColor;
        }

        private void PaintCell(int row, int x, int y)
        {
            switch (_paintTool)
            {
                case PaintTool.Playable:
                    SetLayoutValue(row, x, 'O');
                    break;

                case PaintTool.Void:
                    SetLayoutValue(row, x, 'X');
                    RemoveAllPlacements(x, y);
                    break;

                case PaintTool.Obstacle:
                    if (!TryRequireSelectedDefinition())
                    {
                        return;
                    }

                    SetLayoutValue(row, x, 'O');
                    AddOrUpdatePlacement(x, y, _selectedDefinition);
                    break;

                case PaintTool.EraseLayer:
                    if (!TryRequireSelectedDefinition())
                    {
                        return;
                    }

                    RemovePlacement(x, y, _selectedDefinition.Layer);
                    break;

                case PaintTool.EraseAll:
                    RemoveAllPlacements(x, y);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TryRequireSelectedDefinition()
        {
            if (_selectedDefinition != null)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Obstacle Definition 필요",
                "Obstacle Definition을 먼저 선택해야 합니다.",
                "확인");

            return false;
        }

        private void EnsureLayoutSize()
        {
            int width = Mathf.Max(1, _width.intValue);
            int height = Mathf.Max(1, _height.intValue);

            _layoutRows.arraySize = height;

            for (int row = 0; row < height; row++)
            {
                SerializedProperty rowProperty =
                    _layoutRows.GetArrayElementAtIndex(row);

                string value = rowProperty.stringValue ?? string.Empty;

                if (value.Length > width)
                {
                    value = value.Substring(0, width);
                }
                else if (value.Length < width)
                {
                    value = value.PadRight(width, 'O');
                }

                rowProperty.stringValue = value;
            }
        }

        private void RemoveOutOfBoundsPlacements()
        {
            int width = _width.intValue;
            int height = _height.intValue;

            for (int i = _obstaclePlacements.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty placement =
                    _obstaclePlacements.GetArrayElementAtIndex(i);

                int x = placement.FindPropertyRelative("_x").intValue;
                int y = placement.FindPropertyRelative("_y").intValue;

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    _obstaclePlacements.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private char GetLayoutValue(int row, int x)
        {
            string value =
                _layoutRows.GetArrayElementAtIndex(row).stringValue;

            return string.IsNullOrEmpty(value) || x >= value.Length
                ? 'O'
                : value[x];
        }

        private void SetLayoutValue(int row, int x, char value)
        {
            SerializedProperty rowProperty =
                _layoutRows.GetArrayElementAtIndex(row);

            char[] characters = rowProperty.stringValue.ToCharArray();
            characters[x] = value;
            rowProperty.stringValue = new string(characters);
        }

        private bool HasPlacement(
            int x,
            int y,
            ObstacleLayer layer)
        {
            return TryFindPlacement(x, y, layer, out _);
        }

        private bool TryFindPlacement(
            int x,
            int y,
            ObstacleLayer layer,
            out int index)
        {
            for (int i = 0; i < _obstaclePlacements.arraySize; i++)
            {
                SerializedProperty placement =
                    _obstaclePlacements.GetArrayElementAtIndex(i);

                int placementX =
                    placement.FindPropertyRelative("_x").intValue;

                int placementY =
                    placement.FindPropertyRelative("_y").intValue;

                var definition =
                    placement.FindPropertyRelative("_definition")
                        .objectReferenceValue as CellObstacleDefinition;

                if (placementX == x &&
                    placementY == y &&
                    definition != null &&
                    definition.Layer == layer)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private void AddOrUpdatePlacement(
            int x,
            int y,
            CellObstacleDefinition definition)
        {
            if (!TryFindPlacement(
                    x,
                    y,
                    definition.Layer,
                    out int index))
            {
                index = _obstaclePlacements.arraySize;
                _obstaclePlacements.InsertArrayElementAtIndex(index);
            }

            SerializedProperty placement =
                _obstaclePlacements.GetArrayElementAtIndex(index);

            placement.FindPropertyRelative("_x").intValue = x;
            placement.FindPropertyRelative("_y").intValue = y;
            placement.FindPropertyRelative("_definition")
                .objectReferenceValue = definition;
        }

        private void RemovePlacement(
            int x,
            int y,
            ObstacleLayer layer)
        {
            if (TryFindPlacement(x, y, layer, out int index))
            {
                _obstaclePlacements.DeleteArrayElementAtIndex(index);
            }
        }

        private void RemoveAllPlacements(int x, int y)
        {
            for (int i = _obstaclePlacements.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty placement =
                    _obstaclePlacements.GetArrayElementAtIndex(i);

                if (placement.FindPropertyRelative("_x").intValue == x &&
                    placement.FindPropertyRelative("_y").intValue == y)
                {
                    _obstaclePlacements.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private int CountObstacleType(CellObstacleType type)
        {
            int count = 0;

            for (int i = 0; i < _obstaclePlacements.arraySize; i++)
            {
                var definition =
                    _obstaclePlacements.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("_definition")
                        .objectReferenceValue as CellObstacleDefinition;

                if (definition != null && definition.Type == type)
                {
                    count++;
                }
            }

            return count;
        }

        private void SetAllPlayable()
        {
            for (int row = 0; row < _layoutRows.arraySize; row++)
            {
                _layoutRows.GetArrayElementAtIndex(row).stringValue =
                    new string('O', _width.intValue);
            }
        }

        private void DrawRawDataSection()
        {
            _showRawData = EditorGUILayout.Foldout(
                _showRawData,
                "Advanced / Raw Data",
                true);

            if (!_showRawData)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_layoutRows, true);
            EditorGUILayout.PropertyField(_obstaclePlacements, true);
            EditorGUI.indentLevel--;
        }
    }
}
#endif