using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed class HotelWorkshopView : MonoBehaviour
    {
        public GameObject screen;
        public Text stageLabel, modeLabel, status, goals, brushHelp, certificate;
        public Button previous, next, save, reload, test, verify, stop, export, folder, back, undo, redo;
        public Button[] brushes;
        public InputField[] fields;
        public GridLayoutGroup grid;
        public Button[] cells;
        public Image[] pieces, obstacles;
        public Text[] cellLabels;
    }
}
