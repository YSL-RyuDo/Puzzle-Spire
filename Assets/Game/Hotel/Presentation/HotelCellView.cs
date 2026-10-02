using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace PuzzleSpire.Hotel
{
    public sealed class HotelCellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button button;
        public Image face, customImage, obstacleBorder;
        public Text symbol, obstacleText;
        public RectTransform blockRoot;
        public Image highlight;
        public Action<bool> Hover;
        public void OnPointerEnter(PointerEventData e) => Hover?.Invoke(true);
        public void OnPointerExit(PointerEventData e) => Hover?.Invoke(false);
        public void ResetPose()
        {
            transform.localScale = Vector3.one;
            blockRoot.anchoredPosition = Vector2.zero;
            blockRoot.localScale = Vector3.one;
            obstacleBorder.rectTransform.localScale = Vector3.one;
            obstacleBorder.rectTransform.localRotation = Quaternion.identity;
            highlight.enabled = false;
        }
    }
}
