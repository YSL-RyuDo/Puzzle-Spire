using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelStoryStudio : MonoBehaviour
    {
        [SerializeField] private HotelStoryStudioView view;
        private HotelStorage storage;
        private Action close;
        private Action<string> previewStory;
        private Action<HotelStage, PuzzleClearance, string, Action<PuzzleWorkshopDraft>> editPuzzle;
        private Func<string, Sprite> defaultImage;
        private bool wired, filling;
        private int tab, selected, page, pickerPage, pickerField, riddleText;
        private bool dialogueAfter;
        private string nodeId, selectedKey, importPath = "", deleteArm;
        private readonly List<Field> fields = new List<Field>();
        private Action<string> longSet;
        private Choice[] choices;
        private Texture2D previewTexture;
        private Sprite previewSprite;
        private string[] projects = Array.Empty<string>();
        public HotelStoryStudioView View => view;
        public HotelStoryProject Project { get; private set; }
        public string ProjectFolder { get; private set; }
        private string ProjectsRoot => Path.Combine(storage.Root, "StoryStudio", "Projects");
        private InvestigationNode Node => Project.pack.nodes.FirstOrDefault(n => n.id == nodeId) ?? Project.pack.nodes.FirstOrDefault();
        private sealed class Choice { public string value, label; public Choice(string v, string l) { value = v; label = l; } }
        private sealed class Field { public string label; public Func<string> get; public Action<string> set; public Choice[] choices; }
        private static T[] Append<T>(T[] list, T item) => list.Concat(new[] { item }).ToArray();
        private static int Number(string text) { ContentValidation.Require(int.TryParse(text, out int value), "숫자를 입력하세요."); return value; }
        private static float Fraction(string text) { ContentValidation.Require(float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && value >= 0 && value <= 100, "좌표는 0~100 사이 숫자로 입력하세요."); return value / 100; }
        private void Button(UnityEngine.UI.Button b, Action action) => b.onClick.AddListener(() => Safe(action));
        private void Safe(Action action) { try { action(); } catch (Exception e) { view.status.text = e.Message; } }
        public void Open(HotelStorage content, Func<string, Sprite> defaults, Action<string> play, Action<HotelStage, PuzzleClearance, string, Action<PuzzleWorkshopDraft>> puzzle, Action onClose)
        {
            storage = content; defaultImage = defaults; previewStory = play; editPuzzle = puzzle; close = onClose;
            if (!wired) Wire();
            Directory.CreateDirectory(ProjectsRoot); RefreshProjects();
            if (Project == null)
            {
                if (projects.Length > 0) LoadProject(projects[0]); else NewProject(false);
            }
            view.pickerPanel.SetActive(false); Render();
        }
        private void Wire()
        {
            wired = true;
            for (int i = 0; i < view.tabs.Length; i++) { int t = i; Button(view.tabs[i], () => { Commit(); tab = t; selected = tab == 4 ? Math.Max(0, Array.FindIndex(Project.pack.nodes, n => n.id == nodeId)) : 0; page = selected / view.rows.Length; deleteArm = null; Render(); }); }
            for (int i = 0; i < view.rows.Length; i++) { int row = i; Button(view.rows[i], () => Select(page * view.rows.Length + row)); }
            for (int i = 0; i < view.fields.Length; i++)
            {
                int f = i;
                view.fields[i].onEndEdit.AddListener(_ => { if (!filling) Safe(() => { Commit(); Render(); }); });
                Button(view.fieldPickers[i], () => OpenPicker(f));
            }
            view.longText.onEndEdit.AddListener(_ => { if (!filling) Safe(() => { Commit(); Render(); }); });
            Button(view.previous, () => { Commit(); page--; Render(); }); Button(view.next, () => { Commit(); page++; Render(); });
            Button(view.add, () => { Commit(); Add(); }); Button(view.remove, Delete);
            Button(view.up, () => Move(-1)); Button(view.down, () => Move(1));
            Button(view.chain, () => { Commit(); HotelStoryProjects.Chain(Project.pack); Save(); Render(); view.status.text = "조사 목록 순서대로 선행 조사를 연결했어요."; });
            Button(view.copy, () => { Commit(); NewProject(true); });
            Button(view.save, () => { Commit(); Save(); view.status.text = "프로젝트와 그림을 저장했어요. 프로젝트 탭에서 다시 열 수 있습니다."; });
            Button(view.back, () => { Commit(); Save(); close(); });
            Button(view.trial, () => { Commit(); Save(); previewStory(null); });
            Button(view.selectedTrial, () => { Commit(); Save(); previewStory(Node?.id); });
            Button(view.export, () => { Commit(); Save(); string zip = HotelStoryProjects.Export(Project, ProjectFolder, storage); view.status.text = "독립 이야기 MOD 등록 완료 · " + Path.GetFileName(zip) + " · 내보낸 폴더에서 공유하세요."; });
            Button(view.folder, () => { string folder = Path.Combine(storage.Root, "StoryStudio", "Exports"); Directory.CreateDirectory(folder); Application.OpenURL(new Uri(folder + Path.DirectorySeparatorChar).AbsoluteUri); });
            Button(view.dialogue, () => { Commit(); tab = 5; selected = page = 0; Render(); });
            Button(view.special, Special);
            Button(view.textMode, () => { Commit(); if (tab == 5) { dialogueAfter = !dialogueAfter; selected = page = 0; } else riddleText = (riddleText + 1) % 3; Render(); });
            Button(view.importImage, () => { Commit(); string path = string.IsNullOrWhiteSpace(importPath) ? HotelPngPicker.Open() : importPath.Trim().Trim('"'); if (string.IsNullOrEmpty(path)) return; ImportImage(path); });
            Button(view.clearImage, () => { Commit(); Project.images = Project.images.Where(i => i.key != selectedKey).ToArray(); Save(); Render(); });
            view.previewPoint.clicked = position => Safe(() => { if (tab != 4 || Node == null) return; Commit(); Node.x = position.x; Node.y = position.y; Save(); Render(); });
            Button(view.pickerClose, () => view.pickerPanel.SetActive(false));
            Button(view.pickerPrevious, () => { pickerPage--; RenderPicker(); }); Button(view.pickerNext, () => { pickerPage++; RenderPicker(); });
            for (int i = 0; i < view.pickerRows.Length; i++) { int index = i; Button(view.pickerRows[i], () => { var option = choices[pickerPage * view.pickerRows.Length + index]; fields[pickerField].set(option.value); Save(); view.pickerPanel.SetActive(false); Render(); }); }
        }
        private void RefreshProjects() => projects = Directory.GetDirectories(ProjectsRoot).Where(d => File.Exists(Path.Combine(d, "project.json"))).OrderBy(d => d).ToArray();
        private void NewProject(bool copy)
        {
            Project = HotelStoryProjects.Create(storage.BasePack, copy); ProjectFolder = Path.Combine(ProjectsRoot, Project.id);
            Save(); RefreshProjects(); nodeId = Project.pack.nodes[0].id; tab = 0; selected = Array.IndexOf(projects, ProjectFolder); page = selected / view.rows.Length; Render();
        }
        private void LoadProject(string folder)
        {
            var project = HotelStorage.ReadJson<HotelStoryProject>(Path.Combine(folder, "project.json"));
            ContentValidation.Require(project.schemaVersion == 1 && project.pack?.nodes != null && project.images != null && project.proofs != null, "프로젝트 형식이 올바르지 않습니다.");
            Project = project; ProjectFolder = folder; nodeId = Project.pack.nodes.FirstOrDefault()?.id; selected = Math.Max(0, Array.IndexOf(projects, folder)); page = selected / view.rows.Length;
        }
        public void Save() => HotelStoryProjects.Save(ProjectFolder, Project);
        private void Commit()
        {
            if (filling || Project == null) return;
            for (int i = 0; i < fields.Count; i++) if (fields[i].choices == null) fields[i].set(view.fields[i].text);
            longSet?.Invoke(view.longText.text); Save();
        }
        private void Select(int index)
        {
            Commit(); selected = index; deleteArm = null;
            if (tab == 0) LoadProject(projects[index]);
            if (tab == 4) nodeId = Project.pack.nodes[index].id;
            if (tab == 7) selectedKey = ImageKeys()[index];
            Render();
        }
        private void FieldValue(string label, Func<string> get, Action<string> set, Choice[] options = null) => fields.Add(new Field { label = label, get = get, set = set, choices = options });
        private void LongValue(string label, string value, Action<string> set) { view.longLabel.text = label; view.longText.SetTextWithoutNotify(value ?? ""); longSet = set; }
        private Choice[] Rooms() => Project.pack.locations.Select(x => new Choice(x.id, x.name)).ToArray();
        private Choice[] People() => Project.pack.characters.Select(x => new Choice(x.id, x.name)).ToArray();
        private Choice[] Nodes(bool empty) => (empty ? new[] { new Choice("", "없음") } : Array.Empty<Choice>()).Concat(Project.pack.nodes.Where(n => n != Node).Select(n => new Choice(n.id, n.chapter + " / " + n.title))).ToArray();
        private DialogueLine[] Lines => Node == null ? Array.Empty<DialogueLine>() : dialogueAfter ? Node.after : Node.before;
        private void SetLines(DialogueLine[] lines) { if (dialogueAfter) Node.after = lines; else Node.before = lines; }
        private string[] ImageKeys()
        {
            var keys = new List<string>();
            keys.AddRange(Project.pack.locations.Select(l => l.image ?? "background." + l.id));
            foreach (var c in Project.pack.characters)
            {
                keys.Add("character." + c.id);
                keys.AddRange(new[] { "curious", "found", "worried", "remember", "think", "explain", "warm" }.Select(p => "character." + c.id + "." + p));
            }
            keys.AddRange(Project.pack.clues.Select(c => "clue." + c.id)); keys.AddRange(HotelThemeKeys.Pieces);
            keys.AddRange(Project.images.Select(i => i.key));
            return keys.Distinct().ToArray();
        }
        private string[] Rows()
        {
            switch (tab)
            {
                case 0: return projects.Select(p => { try { return HotelStorage.ReadJson<HotelStoryProject>(Path.Combine(p, "project.json")).pack.title; } catch { return "읽을 수 없는 프로젝트"; } }).ToArray();
                case 1: return Project.pack.locations.Select(x => x.name).ToArray();
                case 2: return Project.pack.characters.Select(x => (x.id == "you" ? "주인공 · " : "") + x.name).ToArray();
                case 3: return Project.pack.clues.Select(x => x.title).ToArray();
                case 4: return Project.pack.nodes.Select(x => x.chapter + " / " + x.title).ToArray();
                case 5: return Lines.Select((x, i) => (i + 1) + ". " + (Project.pack.characters.FirstOrDefault(c => c.id == x.speaker)?.name ?? x.speaker) + " · " + x.text).ToArray();
                case 6: return Project.pack.riddles.Select(x => x.title).ToArray();
                default: return ImageKeys();
            }
        }
        private void OpenPicker(int index) { Commit(); pickerField = index; choices = fields[index].choices; pickerPage = 0; view.pickerTitle.text = fields[index].label; view.pickerPanel.SetActive(true); RenderPicker(); }
        private void RenderPicker()
        {
            int pages = Math.Max(1, (choices.Length + view.pickerRows.Length - 1) / view.pickerRows.Length);
            pickerPage = Mathf.Clamp(pickerPage, 0, pages - 1); view.pickerPage.text = (pickerPage + 1) + " / " + pages;
            view.pickerPrevious.interactable = pickerPage > 0; view.pickerNext.interactable = pickerPage < pages - 1;
            for (int i = 0; i < view.pickerRows.Length; i++) { int at = pickerPage * view.pickerRows.Length + i; view.pickerRows[i].gameObject.SetActive(at < choices.Length); if (at < choices.Length) HotelView.Label(view.pickerRows[i], choices[at].label); }
        }
    }
}
