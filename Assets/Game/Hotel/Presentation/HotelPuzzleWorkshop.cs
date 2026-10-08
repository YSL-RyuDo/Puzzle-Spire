using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    // Only binds authored widgets; all UI lives in the scene/prefab.
    public sealed class HotelPuzzleWorkshop : MonoBehaviour
    {
        [SerializeField] private HotelWorkshopView view;
        private HotelStorage storage;
        private Func<int, Sprite> piece;
        private Action close;
        private PuzzleWorkshopDraft draft;
        private HotelPuzzle play;
        private HotelStage playStage;
        private readonly List<int> clicks = new List<int>();
        private readonly Stack<string> undo = new Stack<string>(), redo = new Stack<string>();
        private int target, brush;
        private bool wired, filling, certifying;
        private string inputError, lastExport;
        private string projectDraftPath;
        private Action<PuzzleWorkshopDraft> projectSaved;
        public HotelWorkshopView View => view;
        public HotelPuzzle Play => play;
        public PuzzleWorkshopDraft Draft => draft;
        private string DraftPath => projectDraftPath ?? Path.Combine(storage.Root, "Workshop", "Drafts", storage.BasePack.stages[target].id + ".json");

        public void Open(HotelStorage content, Func<int, Sprite> sprite, Action onClose)
        {
            storage = content; piece = sprite; close = onClose;
            projectDraftPath = null; projectSaved = null;
            if (!wired) Wire();
            target = 0; play = null;
            LoadTarget();
        }
        public void OpenProject(HotelStorage content, string path, PuzzleWorkshopDraft data, Func<int, Sprite> sprite, Action<PuzzleWorkshopDraft> saved, Action onClose)
        {
            storage = content; piece = sprite; close = onClose; projectDraftPath = path; projectSaved = saved;
            if (!wired) Wire();
            target = 0; play = null; playStage = null; draft = data; draft.stage.fixedSeed = true;
            HotelWorkshopFiles.SaveDraft(path, draft); inputError = null; undo.Clear(); redo.Clear(); Fill(); Render();
        }
        private void Button(Button button, Action action) => button.onClick.AddListener(() => Safe(action));
        private void Safe(Action action)
        {
            try { action(); }
            catch (Exception e) { view.status.text = e.Message; }
        }
        private void Wire()
        {
            wired = true;
            Button(view.previous, () => ChangeTarget(-1)); Button(view.next, () => ChangeTarget(1));
            Button(view.save, () => { Save(); view.status.text = "초안을 저장했어요. 다음 실행에서도 이어서 편집할 수 있습니다."; });
            Button(view.reload, () => { LoadTarget(); view.status.text = "마지막으로 저장한 초안을 불러왔어요."; });
            Button(view.test, () => BeginPlay(false)); Button(view.verify, () => BeginPlay(true));
            Button(view.stop, StopPlay); Button(view.export, Export);
            Button(view.folder, () => {
                string path = Path.Combine(storage.Root, "Workshop", "Exports"); Directory.CreateDirectory(path);
                Application.OpenURL(new Uri(path + Path.DirectorySeparatorChar).AbsoluteUri);
            });
            Button(view.back, () => { RequireEditing(); Save(); close(); });
            Button(view.undo, () => Restore(undo, redo)); Button(view.redo, () => Restore(redo, undo));
            for (int i = 0; i < view.brushes.Length; i++) { int b = i; Button(view.brushes[i], () => { RequireEditing(); brush = b; Render(); }); }
            for (int i = 0; i < view.cells.Length; i++) { int cell = i; Button(view.cells[i], () => Click(cell)); }
            foreach (var field in view.fields) field.onValueChanged.AddListener(_ => { if (!filling) Safe(ApplyFields); });
        }
        private void RequireEditing() => ContentValidation.Require(play == null, "플레이를 끝내고 편집으로 돌아와 주세요.");
        private void ChangeTarget(int delta)
        {
            RequireEditing(); Save(); target = (target + delta + storage.BasePack.stages.Length) % storage.BasePack.stages.Length; LoadTarget();
        }
        private void LoadTarget()
        {
            RequireEditing();
            draft = File.Exists(DraftPath) ? HotelWorkshopFiles.LoadDraft(DraftPath, projectSaved != null ? draft.stage.id : storage.BasePack.stages[target].id)
                : new PuzzleWorkshopDraft { stage = HotelPuzzleClearance.Clone(storage.BasePack.stages[target]) };
            draft.stage.fixedSeed = true;
            inputError = null; undo.Clear(); redo.Clear(); Fill(); Render();
            view.status.text = "수정한 퍼즐은 기본 이야기의 이 자리를 교체합니다. 초안 저장은 검증 전에도 가능합니다.";
        }
        private void Fill()
        {
            filling = true;
            var s = draft.stage;
            string[] values = { s.title, s.width.ToString(), s.height.ToString(), s.moves.ToString(), s.colors.ToString(), s.targetColor.ToString(), s.targetAmount.ToString(), s.boxes.ToString(), s.ice.ToString(), s.seed.ToString() };
            for (int i = 0; i < values.Length; i++) view.fields[i].text = values[i];
            filling = false;
        }
        private void Remember() { undo.Push(JsonUtility.ToJson(draft.stage)); redo.Clear(); }
        private void Restore(Stack<string> source, Stack<string> destination)
        {
            RequireEditing(); if (source.Count == 0) return;
            destination.Push(JsonUtility.ToJson(draft.stage)); draft.stage = JsonUtility.FromJson<HotelStage>(source.Pop());
            inputError = null; Invalidate(); Fill(); Render();
        }
        private void Invalidate()
        {
            if (draft.clearance != null && draft.clearance.fingerprint != HotelPuzzleClearance.Fingerprint(draft.stage)) draft.clearance = null;
        }
        private void ApplyFields()
        {
            RequireEditing();
            var numbers = new int[9];
            for (int i = 0; i < numbers.Length; i++)
                if (!int.TryParse(view.fields[i + 1].text, NumberStyles.Integer, CultureInfo.InvariantCulture, out numbers[i]))
                { inputError = "숫자 입력을 완료해 주세요. 입력 중에는 플레이와 내보내기가 잠깁니다."; Render(); return; }
            if (numbers[0] < 3 || numbers[0] > 9 || numbers[1] < 3 || numbers[1] > 9)
            { inputError = "가로·세로는 3~9칸으로 설정하세요."; Render(); return; }
            inputError = null;
            var s = draft.stage; var updated = HotelPuzzleClearance.Clone(s);
            updated.title = view.fields[0].text;
            updated.width = numbers[0]; updated.height = numbers[1]; updated.moves = numbers[2]; updated.colors = numbers[3];
            updated.targetColor = numbers[4]; updated.targetAmount = numbers[5]; updated.boxes = numbers[6]; updated.ice = numbers[7]; updated.seed = numbers[8];
            if (updated.width != s.width || updated.height != s.height)
            {
                updated.layout = Enumerable.Range(0, updated.height).Select(row => new string(Enumerable.Range(0, updated.width).Select(x => {
                    int oldRow = s.height - updated.height + row;
                    return oldRow >= 0 && oldRow < s.height && x < s.width ? s.layout[oldRow][x] : 'O';
                }).ToArray())).ToArray();
                updated.obstacles = updated.obstacles.Where(o => o.x < updated.width && o.y < updated.height).ToArray();
            }
            if (JsonUtility.ToJson(s) != JsonUtility.ToJson(updated)) { Remember(); draft.stage = updated; Invalidate(); }
            Render();
        }
        private void Save()
        {
            RequireEditing(); ContentValidation.Require(inputError == null, inputError);
            HotelWorkshopFiles.SaveDraft(DraftPath, draft);
            projectSaved?.Invoke(draft);
        }
        private void Click(int index)
        {
            var s = playStage ?? draft.stage;
            if (index < 0 || index >= s.width * s.height) return;
            int x = index % s.width, y = s.height - 1 - index / s.width;
            if (play != null)
            {
                if (play.Cleared || play.Failed || !play.Board.IsPlayable(new GridPosition(x, y))) return;
                int before = play.Session.RemainingMoves;
                play.Play(new GridPosition(x, y));
                if (before == play.Session.RemainingMoves) return;
                clicks.Add(index);
                if (play.Cleared && certifying)
                {
                    draft.clearance = HotelPuzzleClearance.Certify(playStage, clicks);
                    HotelWorkshopFiles.SaveDraft(DraftPath, draft);
                }
                Render(); return;
            }
            ContentValidation.Require(inputError == null, inputError);
            Remember();
            var row = s.layout[index / s.width].ToCharArray();
            var obstacles = s.obstacles.Where(o => o.x != x || o.y != y).ToList();
            if (brush != 4) row[x] = brush == 1 ? 'X' : 'O';
            if (brush == 2 || brush == 3)
            {
                var previous = s.obstacles.FirstOrDefault(o => o.x == x && o.y == y);
                string type = brush == 2 ? "box" : "ice";
                obstacles.Add(new ObstacleData { x = x, y = y, type = type, durability = previous?.type == type ? previous.durability % 3 + 1 : 1 });
            }
            s.layout[index / s.width] = new string(row); s.obstacles = obstacles.ToArray(); Invalidate(); Render();
        }
        private void BeginPlay(bool verification)
        {
            RequireEditing(); ContentValidation.Require(inputError == null, inputError);
            ContentValidation.ValidateStage(draft.stage); Save();
            playStage = HotelPuzzleClearance.Clone(draft.stage); play = new HotelPuzzle(playStage); clicks.Clear(); certifying = verification; Render();
        }
        private void StopPlay()
        {
            play = null; playStage = null; Render();
        }
        private void Export()
        {
            RequireEditing(); Save();
            if (projectSaved != null) { view.status.text = "검증된 퍼즐을 이야기에 저장했어요. ‘저장 후 제작실’로 돌아가세요."; return; }
            lastExport = HotelWorkshopFiles.Export(storage, draft);
            Render(); view.status.text = "MOD 등록 완료! 타이틀의 MOD 관리에서 선택하세요.\nZIP: " + Path.GetFileName(lastExport) + "  ·  ‘내보낸 폴더’에서 공유할 파일을 확인하세요.";
        }
        private string ValidationMessage()
        {
            if (inputError != null) return inputError;
            try { ContentValidation.ValidateStage(draft.stage); return null; }
            catch (Exception e) { return e.Message; }
        }
        private void Render()
        {
            var s = playStage ?? draft.stage;
            bool editing = play == null;
            string error = ValidationMessage();
            view.stageLabel.text = projectSaved != null ? "이야기 퍼즐 · " + draft.stage.title : (target + 1) + " / " + storage.BasePack.stages.Length + "  ·  " + storage.BasePack.stages[target].title + " 교체";
            HotelView.Label(view.back, projectSaved != null ? "저장 후 제작실" : "저장 후 타이틀");
            HotelView.Label(view.export, projectSaved != null ? "검증된 퍼즐 저장" : "검증된 MOD 내보내기");
            view.modeLabel.text = editing ? "퍼즐 편집" : certifying ? "클리어 검증 · 처음부터 직접 완료하세요" : "시험 플레이 · 인증에는 반영되지 않아요";
            bool verified = draft.clearance != null && draft.clearance.fingerprint == HotelPuzzleClearance.Fingerprint(draft.stage);
            view.certificate.text = verified ? "✓ 클리어 확인됨 · " + draft.clearance.clicks.Length + "회" : "클리어 검증 필요";
            view.certificate.color = verified ? new Color(.55f, .9f, .68f) : new Color(.95f, .75f, .45f);
            foreach (var field in view.fields) field.interactable = editing;
            view.previous.interactable = view.next.interactable = view.save.interactable = view.back.interactable = editing && inputError == null;
            if (projectSaved != null) view.previous.interactable = view.next.interactable = false;
            view.reload.interactable = editing;
            view.undo.interactable = editing && undo.Count > 0; view.redo.interactable = editing && redo.Count > 0;
            view.test.interactable = view.verify.interactable = editing && error == null;
            view.export.interactable = editing && verified && error == null;
            view.stop.interactable = !editing;
            for (int i = 0; i < view.brushes.Length; i++) { view.brushes[i].interactable = editing; view.brushes[i].GetComponent<Image>().color = i == brush ? new Color(.51f, .4f, .23f) : new Color(.19f, .36f, .38f); }
            view.brushHelp.text = "칸을 클릭해 배치하세요.\n상자·얼음을 반복 클릭하면\n내구도 1 → 2 → 3으로 변경됩니다.\n\n색 번호\n0 산호 · 1 파랑 · 2 초록\n3 노랑 · 4 보라\n\n초기 블록은 시드로 정해집니다.";
            view.status.text = error ?? (editing ? "편집 → 시험 플레이 → 클리어 검증 → MOD 내보내기\n배치·목표·횟수·시드를 바꾸면 인증을 다시 받아야 합니다." : play.Cleared ? (certifying ? "클리어 검증 완료! 편집으로 돌아가 MOD를 내보낼 수 있어요." : "시험 플레이 성공! 배포하려면 클리어 검증으로 완주하세요.") : play.Failed ? "이동 횟수를 모두 사용했어요. 편집으로 돌아가 다시 도전하세요." : "같은 색 2개 이상을 클릭하세요. 특수 블록은 하나만 클릭해도 작동합니다.");
            view.goals.text = editing ? "상자 " + s.obstacles.Count(o => o.type == "box") + "개 / 얼음 " + s.obstacles.Count(o => o.type == "ice") + "개 배치" : "남은 이동 " + play.Session.RemainingMoves + "회  ·  " + string.Join("  ·  ", play.Objectives.Objectives.Select(g => (g is RemoveColorObjective ? "색" : g is RemoveBoxObjective ? "상자" : "얼음") + " " + Math.Min(g.CurrentAmount, g.TargetAmount) + "/" + g.TargetAmount));
            view.grid.constraintCount = s.width;
            float size = Mathf.Min((560 - (s.width - 1) * 6f) / s.width, (560 - (s.height - 1) * 6f) / s.height);
            view.grid.cellSize = new Vector2(size, size);
            for (int index = 0; index < view.cells.Length; index++)
            {
                bool active = index < s.width * s.height; view.cells[index].gameObject.SetActive(active); if (!active) continue;
                int x = index % s.width, y = s.height - 1 - index / s.width;
                var pos = new GridPosition(x, y);
                bool playable = s.layout[index / s.width][x] == 'O';
                var obstacle = s.obstacles.FirstOrDefault(o => o.x == x && o.y == y);
                int durability = obstacle?.durability ?? 0;
                Block block = play != null && playable ? play.Board.GetBlock(pos) : null;
                if (play != null && obstacle != null)
                    durability = (play.Board.GetObstacle(pos, obstacle.type == "box" ? ObstacleLayer.Blocking : ObstacleLayer.Overlay) as IDamageableObstacle)?.CurrentDurability ?? 0;
                view.cells[index].interactable = editing || (!play.Cleared && !play.Failed && block != null);
                view.cells[index].GetComponent<Image>().color = playable ? new Color(.16f, .28f, .3f) : new Color(.055f, .09f, .11f);
                view.pieces[index].enabled = block != null;
                if (block != null) view.pieces[index].sprite = piece(block.Type == BlockType.Normal ? (int)block.Color : block.Type == BlockType.HorizontalRocket ? 5 : block.Type == BlockType.VerticalRocket ? 6 : block.Type == BlockType.Bomb ? 7 : 8);
                view.obstacles[index].enabled = durability > 0;
                if (durability > 0) view.obstacles[index].sprite = piece(obstacle.type == "box" ? (durability < obstacle.durability ? 10 : 9) : (durability < obstacle.durability ? 12 : 11));
                view.cellLabels[index].text = durability > 0 ? durability.ToString() : editing ? (playable ? "·" : "×") : "";
            }
        }
    }
}
