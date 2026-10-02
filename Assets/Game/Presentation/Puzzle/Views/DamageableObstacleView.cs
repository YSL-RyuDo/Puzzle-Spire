using System;
using DG.Tweening;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    public abstract class DamageableObstacleView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _damageStageSprites;
        [SerializeField, Min(0.01f)] private float _hitDuration = 0.16f;
        [SerializeField, Min(0.01f)] private float _destroyDuration = 0.18f;

        private int _maxDurability;
        private int _currentDurability;

        public GridPosition Position { get; private set; }
        public ObstacleLayer Layer { get; private set; }
        public CellObstacleType Type { get; private set; }
        public string DefinitionId { get; private set; }
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public ObstacleKey Key => new ObstacleKey(Position, Layer);

        public void Initialize(
            GridPosition position,
            IDamageableObstacle obstacle)
        {
            if (obstacle == null)
            {
                throw new ArgumentNullException(nameof(obstacle));
            }

            ValidateReferences();

            Position = position;
            Layer = obstacle.Layer;
            Type = obstacle.Type;
            DefinitionId = obstacle.DefinitionId;
            _maxDurability = obstacle.MaxDurability;
            _currentDurability = obstacle.CurrentDurability;

            ApplySprite();
        }

        public Tween CreateStateChangeTween(ObstacleStateChange change)
        {
            if (change.Position != Position || change.Layer != Layer)
            {
                throw new InvalidOperationException(
                    $"장애물 View 키가 일치하지 않습니다. View: {Key}");
            }

            if (!string.Equals(
                    change.DefinitionId,
                    DefinitionId,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"장애물 Definition ID가 일치하지 않습니다. " +
                    $"View: {DefinitionId}, Change: {change.DefinitionId}");
            }

            _currentDurability = change.CurrentDurability;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                transform.DOPunchScale(
                    new Vector3(0.14f, 0.14f, 0f),
                    _hitDuration,
                    7,
                    0.55f));

            sequence.AppendCallback(ApplySprite);

            if (change.WasDestroyed)
            {
                sequence.Append(
                    transform.DOScale(
                            Vector3.zero,
                            _destroyDuration)
                        .SetEase(Ease.InBack));
            }

            return sequence;
        }

        private void ApplySprite()
        {
            if (_currentDurability <= 0)
            {
                return;
            }

            int lastIndex = _damageStageSprites.Length - 1;
            int maximumDamage = Math.Max(1, _maxDurability - 1);
            int currentDamage = Math.Max(0, _maxDurability - _currentDurability);

            int spriteIndex = Mathf.Clamp(
                Mathf.RoundToInt(
                    currentDamage * lastIndex /
                    (float)maximumDamage),
                0,
                lastIndex);

            _spriteRenderer.sprite = _damageStageSprites[spriteIndex];
        }

        private void ValidateReferences()
        {
            if (_spriteRenderer == null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name}: SpriteRenderer가 연결되지 않았습니다.");
            }

            if (_damageStageSprites == null ||
                _damageStageSprites.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name}: Damage Stage Sprites가 비어 있습니다.");
            }

            for (int i = 0; i < _damageStageSprites.Length; i++)
            {
                if (_damageStageSprites[i] == null)
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name}: Damage Stage Sprite {i}가 비어 있습니다.");
                }
            }
        }
    }
}