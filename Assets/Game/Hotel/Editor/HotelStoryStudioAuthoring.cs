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
        [MenuItem("Puzzle Spire/이야기 제작실 추가 · 씬과 프리팹 저장")]
        public static void AddStoryStudio()
        {
            EditorSceneManager.OpenScene(ScenePath); v = Object.FindFirstObjectByType<HotelView>();
            if (v.storyStudio != null) { Debug.Log("HOTEL_STUDIO_ALREADY_AUTHORED"); return; }
            root = v.titleScreen.transform.parent;
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/RoundedPanel.png");
            var panel = Box(root, "12 Story Studio · EDIT THIS PREFAB", 0, 0, 1600, 900, Ink).gameObject;
            panel.transform.SetSiblingIndex(v.endingScreen.transform.GetSiblingIndex() + 1);
            var w = panel.AddComponent<HotelStoryStudioView>(); w.screen = panel; var p = panel.transform;
            Box(p, "Gold rule", 40, 31, 1520, 2, Gold);
            w.heading = Text(p, "Project heading", "이야기 제작실", 40, 45, 1500, 44, 32, Paper, FontStyle.Bold);
            string[] tabs = { "프로젝트", "장소", "인물", "단서", "조사 · 챕터", "대사", "수수께끼", "그림" };
            w.tabs = new Button[8]; for (int i = 0; i < 8; i++) w.tabs[i] = Button(p, "Tab " + i, tabs[i], 40 + i * 190, 103, 178, 44, Teal, 22);
            w.status = Text(p, "Status and errors", "저장 상태", 40, 157, 1520, 38, 18, Gold);
            w.rows = new Button[12]; for (int i = 0; i < 12; i++) w.rows[i] = Button(p, "Item " + i, "항목", 40, 206 + i * 43, 285, 37, Panel, 17);
            w.previous = Button(p, "Previous items", "←", 40, 731, 65, 37, Teal, 21);
            w.page = Text(p, "Items page", "1 / 1", 110, 735, 140, 30, 19, Gold, FontStyle.Normal, TextAnchor.MiddleCenter);
            w.next = Button(p, "Next items", "→", 260, 731, 65, 37, Teal, 21);
            w.add = Button(p, "Add item", "추가", 40, 778, 65, 35, Teal, 17);
            w.remove = Button(p, "Delete item", "삭제", 113, 778, 65, 35, Teal, 17);
            w.up = Button(p, "Move up", "위로", 186, 778, 65, 35, Teal, 17);
            w.down = Button(p, "Move down", "아래로", 259, 778, 66, 35, Teal, 17);
            w.context = Text(p, "Selection context", "선택한 항목", 360, 202, 740, 28, 21, Gold);
            w.fields = new InputField[9]; w.labels = new Text[9]; w.fieldPickers = new Button[9];
            for (int i = 0; i < 9; i++)
            {
                float y = 242 + i * 47;
                w.labels[i] = Text(p, "Field label " + i, "설정", 360, y, 735, 18, 15, Muted);
                var bg = Box(p, "Field " + i, 360, y + 19, 652, 25, Panel, true); bg.raycastTarget = true;
                w.fields[i] = bg.gameObject.AddComponent<InputField>(); w.fields[i].targetGraphic = bg;
                w.fields[i].textComponent = Text(bg.transform, "Value", "", 8, 0, 634, 25, 19, Paper, FontStyle.Normal, TextAnchor.MiddleLeft);
                w.fields[i].characterLimit = 300;
                w.fieldPickers[i] = Button(p, "Select reference " + i, "선택", 1022, y + 19, 70, 25, Teal, 15);
            }
            w.longLabel = Text(p, "Long text label", "내용", 360, 674, 730, 22, 18, Gold);
            var longBg = Box(p, "Long text editor", 360, 701, 732, 100, Panel, true); longBg.raycastTarget = true;
            w.longText = longBg.gameObject.AddComponent<InputField>(); w.longText.targetGraphic = longBg;
            w.longText.textComponent = Text(longBg.transform, "Text", "", 12, 8, 708, 84, 20, Paper);
            w.longText.lineType = InputField.LineType.MultiLineNewline; w.longText.characterLimit = 4000;
            var frame = Box(p, "Scene and asset preview", 1120, 242, 440, 248, Panel, true); frame.raycastTarget = true;
            w.previewPoint = frame.gameObject.AddComponent<HotelStudioPreviewPoint>();
            w.preview = Box(frame.transform, "Preview image", 0, 0, 440, 248, Color.white); w.preview.preserveAspect = true;
            w.preview.rectTransform.pivot = Vector2.one * .5f; Stretch(w.preview.rectTransform, 0);
            w.marker = Box(frame.transform, "Investigation point", 0, 0, 24, 24, Gold, true); w.marker.rectTransform.pivot = Vector2.one * .5f;
            w.previewCaption = Text(p, "Preview caption", "미리보기", 1120, 498, 440, 52, 17, Muted);
            w.special = Button(p, "Edit linked content", "그림 편집", 1120, 565, 440, 38, Teal, 21);
            w.dialogue = Button(p, "Edit selected dialogue", "선택한 조사의 대사", 1120, 613, 440, 38, Teal, 21);
            w.selectedTrial = Button(p, "Test selected event", "현재 조사 시험", 1120, 661, 440, 38, Teal, 21);
            w.textMode = Button(p, "Switch text section", "조사 전 / 후 대사", 1120, 709, 440, 38, Teal, 20);
            w.importImage = Button(p, "Import PNG", "PNG 가져오기", 1120, 709, 260, 38, Teal, 20);
            w.clearImage = Button(p, "Reset PNG", "기본 그림", 1390, 709, 170, 38, Teal, 19);
            w.copy = Button(p, "Copy hotel story", "호텔 이야기를 복사해 시작", 1120, 757, 440, 38, Teal, 20);
            w.chain = Button(p, "Connect list order", "목록 순서대로 조사 연결", 1120, 757, 440, 38, Teal, 20);
            w.save = Button(p, "Save project", "프로젝트 저장", 40, 839, 210, 46, Teal, 21);
            w.trial = Button(p, "Test full story", "이야기 처음부터 시험", 265, 839, 285, 46, Teal, 21);
            w.export = Button(p, "Export story MOD", "독립 MOD 내보내기", 565, 839, 280, 46, Hex("766247"), 21);
            w.folder = Button(p, "Story exports folder", "내보낸 폴더", 860, 839, 210, 46, Teal, 21);
            w.back = Button(p, "Studio back", "저장 후 타이틀", 1320, 839, 240, 46, Teal, 21);
            w.pickerPanel = Box(p, "Reference picker overlay", 0, 0, 1600, 900, new Color(0, 0, 0, .85f)).gameObject; w.pickerPanel.GetComponent<Image>().raycastTarget = true;
            var picker = Box(w.pickerPanel.transform, "Selection card", 360, 92, 880, 730, Panel, true).transform;
            w.pickerTitle = Text(picker, "Reference type", "선택", 30, 25, 810, 40, 29, Gold);
            w.pickerRows = new Button[10]; for (int i = 0; i < 10; i++) w.pickerRows[i] = Button(picker, "Choice " + i, "선택지", 30, 91 + i * 48, 820, 40, Teal, 21);
            w.pickerPrevious = Button(picker, "Previous choices", "←", 30, 650, 70, 45);
            w.pickerPage = Text(picker, "Choices page", "1 / 1", 110, 658, 120, 35, 21, Gold);
            w.pickerNext = Button(picker, "Next choices", "→", 240, 650, 70, 45);
            w.pickerClose = Button(picker, "Close choices", "닫기", 650, 650, 200, 45);
            w.pickerPanel.SetActive(false);
            Directory.CreateDirectory("Assets/Game/Hotel/Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(panel, "Assets/Game/Hotel/Prefabs/StoryStudio.prefab", InteractionMode.AutomatedAction);
            var controller = v.gameObject.AddComponent<HotelStoryStudio>(); var so = new SerializedObject(controller);
            so.FindProperty("view").objectReferenceValue = w; so.ApplyModifiedPropertiesWithoutUndo(); v.storyStudio = controller;
            v.storyStudioButton = Button(v.titleScreen.transform, "Story studio", "이야기 제작실", 1110, 735, 400, 58, Hex("766247"), 24);
            v.studioReturnButton = Button(root, "Return from studio preview", "시험 중 · 제작실로 돌아가기", 640, 3, 400, 32, Teal, 18); v.studioReturnButton.gameObject.SetActive(false);
            v.gameTitle = v.titleScreen.transform.Find("Game title").GetComponent<Text>();
            v.endingHeading = v.endingScreen.transform.Find("Ending title").GetComponent<Text>();
            var hud = v.explorationHud.transform;
            v.roomPrevious = Button(hud, "Previous room page", "◀", 740, 825, 36, 45, Teal, 20);
            v.roomNext = Button(hud, "Next room page", "▶", 778, 825, 36, 45, Teal, 20);
            v.roomPrevious.gameObject.SetActive(false); v.roomNext.gameObject.SetActive(false);
            v.Show(v.titleScreen); EditorUtility.SetDirty(v); EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_STUDIO_AUTHORED");
        }
        public static void AddStoryStudioAndBuild() { AddStoryStudio(); CreateReleaseBuild(); }
    }
}
