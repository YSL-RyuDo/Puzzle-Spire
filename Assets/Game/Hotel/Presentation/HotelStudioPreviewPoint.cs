using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace PuzzleSpire.Hotel
{
    public sealed class HotelStudioPreviewPoint : MonoBehaviour, IPointerClickHandler
    {
        public Action<Vector2> clicked;
        public void OnPointerClick(PointerEventData e)
        {
            var rect = (RectTransform)transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var p))
                clicked?.Invoke(new Vector2(Mathf.Clamp01((p.x - rect.rect.xMin) / rect.rect.width), Mathf.Clamp01(1 - (p.y - rect.rect.yMin) / rect.rect.height)));
        }
    }
}
