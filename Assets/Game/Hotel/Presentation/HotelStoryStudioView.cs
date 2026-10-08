using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed class HotelStoryStudioView : MonoBehaviour
    {
        public GameObject screen, pickerPanel;
        public Text heading, status, context, page, previewCaption, longLabel, pickerTitle, pickerPage;
        public Button[] tabs, rows, fieldPickers, pickerRows;
        public Text[] labels;
        public InputField[] fields;
        public InputField longText;
        public Button previous, next, add, remove, up, down, chain, copy, save, trial, export, back, special, dialogue, selectedTrial, textMode, folder, importImage, clearImage;
        public Button pickerPrevious, pickerNext, pickerClose;
        public Image preview, marker;
        public HotelStudioPreviewPoint previewPoint;
    }
}
