using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Bootstrap
{
    public abstract class CellObstacleDefinition :
        ScriptableObject
    {
        [SerializeField]
        private string _definitionId;

        public string DefinitionId =>
            _definitionId;

        public abstract CellObstacleType Type
        {
            get;
        }

        public abstract ObstacleLayer Layer
        {
            get;
        }

        public abstract ICellObstacle
            CreateRuntimeObstacle();

        public virtual void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(
                    _definitionId))
            {
                throw new InvalidOperationException(
                    $"{name}: Definition ID가 비어 있습니다.");
            }
        }
    }
}