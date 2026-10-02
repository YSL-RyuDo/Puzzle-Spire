using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame : MonoBehaviour
    {
        [SerializeField] private HotelView view;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0.05f, 1f)] private float turnAnimationSeconds = .22f;
        [SerializeField] private Color[] blockColors = { new Color(.83f,.38f,.34f), new Color(.31f,.59f,.79f), new Color(.39f,.68f,.52f), new Color(.87f,.68f,.29f), new Color(.66f,.49f,.75f) };
        [Header("Default art · Inspector에서 교체 / MOD가 있으면 덮어씀")]
        [SerializeField] private Sprite[] defaultBlockSprites = new Sprite[5];
        [SerializeField] private Sprite[] defaultPortraits = new Sprite[4];
        private Sprite[] defaultBackgrounds;
        private HotelStorage storage;
        private InvestigationSession story;
        private InvestigationNode activeNode;
        private HotelPuzzle puzzle;
        private HotelStage stage;
        private RiddleData riddle;
        private readonly List<int> selection = new List<int>();
        private DialogueLine[] lines;
        private int dialogueIndex, hints, attempt, catalogPage;
        private Action afterDialogue;
        private bool practice, busy, notebookFromEnding, smoke;
        private string catalogMode = "mods", screen = "title", toast = "";
        private AudioClip clickClip;
        public HotelView View => view;

        private void Start()
        {
            smoke = Environment.GetCommandLineArgs().Contains("-hotel-smoke");
            bool preview = Environment.GetCommandLineArgs().Contains("-hotel-preview");
            bool fullPlay = Environment.GetCommandLineArgs().Contains("-hotel-full-play");
            bool visualReview = Environment.GetCommandLineArgs().Contains("-hotel-effects-review");
            try
            {
                string root = smoke || preview || fullPlay || visualReview ? Path.Combine(Path.GetTempPath(), "PuzzleSpire-Smoke-" + Guid.NewGuid().ToString("N")) : Application.persistentDataPath;
                storage = new HotelStorage(Path.Combine(Application.streamingAssetsPath, "Hotel", "base.json"), root);
                defaultBackgrounds = view.locationImages.Select(i => i.sprite).ToArray();
                WireButtons();
                MakeSound();
                ShowTitle();
                if (!string.IsNullOrEmpty(storage.Warning)) Notice("콘텐츠 확인", storage.Warning, ShowTitle);
                if (smoke) StartCoroutine(SmokeTest());
                else if (preview) StartCoroutine(PresentationPreview());
                else if (fullPlay) BeginFullPlayRecording();
                else if (visualReview) StartCoroutine(ReviewPresentation());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                view.Show(view.titleScreen);
                Notice("시작할 수 없습니다", e.Message, () => Application.Quit());
            }
        }
        private void Bind(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { if (!busy) { Sound(); Guard(action); } });
        }
        private void Guard(Action action)
        {
            try { action(); }
            catch (Exception e) { Debug.LogException(e); Notice("확인이 필요합니다", e.Message, ShowTitle); }
        }
        private void WireButtons()
        {
            Bind(view.newButton, () => { if (storage.HasSave) Confirm("새 조사 시작", "선택한 콘텐츠의 진행을 처음부터 시작합니다. 다른 MOD의 저장은 유지됩니다.", NewGame, ShowTitle); else NewGame(); });
            Bind(view.continueButton, ContinueGame);
            Bind(view.practiceButton, () => ShowCatalog("practice"));
            Bind(view.modsButton, () => ShowCatalog("mods"));
            Bind(view.settingsButton, ShowSettings);
            Bind(view.quitButton, () => Application.Quit());
            Bind(view.exploreTitleButton, ShowTitle);
            Bind(view.notebookButton, () => ShowNotebook(false));
            Bind(view.notebookBack, () => { if (notebookFromEnding) ShowEnding(); else ShowExplore(); });
            Bind(view.nextLocation, () => { var next = story.Next; if (next != null) MoveLocation(next.location); });
            for (int i = 0; i < view.locationButtons.Length; i++)
            {
                int index = i;
                Bind(view.locationButtons[i], () => MoveLocation(storage.Pack.locations[index].id));
            }
            Bind(view.dialogueNext, NextDialogue);
            Bind(view.retryButton, () => Confirm("다시 조사하기", "현재 보드를 새로 시작합니다. 모은 단서는 유지됩니다.", () => StartPuzzle(stage, practice, true), () => view.resultPanel.SetActive(false)));
            Bind(view.puzzleBackButton, LeavePuzzle);
            Bind(view.puzzleHintButton, PuzzleHint);
            Bind(Presentation.helpButton, () => { Presentation.helpPanel.SetActive(!Presentation.helpPanel.activeSelf); ClearHighlights(); });
            for (int i = 0; i < Presentation.props.Length; i++) { int index = i; Bind(Presentation.props[i], () => InspectProp(index)); }
            Bind(view.riddleBack, ShowExplore);
            Bind(view.resetAnswer, () => { selection.Clear(); RenderRiddle(); });
            Bind(view.riddleHint, RiddleHint);
            Bind(view.submitAnswer, SubmitRiddle);
            for (int i = 0; i < view.options.Length; i++) { int index = i; Bind(view.options[i], () => SelectOption(index)); }
            for (int i = 0; i < view.cells.Length; i++) { int index = i; Bind(view.cells[i].button, () => ClickCell(index)); view.cells[i].Hover = entered => HoverCell(index, entered); }
            Bind(view.catalogBack, ShowTitle);
            Bind(view.catalogPrevious, () => { catalogPage--; RenderCatalog(); });
            Bind(view.catalogNext, () => { catalogPage++; RenderCatalog(); });
            Bind(view.catalogFolder, () => Application.OpenURL(new Uri(storage.ModsPath + Path.DirectorySeparatorChar).AbsoluteUri));
            Bind(view.catalogRefresh, () => { storage.RefreshMods(); RenderCatalog(); });
            Bind(view.catalogSample, () => { storage.InstallExample(Path.Combine(Application.streamingAssetsPath, "Hotel", "ExampleMod")); RenderCatalog(); });
            Bind(view.soundButton, () => { storage.Preferences.sound = !storage.Preferences.sound; storage.SavePreferences(); ShowSettings(); });
            Bind(view.fullscreenButton, () => { Screen.fullScreen = !Screen.fullScreen; });
            Bind(view.settingsBack, ShowTitle);
            Bind(view.endingNotebook, () => ShowNotebook(true));
            Bind(view.endingTitle, ShowTitle);
        }
        public void ShowTitle()
        {
            screen = "title"; view.Show(view.titleScreen);
            view.continueButton.interactable = storage.HasSave;
            view.titleStatus.text = storage.PackName + "  ·  v" + storage.PackVersion + "\n조사 완료 시 자동 저장 · 퍼즐 중 종료하면 해당 조사부터 다시 시작";
        }
        private void NewGame()
        {
            story = new InvestigationSession(storage.Pack, storage.NewProgress());
            storage.Save(story.Progress); ShowExplore();
        }
        private void ContinueGame()
        {
            var progress = storage.Load(out string warning);
            story = new InvestigationSession(storage.Pack, progress);
            if (progress.ending) ShowEnding(); else ShowExplore();
            if (warning != null) Notice("저장 복구", warning, () => { if (progress.ending) ShowEnding(); else ShowExplore(); });
        }
        private void MoveLocation(string id)
        {
            var room = storage.Pack.locations.First(l => l.id == id);
            if (!story.IsUnlocked(room) || busy || story.Progress.location == id) return;
            busy = true;
            view.stagePresentation.ChangeRoom(() => Guard(() =>
            { story.Progress.location = id; storage.Save(story.Progress); ShowExplore(); }), () => busy = false, false);
        }
        private void ShowExplore()
        {
            screen = "explore"; view.Show(view.exploreScreen);
            var pack = storage.Pack;
            var room = pack.locations.First(l => l.id == story.Progress.location);
            int roomIndex = Array.IndexOf(pack.locations, room);
            var next = story.Next;
            view.chapter.text = next?.chapter ?? "모든 조사를 마쳤습니다";
            view.objective.text = next?.purpose ?? "달의 방에 새 이야기를 남겼습니다.";
            view.locationTitle.text = room.name; view.locationDescription.text = room.subtitle;
            view.progressText.text = "조사 " + story.Progress.completed.Count + " / " + pack.nodes.Length + "     단서 " + story.Progress.clues.Count + " / " + pack.clues.Length;
            for (int i = 0; i < view.locationButtons.Length; i++)
            {
                bool exists = i < pack.locations.Length;
                view.locationButtons[i].gameObject.SetActive(exists);
                view.locationArt[i].SetActive(exists && i == roomIndex);
                if (!exists) continue;
                var l = pack.locations[i];
                HotelView.Label(view.locationButtons[i], (i == roomIndex ? "● " : "") + l.name + (story.IsUnlocked(l) ? "" : " · 잠김"));
                view.locationButtons[i].interactable = story.IsUnlocked(l);
                var image = storage.Image(l.image ?? "background." + l.id) ?? defaultBackgrounds[i];
                view.locationImages[i].gameObject.SetActive(image != null && i == roomIndex);
                view.locationImages[i].sprite = image;
            }
            var local = pack.nodes.FirstOrDefault(n => n.location == room.id && story.IsAvailable(n));
            view.hotspot.gameObject.SetActive(local != null);
            if (local != null)
            {
                HotelView.Label(view.hotspot, "＋  " + local.title);
                // Only move the authored hotspot; the widget itself is a serialized scene object.
                var rect = (RectTransform)view.hotspot.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(local.x, 1 - local.y);
                rect.anchoredPosition = Vector2.zero;
                Bind(view.hotspot, () => BeginNode(local));
            }
            view.hotspotTitle.text = local?.title ?? "이곳의 조사를 마쳤습니다";
            view.hotspotDescription.text = local?.purpose ?? (next != null ? pack.locations.First(l => l.id == next.location).name + "에서 다음 단서를 찾아보세요." : "모은 단서를 수첩에서 다시 볼 수 있습니다.");
            view.nextLocation.gameObject.SetActive(next != null && next.location != room.id);
            if (next != null) HotelView.Label(view.nextLocation, pack.locations.First(l => l.id == next.location).name + "로 이동 →");
            RefreshInvestigation();
        }
        private void BeginNode(InvestigationNode node)
        {
            if (!story.IsAvailable(node)) return;
            activeNode = node;
            if (!string.IsNullOrEmpty(node.inspection))
            {
                ShowInspection(node, false, () => BeginNodeDialogue(node));
                return;
            }
            BeginNodeDialogue(node);
        }
        private void BeginNodeDialogue(InvestigationNode node)
        {
            ShowDialogue(node.before, () =>
            {
                if (node.kind == "stage") { attempt = 0; StartPuzzle(storage.Pack.stages.First(s => s.id == node.contentId), false, false); }
                else if (node.kind == "riddle") StartRiddle(storage.Pack.riddles.First(r => r.id == node.contentId));
                else FinishNode();
            });
        }
        private void FinishNode()
        {
            if (!string.IsNullOrEmpty(activeNode.discovery))
            {
                ShowInspection(activeNode, true, FinishNodeDialogue);
                return;
            }
            FinishNodeDialogue();
        }
        private void FinishNodeDialogue()
        {
            ShowDialogue(activeNode.after, () =>
            {
                var completed = activeNode;
                if (!story.Complete(completed.id)) throw new InvalidOperationException("이미 완료했거나 아직 시작할 수 없는 조사입니다.");
                storage.Save(story.Progress);
                if (story.Progress.ending) ShowEnding();
                else
                {
                    ShowExplore();
                    if (completed.rewards.Length > 0)
                        Notice("새 단서를 기록했습니다", string.Join("\n\n", completed.rewards.Select(id => { var c = storage.Pack.clues.First(x => x.id == id); return c.title + "\n" + c.description; })), ShowExplore);
                }
            });
        }
        private void ShowDialogue(DialogueLine[] dialogue, Action done)
        {
            if (dialogue.Length == 0) { done(); return; }
            // Conversations always take place in the room, including after a puzzle.
            ShowExplore();
            lines = dialogue; dialogueIndex = 0; afterDialogue = done;
            view.resultPanel.SetActive(false); view.dialoguePanel.SetActive(true);
            view.explorationHud.alpha = 0; view.explorationHud.blocksRaycasts = false;
            var partner = dialogue.FirstOrDefault(l => l.speaker != "you")?.speaker;
            view.stagePresentation.Open(Portrait("you"), partner == null ? null : Portrait(partner));
            RenderDialogue();
        }
        private Sprite Portrait(string id, string pose = null)
        {
            var character = storage.Pack.characters.FirstOrDefault(c => c.id == id);
            if (character == null) return null;
            int index = Array.IndexOf(storage.Pack.characters, character);
            return (!string.IsNullOrEmpty(pose) ? storage.Image("character." + id + "." + pose) : null) ?? storage.Image(character.portrait ?? "character." + character.id) ?? Presentation.Pose(id, pose) ??
                (index < defaultPortraits.Length ? defaultPortraits[index] : null);
        }
        private void RenderDialogue()
        {
            var line = lines[dialogueIndex];
            var character = storage.Pack.characters.First(c => c.id == line.speaker);
            view.speakerName.text = character.name; view.speakerRole.text = character.role;
            view.dialogueCounter.text = "클릭 / Space  ·  글자 완성, 한 번 더 누르면 다음";
            view.stagePresentation.Speak(line.speaker == "you", Portrait(line.speaker, line.pose), line.text, smoke);
        }
        private void NextDialogue()
        {
            if (!view.dialoguePanel.activeSelf) return;
            if (view.stagePresentation.IsClosing) return;
            if (view.stagePresentation.IsRevealing) { view.stagePresentation.CompleteText(); return; }
            if (++dialogueIndex < lines.Length) RenderDialogue();
            else
            {
                busy = true;
                view.stagePresentation.Close(() =>
                {
                    busy = false; view.dialoguePanel.SetActive(false);
                    view.explorationHud.alpha = 1; view.explorationHud.blocksRaycasts = true;
                    var callback = afterDialogue; afterDialogue = null; Guard(() => callback?.Invoke());
                }, smoke);
            }
        }
        private void StartPuzzle(HotelStage data, bool isPractice, bool retry)
        {
            stage = data; practice = isPractice; if (retry) attempt++;
            puzzle = new HotelPuzzle(stage, attempt); screen = "puzzle"; busy = false;
            view.Show(view.puzzleScreen);
            view.puzzleTitle.text = stage.title;
            view.puzzlePurpose.text = practice ? "연습 · 이야기 진행에는 영향을 주지 않습니다" : activeNode.purpose;
            SetupPuzzlePresentation();
            RenderBoard();
        }
        private static string ColorName(BlockColor color) => new[] { "산호", "파랑", "초록", "노랑", "보라" }[(int)color];
        private void RenderBoard(bool updateHud = true)
        {
            var board = puzzle.Board;
            view.boardLayout.constraintCount = board.Width;
            var area = ((RectTransform)view.boardLayout.transform).rect;
            float size = Mathf.Min((area.width - 16 - (board.Width - 1) * view.boardLayout.spacing.x) / board.Width, (area.height - 16 - (board.Height - 1) * view.boardLayout.spacing.y) / board.Height);
            view.boardLayout.cellSize = new Vector2(size, size);
            for (int i = 0; i < view.cells.Length; i++)
            {
                var cell = view.cells[i]; bool visible = i < board.Width * board.Height;
                cell.gameObject.SetActive(visible); if (!visible) continue;
                DrawCell(i);
            }
            if (updateHud) UpdatePuzzleHud();
        }
        private void ClickCell(int index)
        {
            if (screen != "puzzle" || busy || view.resultPanel.activeSelf || view.dialoguePanel.activeSelf) return;
            var p = new GridPosition(index % stage.width, stage.height - 1 - index / stage.width);
            int shuffles = puzzle.Reshuffles;
            var result = puzzle.Play(p);
            if (!result.WasResolved) { toast = "같은 색이 두 개 이상 연결된 곳을 찾아보세요."; view.comboText.text = toast; return; }
            busy = true;
            turnReshuffled = puzzle.Reshuffles > shuffles;
            toast = puzzle.Reshuffles > shuffles ? "움직일 곳이 없어 블록을 다시 배치했어요. 횟수는 추가로 쓰지 않습니다." : result.RemovedCount + "개 정리" + (result.HasCreatedSpecialBlock ? " · 특수 블록 생성!" : "");
            StartCoroutine(AnimateTurn(result));
        }
        private IEnumerator AnimateTurn(PuzzleTurnResult result)
        {
            yield return PresentTurn(result);
            busy = false; RenderBoard();
            if (puzzle.Cleared)
            {
                Result("조사 완료", stage.title, practice ? "목표를 모두 달성했어요." : activeNode?.inspection == "cabinet" ? "빈 열쇠 고리에 작은 흔적이 남아 있어요." : "주변이 정리됐어요. 새 흔적을 확인해 보세요.", practice ? "연습 목록" : "흔적 확인", () => { if (practice) ShowCatalog("practice"); else FinishNode(); }, "다시 플레이", () => StartPuzzle(stage, practice, true));
            }
            else if (puzzle.Failed) Result("다시 시도", "흔적이 조금 남았어요", string.Join("\n", puzzle.Objectives.Objectives.Select(g => (g is RemoveColorObjective c ? ColorName(c.TargetColor) : g is RemoveBoxObjective ? "상자" : "얼음") + (g.IsCompleted ? " · 완료" : " · " + (g.TargetAmount - g.CurrentAmount) + "개 남음"))), "재도전", () => StartPuzzle(stage, practice, true), "돌아가기", ReturnFromPuzzle);
        }
        private void LeavePuzzle() => Confirm("조사를 잠시 멈출까요?", "이 보드의 진행은 저장되지 않습니다. 완료한 조사와 단서는 이미 저장되어 있습니다.", ReturnFromPuzzle, () => view.resultPanel.SetActive(false));
        private void ReturnFromPuzzle() { if (practice) ShowCatalog("practice"); else ShowExplore(); }
        private void PuzzleHint()
        {
            var move = puzzle.FindMove();
            if (!move.HasValue) return;
            int index = (stage.height - 1 - move.Value.Y) * stage.width + move.Value.X;
            HoverCell(index, true);
            view.comboText.text = "빛나는 묶음을 눌러 보세요 · 횟수 소모 없음";
        }
        private void StartRiddle(RiddleData data)
        {
            screen = "riddle"; riddle = data; hints = 0; selection.Clear();
            view.Show(view.riddleScreen); view.feedbackText.text = "틀려도 불이익은 없습니다. 단서를 떠올려 보세요."; RenderRiddle();
        }
        private void RenderRiddle()
        {
            view.riddleTitle.text = riddle.title; view.question.text = riddle.question;
            for (int i = 0; i < view.options.Length; i++)
            {
                view.options[i].gameObject.SetActive(i < riddle.options.Length); if (i >= riddle.options.Length) continue;
                int selected = selection.IndexOf(i);
                HotelView.Label(view.options[i], (selected >= 0 ? "[" + (selected + 1) + "] " : "○  ") + riddle.options[i]);
            }
            view.selectionText.text = riddle.kind == "choice" ? "맞는 설명 하나를 선택하세요." : "먼저 일어난 일부터 선택하세요. 다시 누르면 선택이 취소됩니다.\n" + string.Join(" → ", selection.Select(i => riddle.options[i]));
            view.submitAnswer.interactable = selection.Count == riddle.answer.Length;
        }
        private void SelectOption(int index)
        {
            if (riddle.kind == "choice") { selection.Clear(); selection.Add(index); }
            else if (!selection.Remove(index)) selection.Add(index);
            RenderRiddle();
        }
        private void RiddleHint()
        {
            if (riddle.hints.Length == 0) { view.feedbackText.text = "문제의 조건을 하나씩 확인해 보세요."; return; }
            hints = Math.Min(hints + 1, riddle.hints.Length);
            view.feedbackText.text = "힌트 " + hints + " / " + riddle.hints.Length + "\n" + riddle.hints[hints - 1];
        }
        private void SubmitRiddle()
        {
            if (!InvestigationSession.CheckAnswer(riddle, selection)) { view.feedbackText.text = "아직 모든 단서와 맞지는 않아요. 조건을 다시 확인하거나 힌트를 펼쳐 보세요."; return; }
            Result("수수께끼 해결", "기록이 이어졌습니다", riddle.explanation, "조사 계속", FinishNode, null, null);
        }
        private void ShowNotebook(bool fromEnding)
        {
            screen = "notebook"; notebookFromEnding = fromEnding; view.Show(view.notebookScreen);
            var entries = NotebookEntries();
            view.clueCount.text = entries.Count + "개의 조사 기록";
            view.clueTitle.text = "조사 수첩"; view.clueDescription.text = "획득한 단서를 선택하면 내용을 다시 확인할 수 있습니다.";
            Presentation.notebookImage.gameObject.SetActive(false);
            for (int i = 0; i < view.clueButtons.Length; i++)
            {
                var button = view.clueButtons[i]; bool exists = i < entries.Count; button.gameObject.SetActive(exists); if (!exists) continue;
                var clue = entries[i];
                HotelView.Label(button, clue.title); button.interactable = true;
                Bind(button, () => SelectNotebookEntry(clue));
            }
            if (entries.Count > 0) SelectNotebookEntry(entries[0]);
        }
        private void ShowCatalog(string mode) { screen = "catalog"; catalogMode = mode; catalogPage = 0; view.Show(view.catalogScreen); RenderCatalog(); }
        private void RenderCatalog()
        {
            bool mods = catalogMode == "mods";
            int count = mods ? storage.Mods.Count + 1 : storage.Pack.stages.Length;
            int pageCount = Math.Max(1, (count + view.catalogRows.Length - 1) / view.catalogRows.Length);
            catalogPage = Mathf.Clamp(catalogPage, 0, pageCount - 1);
            view.catalogTitle.text = mods ? "MOD 관리" : "퍼즐 연습";
            view.catalogDescription.text = mods ? "한 번에 하나의 MOD를 적용합니다. 저장은 MOD별로 분리됩니다.\n현재: " + storage.PackName + "\n" + storage.ModsPath : "기본 스테이지와 MOD 스테이지를 자유롭게 연습하세요.\n이야기의 단서와 진행에는 영향을 주지 않습니다.";
            view.catalogPage.text = (catalogPage + 1) + " / " + pageCount;
            view.catalogPrevious.interactable = catalogPage > 0; view.catalogNext.interactable = catalogPage + 1 < pageCount;
            view.catalogFolder.gameObject.SetActive(mods); view.catalogRefresh.gameObject.SetActive(mods); view.catalogSample.gameObject.SetActive(mods);
            for (int i = 0; i < view.catalogRows.Length; i++)
            {
                int index = catalogPage * view.catalogRows.Length + i;
                var button = view.catalogRows[i]; button.gameObject.SetActive(index < count); if (index >= count) continue;
                button.interactable = true;
                if (!mods)
                {
                    var item = storage.Pack.stages[index]; HotelView.Label(button, item.title + "    ·    " + item.moves + "회");
                    Bind(button, () => { attempt = 0; StartPuzzle(item, true, false); });
                }
                else if (index == 0) { HotelView.Label(button, (storage.PackId == "base" ? "● " : "○ ") + "기본 이야기"); Bind(button, () => ApplyMod("base")); }
                else
                {
                    var item = storage.Mods[index - 1];
                    HotelView.Label(button, (item.Valid ? storage.PackId == item.manifest.id ? "● " : "○ " : "! ") + (item.manifest?.name ?? Path.GetFileName(item.directory)) + "\n" + (item.Valid ? item.manifest.description : item.error));
                    Bind(button, () => { if (!item.Valid) Notice("MOD를 확인해 주세요", item.error, () => ShowCatalog("mods")); else ApplyMod(item.manifest.id); });
                }
            }
        }
        private void ApplyMod(string id)
        {
            if (!storage.TryActivate(id, out var error)) throw new InvalidOperationException(error);
            storage.SavePreferences(); story = null; RenderCatalog();
        }
        private void ShowSettings()
        {
            screen = "settings"; view.Show(view.settingsScreen);
            HotelView.Label(view.soundButton, "효과음: " + (storage.Preferences.sound ? "켜짐" : "꺼짐"));
            view.settingsInfo.text = "마우스 왼쪽 클릭: 선택 / 조사\nEnter 또는 Space: 대화 계속\nEsc: 대화·팝업이 없을 때 돌아가기\n\n퍼즐은 완료 지점에서 저장됩니다.\n\n저장 위치\n" + storage.Root;
        }
        private void ShowEnding()
        {
            screen = "ending"; view.Show(view.endingScreen);
            view.endingSummary.text = "열쇠는 수건을 따라 이동했고, 전화벨은 오래된 설비의 신호였습니다.\n두 사건이 가리킨 곳에는 잊힌 서재와 아직 유효한 약속이 있었습니다.\n\n서윤의 첫 번째 조사 기록 — 완료\n" + story.Progress.clues.Count + "개의 단서 · " + storage.Pack.stages.Length + "개의 기본/추가 스테이지\n\n다음 손님이 남길 이야기를 기다립니다.";
        }
        private void Result(string eyebrow, string title, string body, string primary, Action action, string secondary, Action other)
        {
            view.dialoguePanel.SetActive(false); view.resultPanel.SetActive(true);
            view.resultEyebrow.text = eyebrow; view.resultTitle.text = title; view.resultBody.text = body;
            HotelView.Label(view.resultPrimary, primary); Bind(view.resultPrimary, () => { view.resultPanel.SetActive(false); action?.Invoke(); });
            view.resultSecondary.gameObject.SetActive(secondary != null);
            if (secondary != null) { HotelView.Label(view.resultSecondary, secondary); Bind(view.resultSecondary, () => { view.resultPanel.SetActive(false); other?.Invoke(); }); }
        }
        private void Notice(string title, string body, Action done) => Result("잔월 호텔", title, body, "확인", done, null, null);
        private void Confirm(string title, string body, Action yes, Action no) => Result("진행 확인", title, body, "계속", yes, "취소", no);
        private void Update()
        {
            if (Keyboard.current == null || busy) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame && Presentation.helpPanel.activeSelf) { Presentation.helpPanel.SetActive(false); return; }
            if (screen == "inspection")
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame && Presentation.inspectionBack.gameObject.activeSelf) Guard(ShowExplore);
                return;
            }
            if (view.dialoguePanel.activeSelf && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)) Guard(NextDialogue);
            if (!view.resultPanel.activeSelf && !view.dialoguePanel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
                Guard(() => { if (screen == "puzzle") LeavePuzzle(); else if (screen == "riddle") ShowExplore(); else if (screen == "notebook") { if (notebookFromEnding) ShowEnding(); else ShowExplore(); } else ShowTitle(); });
        }
        private void MakeSound()
        {
            clickClip = AudioClip.Create("Hotel paper chime", 2205, 1, 22050, false);
            float[] samples = new float[2205];
            for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 660 / 22050) * .06f * (1f - i / (float)samples.Length);
            clickClip.SetData(samples, 0);
        }
        private void Sound() { if (storage != null && storage.Preferences.sound && clickClip != null && !smoke) audioSource.PlayOneShot(clickClip); }
        private void OnDestroy() { storage?.Dispose(); if (clickClip != null) Destroy(clickClip); }

        private void PressForSmoke(Button button)
        {
            HotelCapture.Save(view, null);
            ContentValidation.Require(button.gameObject.activeInHierarchy && button.IsInteractable(), "QA button is not interactable: " + button.name);
            var rect = (RectTransform)button.transform;
            var canvas = button.GetComponentInParent<Canvas>();
            var point = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, rect.TransformPoint(rect.rect.center));
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, results);
            ContentValidation.Require(results.Count > 0 && (results[0].gameObject == button.gameObject || results[0].gameObject.transform.IsChildOf(button.transform)),
                "QA button is occluded: " + button.name + " at " + point + " hits " + string.Join(",", results.Select(r => r.gameObject.name + ":" + r.depth)));
            button.onClick.Invoke();
        }

        private IEnumerator SmokeTest()
        {
            // Player-only integration test uses actual serialized controls and an isolated save directory.
            string output = Path.Combine(Application.dataPath, "..", "Smoke"); Directory.CreateDirectory(output);
            HotelCapture.PrepareHiddenPlayer(view);
            yield return null; yield return new WaitForEndOfFrame();
            HotelCapture.Save(view, Path.Combine(output, "01-title.png"));
            PressForSmoke(view.newButton);
            yield return null; yield return new WaitForEndOfFrame();
            HotelCapture.Save(view, Path.Combine(output, "02-explore.png"));
            bool capturedBoard = false, capturedRiddle = false, capturedDialogue = false;
            int safety = 0;
            double deadline = Time.realtimeSinceStartupAsDouble + 120;
            while (story != null && !story.Progress.ending && safety < 3000 && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null;
                if (busy) continue;
                safety++;
                if (view.dialoguePanel.activeSelf)
                {
                    if (!capturedDialogue) { yield return new WaitForSecondsRealtime(.4f); yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "03-dialogue.png")); capturedDialogue = true; }
                    PressForSmoke(view.dialogueNext); continue;
                }
                if (view.resultPanel.activeSelf) { yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "09-result.png")); PressForSmoke(view.resultPrimary); continue; }
                if (screen == "inspection") { yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, Presentation.discoveryCard.activeSelf ? "11-discovery.png" : "10-inspection.png")); PressForSmoke(Presentation.inspectionContinue); continue; }
                if (screen == "explore")
                {
                    var next = story.Next;
                    if (next == null) break;
                    if (story.Progress.location != next.location) MoveLocation(next.location);
                    else PressForSmoke(view.hotspot);
                }
                else if (screen == "puzzle")
                {
                    if (!capturedBoard) { yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "04-puzzle.png")); capturedBoard = true; }
                    var best = BestSmokeMove();
                    if (best.HasValue) PressForSmoke(view.cells[(stage.height - 1 - best.Value.Y) * stage.width + best.Value.X].button);
                }
                else if (screen == "riddle")
                {
                    if (!capturedRiddle) { yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "05-riddle.png")); capturedRiddle = true; }
                    selection.Clear(); foreach (int answer in riddle.answer) PressForSmoke(view.options[answer]); PressForSmoke(view.submitAnswer);
                }
            }
            try
            {
                ContentValidation.Require(story != null && story.Progress.ending, "Smoke did not reach ending: " + screen + "/" + activeNode?.id + "/" + attempt);
                storage.Load(out _);
                Debug.Log("HOTEL_SMOKE_CAMPAIGN_PASSED: " + story.Progress.completed.Count);
            }
            catch (Exception e) { Debug.LogException(e); Application.Quit(2); yield break; }
            yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "06-ending.png"));
            ShowNotebook(true); yield return null; yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "07-notebook.png"));
            bool modOk = true;
            try
            {
                storage.InstallExample(Path.Combine(Application.streamingAssetsPath, "Hotel", "ExampleMod"));
                ContentValidation.Require(storage.TryActivate("moonlight.example", out var error), error);
                ContentValidation.Require(storage.Image("block.0") != null && storage.Pack.stages.Any(s => s.id == "moonlight_bonus"), "MOD asset/stage did not load");
                ContentValidation.Require(!storage.HasSave, "MOD save isolation failed");
                storage.Save(storage.NewProgress()); storage.Load(out _); storage.SavePreferences(); ShowCatalog("mods");
            }
            catch (Exception e) { Debug.LogException(e); modOk = false; }
            yield return null; yield return new WaitForEndOfFrame(); HotelCapture.Save(view, Path.Combine(output, "08-mods.png"));
            yield return null; yield return null;
            File.WriteAllText(Path.Combine(output, "result.txt"), modOk ? "PASS: campaign, 12 puzzles, 4 riddles, save/load, MOD image/stage, isolated progress." : "FAIL: MOD");
            Debug.Log(modOk ? "HOTEL_SMOKE_PASSED" : "HOTEL_SMOKE_FAILED"); Application.Quit(modOk ? 0 : 2);
        }
        private IEnumerator PresentationPreview()
        {
            string output = Path.Combine(Application.dataPath, "..", "PresentationPreview");
            Directory.CreateDirectory(output); HotelCapture.PrepareHiddenPlayer(view);
            storage.Preferences.sound = false;
            NewGame();
            var timings = new List<string>(); double previous = 0;
            for (int frame=0;frame<180;frame++)
            {
                yield return new WaitForSecondsRealtime(1f/12);
                if (frame == 18) PressForSmoke(view.hotspot);
                if (frame == 19)
                {
                    int before = dialogueIndex;
                    ContentValidation.Require(view.stagePresentation.IsRevealing, "QA typewriter never started");
                    PressForSmoke(view.dialogueNext);
                    ContentValidation.Require(dialogueIndex == before && !view.stagePresentation.IsRevealing, "First click must reveal without advancing");
                }
                if (frame == 50 || frame == 88 || frame == 124 || frame == 158)
                {
                    if (view.dialoguePanel.activeSelf && !busy) PressForSmoke(view.dialogueNext);
                }
                yield return new WaitForEndOfFrame();
                double now=Time.realtimeSinceStartupAsDouble;
                if(frame>0)timings.Add("duration "+(now-previous).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                string name="frame-"+frame.ToString("D4")+".png";
                timings.Add("file '"+name+"'");previous=now;
                HotelCapture.Save(view,Path.Combine(output,name));
            }
            timings.Add("duration 0.1");timings.Add("file 'frame-0179.png'");
            File.WriteAllLines(Path.Combine(output,"frames.txt"),timings);
            Debug.Log("HOTEL_PRESENTATION_PREVIEW_PASSED: reveal-first input, authored controls, real-time transitions");
            Application.Quit(0);
        }
        private GridPosition? BestSmokeMove()
        {
            var finder = new ConnectedBlockFinder(); GridPosition? best = null; int score = -1;
            for (int y = 0; y < puzzle.Board.Height; y++) for (int x = 0; x < puzzle.Board.Width; x++)
            {
                var p = new GridPosition(x, y); if (!puzzle.Board.IsPlayable(p)) continue;
                var block = puzzle.Board.GetBlock(p); if (block == null) continue;
                var group = finder.Find(puzzle.Board, p);
                if (block.Type == BlockType.Normal && group.Count < 2) continue;
                int value = block.Type != BlockType.Normal ? 80 : group.Count >= 2 ? group.Count * 2 + ((int)block.Color == stage.targetColor ? group.Count * 3 : 0) : -1;
                foreach (var q in group)
                {
                    if (puzzle.Board.GetObstacle(q, ObstacleLayer.Overlay) != null) value += 12;
                    foreach (var d in new[] { new GridPosition(1,0), new GridPosition(-1,0), new GridPosition(0,1), new GridPosition(0,-1) })
                    { var a = new GridPosition(q.X+d.X,q.Y+d.Y); if (puzzle.Board.IsPlayable(a) && puzzle.Board.GetObstacle(a, ObstacleLayer.Blocking) != null) value += 10; }
                }
                if (value > score) { score = value; best = p; }
            }
            return best;
        }
    }
}
