using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel.Editor
{
    public static partial class HotelSceneBuilder
    {
        [MenuItem("Puzzle Spire/퍼즐 제작실 추가 · 기존 UI 보존")]
        public static void AddPuzzleWorkshop()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = Object.FindFirstObjectByType<HotelView>();
            if (v.workshop != null) { Debug.Log("HOTEL_WORKSHOP_ALREADY_AUTHORED"); return; }
            root = v.titleScreen.transform.parent;
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/RoundedPanel.png");
            var panel = Box(root, "11 Puzzle Workshop · EDIT THIS PREFAB", 0, 0, 1600, 900, Ink).gameObject;
            // Keep confirmation and dialogue overlays above normal screens.
            panel.transform.SetSiblingIndex(v.endingScreen.transform.GetSiblingIndex() + 1);
            var w = panel.AddComponent<HotelWorkshopView>(); w.screen = panel;
            var p = panel.transform;
            Box(p, "Gold rule", 40, 35, 1520, 2, Gold);
            Text(p, "Heading", "퍼즐 제작실", 40, 53, 650, 50, 36, Paper, FontStyle.Bold);
            Text(p, "Subtitle", "만들고, 직접 풀고, MOD로 나누세요.", 40, 108, 900, 32, 22, Muted);
            w.certificate = Text(p, "Clearance status", "클리어 검증 필요", 1130, 73, 425, 45, 25, Gold, FontStyle.Bold, TextAnchor.MiddleRight);
            w.previous = Button(p, "Previous target", "←", 40, 159, 55, 44);
            w.stageLabel = Text(p, "Target story puzzle", "기본 이야기의 퍼즐 교체", 111, 163, 850, 38, 23, Gold);
            w.next = Button(p, "Next target", "→", 990, 159, 55, 44);
            w.back = Button(p, "Save and title", "저장 후 타이틀", 1330, 147, 225, 52, Teal, 20);

            Box(p, "Tools panel", 40, 221, 290, 504, Panel, true);
            Text(p, "Brush label", "배치 도구", 59, 240, 250, 34, 24, Gold, FontStyle.Bold);
            string[] brushes = { "일반 칸", "빈 공간", "상자", "얼음", "장애물 지우기" };
            w.brushes = new Button[brushes.Length];
            for (int i = 0; i < brushes.Length; i++) w.brushes[i] = Button(p, "Brush " + i, brushes[i], 59, 283 + i * 48, 252, 40, Teal, 21);
            w.brushHelp = Text(p, "Brush help", "도구 안내", 59, 534, 250, 180, 17, Muted);

            w.modeLabel = Text(p, "Mode", "퍼즐 편집", 367, 214, 755, 35, 22, Gold, FontStyle.Bold);
            var board = Box(p, "Authored board · 81 cells", 410, 257, 560, 560, new Color(.07f, .13f, .16f));
            w.grid = board.gameObject.AddComponent<GridLayoutGroup>(); w.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            w.grid.constraintCount = 7; w.grid.spacing = new Vector2(6, 6); w.grid.childAlignment = TextAnchor.MiddleCenter;
            w.cells = new Button[81]; w.pieces = new Image[81]; w.obstacles = new Image[81]; w.cellLabels = new Text[81];
            for (int i = 0; i < 81; i++)
            {
                var cell = Button(board.transform, "Workshop cell " + i, "", 0, 0, 70, 70, Teal, 22);
                w.cells[i] = cell;
                w.pieces[i] = Box(cell.transform, "Block image", 3, 3, 64, 64, Color.white); Stretch(w.pieces[i].rectTransform, 3); w.pieces[i].preserveAspect = true;
                w.obstacles[i] = Box(cell.transform, "Obstacle image", 1, 1, 68, 68, Color.white); Stretch(w.obstacles[i].rectTransform, 1); w.obstacles[i].preserveAspect = true;
                w.cellLabels[i] = cell.GetComponentInChildren<Text>(); w.cellLabels[i].transform.SetAsLastSibling(); w.cellLabels[i].fontStyle = FontStyle.Bold;
            }

            Box(p, "Settings panel", 1110, 221, 445, 504, Panel, true);
            Text(p, "Settings title", "퍼즐 설정", 1130, 239, 400, 36, 24, Gold, FontStyle.Bold);
            w.fields = new InputField[10];
            string[] labels = { "퍼즐 이름", "가로 3~9", "세로 3~9", "이동 횟수 1~100", "색 종류 2~5", "목표 색 0~4", "색 목표 개수", "상자 목표 개수", "얼음 목표 개수", "랜덤 시드" };
            w.fields[0] = WorkshopInput(p, labels[0], 1130, 282, 400, false);
            for (int i = 1; i < 9; i++) w.fields[i] = WorkshopInput(p, labels[i], 1130 + ((i - 1) % 2) * 210, 350 + ((i - 1) / 2) * 68, 190, true);
            w.fields[9] = WorkshopInput(p, labels[9], 1130, 622, 400, true);

            w.undo = Button(p, "Undo edit", "되돌리기", 40, 738, 138, 44, Teal, 18);
            w.redo = Button(p, "Redo edit", "다시 실행", 190, 738, 140, 44, Teal, 18);
            w.save = Button(p, "Save draft", "초안 저장", 1110, 739, 140, 44, Teal, 18);
            w.reload = Button(p, "Reload draft", "초안 불러오기", 1262, 739, 150, 44, Teal, 17);
            w.folder = Button(p, "Open exports", "내보낸 폴더", 1424, 739, 131, 44, Teal, 17);
            w.test = Button(p, "Test play", "시험 플레이", 40, 794, 290, 46, Teal, 21);
            w.verify = Button(p, "Verify clear", "클리어 검증 시작", 1110, 794, 270, 46, Teal, 21);
            w.stop = Button(p, "Stop play", "편집으로", 1392, 794, 163, 46, Teal, 20);
            w.export = Button(p, "Export verified MOD", "검증된 MOD 내보내기", 1110, 850, 445, 40, Hex("766247"), 21);
            w.goals = Text(p, "Goals and moves", "배치 현황", 370, 819, 710, 26, 20, Gold);
            w.status = Text(p, "Help and result", "", 40, 848, 1050, 48, 17, Muted);

            Directory.CreateDirectory("Assets/Game/Hotel/Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(panel, "Assets/Game/Hotel/Prefabs/PuzzleWorkshop.prefab", InteractionMode.AutomatedAction);
            var controller = v.gameObject.AddComponent<HotelPuzzleWorkshop>();
            var serialized = new SerializedObject(controller); serialized.FindProperty("view").objectReferenceValue = w; serialized.ApplyModifiedPropertiesWithoutUndo();
            v.workshop = controller;
            v.workshopButton = Button(v.titleScreen.transform, "Puzzle workshop", "퍼즐 제작실", 1110, 803, 400, 58, Hex("766247"), 24);
            var statusRect = (RectTransform)v.titleStatus.transform;
            statusRect.sizeDelta = new Vector2(1000, statusRect.sizeDelta.y);
            v.Show(v.titleScreen); EditorUtility.SetDirty(v); EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_WORKSHOP_AUTHORED: saved scene and editable prefab.");
        }
        private static InputField WorkshopInput(Transform parent, string name, float x, float y, float width, bool number)
        {
            Text(parent, name + " label", name, x, y, width, 23, 17, Muted);
            var bg = Box(parent, name + " input", x, y + 25, width, 36, Ink, true); bg.raycastTarget = true;
            var input = bg.gameObject.AddComponent<InputField>(); input.targetGraphic = bg;
            input.textComponent = Text(bg.transform, "Value", "", 10, 3, width - 20, 30, 22, Paper, FontStyle.Normal, TextAnchor.MiddleLeft);
            input.contentType = number ? InputField.ContentType.IntegerNumber : InputField.ContentType.Standard;
            input.characterLimit = number ? 11 : 50;
            return input;
        }
        public static void AddPuzzleWorkshopAndBuild() { AddPuzzleWorkshop(); CreateReleaseBuild(); }
        // Targeted migration for the first authored layout; never recreates the screen.
        public static void PolishPuzzleWorkshopAndBuild()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = Object.FindFirstObjectByType<HotelView>();
            var w = v.workshop.View;
            var goals = (RectTransform)w.goals.transform;
            goals.sizeDelta = new Vector2(710, 26);
            PrefabUtility.RecordPrefabInstancePropertyModifications(goals);
            var titleStatus = (RectTransform)v.titleStatus.transform;
            titleStatus.sizeDelta = new Vector2(1000, titleStatus.sizeDelta.y);
            EditorUtility.SetDirty(titleStatus);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            // Keep the reusable prefab consistent with the authored instance.
            PrefabUtility.ApplyPropertyOverride(new SerializedObject(goals).FindProperty("m_SizeDelta"), "Assets/Game/Hotel/Prefabs/PuzzleWorkshop.prefab", InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            CreateReleaseBuild();
        }
    }
}
