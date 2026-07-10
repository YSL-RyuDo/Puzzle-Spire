using System;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Presentation.Puzzle
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BlockView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        public GridPosition Position { get; private set; }

        public Block Block { get; private set; }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(
            GridPosition position,
            Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Position = position;
            Block = block;

            transform.name =
                $"Block_{position.X}_{position.Y}_{block.Color}";

            ApplyColor(block.Color);
        }

        public void SetGridPosition(GridPosition position)
        {
            Position = position;
        }

        private void ApplyColor(BlockColor blockColor)
        {
            _spriteRenderer.color =
                ConvertToUnityColor(blockColor);
        }

        private static Color ConvertToUnityColor(
            BlockColor blockColor)
        {
            switch (blockColor)
            {
                case BlockColor.Red:
                    return new Color32(235, 83, 83, 255);

                case BlockColor.Blue:
                    return new Color32(75, 145, 235, 255);

                case BlockColor.Green:
                    return new Color32(87, 193, 115, 255);

                case BlockColor.Yellow:
                    return new Color32(245, 204, 78, 255);

                case BlockColor.Purple:
                    return new Color32(168, 102, 220, 255);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(blockColor),
                        blockColor,
                        "지원하지 않는 블록 색상입니다.");
            }
        }
    }
}