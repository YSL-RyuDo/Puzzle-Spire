using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    /// <summary>Scene-authored pieces for the visual investigation presentation. No runtime UI creation.</summary>
    public sealed class HotelInvestigationView : MonoBehaviour
    {
        public Sprite[] pieces, poses;
        public Sprite cabinet, effectDisk;
        public Image[] goalIcons, goalChecks, effects;
        public Text[] goalAmounts, goalNames;
        public RectTransform effectsRoot;
        public Image puzzleScene, puzzlePortrait, notebookImage;
        public Text contextTitle, contextBody;
        public Button helpButton;
        public GameObject helpPanel;
        public GameObject inspectionPanel, discoveryCard;
        public Image inspectionImage, discoveryImage;
        public Text inspectionTitle, inspectionBody, discoveryTitle;
        public Button inspectionContinue, inspectionBack;
        public Button[] props;
        public Text propCaption;
        public GameObject investigatedMark;

        public Sprite Piece(int index) => pieces != null && index >= 0 && index < pieces.Length ? pieces[index] : null;
        public Sprite Pose(string speaker, string pose)
        {
            int index = speaker == "you" ? (pose == "curious" ? 0 : pose == "found" ? 1 : -1)
                : speaker == "front" ? (pose == "worried" ? 2 : pose == "remember" ? 3 : -1)
                : speaker == "keeper" ? (pose == "think" ? 4 : pose == "explain" ? 5 : -1)
                : speaker == "guest" ? (pose == "remember" ? 6 : pose == "warm" ? 7 : -1) : -1;
            return poses != null && index >= 0 && index < poses.Length ? poses[index] : null;
        }
        public Sprite Evidence(string id) => id == "thread" ? Piece(13) : id == "key" ? Piece(14) : id == "towel" ? Piece(15) : null;
        public void ClearEffects()
        {
            foreach (var effect in effects) effect.gameObject.SetActive(false);
        }
    }
}
