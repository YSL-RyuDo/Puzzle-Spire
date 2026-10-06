using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    /// <summary>All widgets are serialized scene objects. Runtime code never builds a Canvas.</summary>
    public sealed class HotelView : MonoBehaviour
    {
        [Header("Screens · Hierarchy에서 자유롭게 편집")]
        public GameObject titleScreen, exploreScreen, puzzleScreen, riddleScreen, notebookScreen;
        public GameObject catalogScreen, settingsScreen, endingScreen;
        [Header("Title")]
        public Button continueButton, newButton, practiceButton, modsButton, settingsButton, quitButton;
        public Text titleStatus;
        [Header("Investigation")]
        public Text chapter, objective, locationTitle, locationDescription, progressText;
        public Button[] locationButtons;
        public GameObject[] locationArt;
        public Image[] locationImages;
        public Button hotspot, nextLocation, notebookButton, exploreTitleButton;
        public Text hotspotTitle, hotspotDescription;
        [Header("Dialogue overlay")]
        public GameObject dialoguePanel;
        public Text speakerName, speakerRole, dialogueText, dialogueCounter, portraitInitial;
        public Image portraitColor, portraitImage;
        public Button dialogueNext;
        [Header("PC single-stage presentation · authored objects only")]
        public HotelStagePresentation stagePresentation;
        public CanvasGroup explorationHud;
        public HotelInvestigationView investigation;
        [Header("Puzzle")]
        public Text puzzleTitle, puzzlePurpose, movesText, goalsText, puzzleHelp, comboText;
        public GridLayoutGroup boardLayout;
        public HotelCellView[] cells;
        public Button retryButton, puzzleBackButton, puzzleHintButton;
        [Header("Riddle")]
        public Text riddleTitle, question, selectionText, feedbackText;
        public Button[] options;
        public Button submitAnswer, resetAnswer, riddleHint, riddleBack;
        [Header("Notebook")]
        public Button[] clueButtons;
        public Text clueTitle, clueDescription, clueCount;
        public Button notebookBack;
        public HotelCollectionView collection;
        [Header("Catalog · MOD / practice")]
        public Text catalogTitle, catalogPage, catalogDescription;
        public Button[] catalogRows;
        public Button catalogPrevious, catalogNext, catalogBack, catalogFolder, catalogRefresh, catalogSample;
        [Header("Settings")]
        public Button soundButton, fullscreenButton, settingsBack;
        public Text settingsInfo;
        [Header("Result / confirmation overlay")]
        public GameObject resultPanel;
        public Text resultEyebrow, resultTitle, resultBody;
        public Button resultPrimary, resultSecondary;
        [Header("Ending")]
        public Text endingSummary;
        public Button endingNotebook, endingTitle;

        public void Show(GameObject target)
        {
            if (collection != null) collection.detailPanel.SetActive(false);
            if (investigation != null) { investigation.inspectionPanel.SetActive(false); investigation.helpPanel.SetActive(false); }
            if (stagePresentation != null) stagePresentation.CancelDialogue();
            if (explorationHud != null) { explorationHud.alpha = 1; explorationHud.blocksRaycasts = true; }
            foreach (var screen in new[] { titleScreen, exploreScreen, puzzleScreen, riddleScreen, notebookScreen, catalogScreen, settingsScreen, endingScreen }) screen.SetActive(screen == target);
            dialoguePanel.SetActive(false); resultPanel.SetActive(false);
        }
        public static void Label(Button button, string text) => button.GetComponentInChildren<Text>(true).text = text;
    }
}
