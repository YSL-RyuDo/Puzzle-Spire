using System;
using System.Text;
using PuzzleSpire.Domain.Puzzle;
using TMPro;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    public sealed class PuzzleHudView : MonoBehaviour
    {
        [Header("HUD")]

        [SerializeField]
        private TMP_Text _remainingMovesText;

        [SerializeField]
        private TMP_Text _objectiveText;

        [Header("Result")]

        [SerializeField]
        private GameObject _resultPanel;

        [SerializeField]
        private TMP_Text _resultTitleText;

        private PuzzleSession _puzzleSession;
        private ObjectiveSystem _objectiveSystem;
        private PuzzleStateMachine _stateMachine;

        public void Bind(
            PuzzleSession puzzleSession,
            ObjectiveSystem objectiveSystem,
            PuzzleStateMachine stateMachine)
        {
            Unbind();

            _puzzleSession =
                puzzleSession
                ?? throw new ArgumentNullException(
                    nameof(puzzleSession));

            _objectiveSystem =
                objectiveSystem
                ?? throw new ArgumentNullException(
                    nameof(objectiveSystem));

            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            ValidateReferences();

            _puzzleSession.RemainingMovesChanged +=
                HandleRemainingMovesChanged;

            _objectiveSystem.ObjectivesChanged +=
                HandleObjectivesChanged;

            _stateMachine.StateChanged +=
                HandleStateChanged;

            RefreshAll();
        }

        private void HandleRemainingMovesChanged(
            int remainingMoves)
        {
            RefreshMoves();
        }

        private void HandleObjectivesChanged()
        {
            RefreshObjectives();
        }

        private void HandleStateChanged(
            PuzzleState state)
        {
            RefreshResult();
        }

        private void RefreshAll()
        {
            RefreshMoves();
            RefreshObjectives();
            RefreshResult();
        }

        private void RefreshMoves()
        {
            _remainingMovesText.text =
                $"MOVES  {_puzzleSession.RemainingMoves}";
        }

        private void RefreshObjectives()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0;
                 i < _objectiveSystem.Objectives.Count;
                 i++)
            {
                IStageObjective objective =
                    _objectiveSystem.Objectives[i];

                if (objective is RemoveColorObjective colorObjective)
                {
                    builder.Append(
                        GetColorName(colorObjective.TargetColor));

                    builder.Append(" BLOCKS  ");

                    builder.Append(
                        colorObjective.CurrentAmount);

                    builder.Append(" / ");

                    builder.Append(
                        colorObjective.TargetAmount);
                }
                else
                {
                    builder.Append(
                        objective.ObjectiveId.ToUpperInvariant());

                    builder.Append("  ");

                    builder.Append(
                        objective.CurrentAmount);

                    builder.Append(" / ");

                    builder.Append(
                        objective.TargetAmount);
                }

                if (objective.IsCompleted)
                {
                    builder.Append("  COMPLETE");
                }

                if (i <
                    _objectiveSystem.Objectives.Count - 1)
                {
                    builder.AppendLine();
                }
            }

            _objectiveText.text = builder.ToString();
        }

        private void RefreshResult()
        {
            bool isCleared =
                _stateMachine.CurrentState
                == PuzzleState.Cleared;

            bool isFailed =
                _stateMachine.CurrentState
                == PuzzleState.Failed;

            bool showResult =
                isCleared || isFailed;

            _resultPanel.SetActive(showResult);

            if (!showResult)
            {
                _resultTitleText.text =
                    string.Empty;

                return;
            }

            _resultTitleText.text =
                isCleared
                    ? "STAGE CLEAR"
                    : "STAGE FAILED";
        }

        private static string GetColorName(BlockColor color)
        {
            switch (color)
            {
                case BlockColor.Red:
                    return "RED";

                case BlockColor.Blue:
                    return "BLUE";

                case BlockColor.Green:
                    return "GREEN";

                case BlockColor.Yellow:
                    return "YELLOW";

                case BlockColor.Purple:
                    return "PURPLE";

                default:
                    return color.ToString().ToUpperInvariant();
            }
        }

        private void ValidateReferences()
        {
            if (_remainingMovesText == null)
            {
                throw new InvalidOperationException(
                    "남은 이동 TMP Text가 연결되지 않았습니다.");
            }

            if (_objectiveText == null)
            {
                throw new InvalidOperationException(
                    "목표 TMP Text가 연결되지 않았습니다.");
            }

            if (_resultPanel == null)
            {
                throw new InvalidOperationException(
                    "결과 패널이 연결되지 않았습니다.");
            }

            if (_resultTitleText == null)
            {
                throw new InvalidOperationException(
                    "결과 제목 TMP Text가 연결되지 않았습니다.");
            }
        }

        private void Unbind()
        {
            if (_puzzleSession != null)
            {
                _puzzleSession.RemainingMovesChanged -=
                    HandleRemainingMovesChanged;
            }

            if (_objectiveSystem != null)
            {
                _objectiveSystem.ObjectivesChanged -=
                    HandleObjectivesChanged;
            }

            if (_stateMachine != null)
            {
                _stateMachine.StateChanged -=
                    HandleStateChanged;
            }

            _puzzleSession = null;
            _objectiveSystem = null;
            _stateMachine = null;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}