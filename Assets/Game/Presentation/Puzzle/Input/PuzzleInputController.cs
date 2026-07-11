using PuzzleSpire.Domain.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleSpire.Presentation.Puzzle
{
    public sealed class PuzzleInputController : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Camera _worldCamera;

        [SerializeField]
        private BoardView _boardView;

        [SerializeField]
        private PuzzleController _puzzleController;

        private void Update()
        {
            if (_puzzleController == null
                || !_puzzleController.CanAcceptInput)
            {
                return;
            }

            if (!TryReadPointerDown(out Vector2 screenPosition))
            {
                return;
            }

            if (_worldCamera == null || _boardView == null)
            {
                Debug.LogError(
                    "PuzzleInputController의 참조가 연결되지 않았습니다.");

                return;
            }

            float distanceFromCamera =
                Mathf.Abs(
                    _boardView.transform.position.z
                    - _worldCamera.transform.position.z);

            Vector3 worldPosition =
                _worldCamera.ScreenToWorldPoint(
                    new Vector3(
                        screenPosition.x,
                        screenPosition.y,
                        distanceFromCamera));

            if (!_boardView.TryWorldToGridPosition(
                    worldPosition,
                    out GridPosition gridPosition))
            {
                return;
            }

            _puzzleController.SelectBlock(gridPosition);
        }

        private static bool TryReadPointerDown(
            out Vector2 screenPosition)
        {
            if (Touchscreen.current != null
                && Touchscreen.current.primaryTouch.press
                    .wasPressedThisFrame)
            {
                screenPosition =
                    Touchscreen.current.primaryTouch.position
                        .ReadValue();

                return true;
            }

            if (Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition =
                    Mouse.current.position.ReadValue();

                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}