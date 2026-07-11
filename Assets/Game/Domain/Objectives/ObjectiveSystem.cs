using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class ObjectiveSystem
    {
        private readonly IStageObjective[] _objectives;

        public IReadOnlyList<IStageObjective>
            Objectives => _objectives;

        public bool AllCompleted
        {
            get
            {
                for (int i = 0;
                     i < _objectives.Length;
                     i++)
                {
                    if (!_objectives[i].IsCompleted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public event Action ObjectivesChanged;

        public ObjectiveSystem(
            IReadOnlyList<IStageObjective> objectives)
        {
            if (objectives == null)
            {
                throw new ArgumentNullException(
                    nameof(objectives));
            }

            if (objectives.Count == 0)
            {
                throw new ArgumentException(
                    "스테이지 목표가 하나 이상 필요합니다.",
                    nameof(objectives));
            }

            _objectives =
                new IStageObjective[objectives.Count];

            HashSet<string> objectiveIds =
                new HashSet<string>();

            for (int i = 0;
                 i < objectives.Count;
                 i++)
            {
                IStageObjective objective =
                    objectives[i]
                    ?? throw new ArgumentException(
                        "null인 목표가 포함되어 있습니다.",
                        nameof(objectives));

                if (!objectiveIds.Add(
                        objective.ObjectiveId))
                {
                    throw new ArgumentException(
                        $"중복된 목표 ID입니다: " +
                        $"{objective.ObjectiveId}",
                        nameof(objectives));
                }

                _objectives[i] = objective;
            }
        }

        public bool Apply(
            PuzzleTurnResult turnResult)
        {
            if (turnResult == null)
            {
                throw new ArgumentNullException(
                    nameof(turnResult));
            }

            bool changed = false;

            for (int i = 0;
                 i < _objectives.Length;
                 i++)
            {
                if (_objectives[i].Apply(turnResult))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                ObjectivesChanged?.Invoke();
            }

            return changed;
        }
    }
}