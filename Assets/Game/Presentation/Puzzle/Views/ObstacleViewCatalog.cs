using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    [CreateAssetMenu(
        fileName = "ObstacleViewCatalog",
        menuName = "Puzzle Spire/Presentation/Obstacle View Catalog")]
    public sealed class ObstacleViewCatalog : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private CellObstacleType _type;
            [SerializeField] private DamageableObstacleView _prefab;

            public CellObstacleType Type => _type;
            public DamageableObstacleView Prefab => _prefab;
        }

        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        private Dictionary<CellObstacleType, DamageableObstacleView> _prefabs;

        public DamageableObstacleView GetPrefab(CellObstacleType type)
        {
            EnsureCache();

            if (!_prefabs.TryGetValue(type, out DamageableObstacleView prefab))
            {
                throw new InvalidOperationException(
                    $"Obstacle View Prefab이 등록되지 않았습니다: {type}");
            }

            return prefab;
        }

        public void ValidateOrThrow()
        {
            EnsureCache();
        }

        private void EnsureCache()
        {
            if (_prefabs != null)
            {
                return;
            }

            _prefabs = new Dictionary<CellObstacleType, DamageableObstacleView>();

            for (int i = 0; i < _entries.Length; i++)
            {
                Entry entry = _entries[i];

                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"{name}: Entry {i}가 null입니다.");
                }

                if (entry.Prefab == null)
                {
                    throw new InvalidOperationException(
                        $"{name}: {entry.Type} View Prefab이 비어 있습니다.");
                }

                if (_prefabs.ContainsKey(entry.Type))
                {
                    throw new InvalidOperationException(
                        $"{name}: {entry.Type} View가 중복 등록됐습니다.");
                }

                _prefabs.Add(entry.Type, entry.Prefab);
            }
        }

        private void OnValidate()
        {
            _prefabs = null;
        }
    }
}