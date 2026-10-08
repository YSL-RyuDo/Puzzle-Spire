using System;
using System.Linq;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelStoryStudio
    {
        private void Add()
        {
            var p = Project.pack;
            if (tab == 0) { NewProject(false); return; }
            if (tab == 1)
            {
                ContentValidation.Require(p.locations.Length < 30, "장소는 최대 30곳입니다.");
                string id = HotelStoryProjects.NewId("room"); p.locations = Append(p.locations, new LocationData { id = id, name = "새 장소", subtitle = "장소 설명", image = "background." + id });
            }
            if (tab == 2)
            {
                string id = HotelStoryProjects.NewId("person"); p.characters = Append(p.characters, new CharacterData { id = id, name = "새 인물", role = "인물 소개", portrait = "character." + id, color = "#A77C56" });
            }
            if (tab == 3)
            {
                ContentValidation.Require(p.clues.Length < 200, "단서는 최대 200개입니다.");
                string id = HotelStoryProjects.NewId("clue"); p.clues = Append(p.clues, new ClueData { id = id, title = "새 단서", description = "단서에 대한 설명", image = id });
            }
            if (tab == 4)
            {
                ContentValidation.Require(p.nodes.Length < 200, "조사는 최대 200개입니다.");
                var list = p.nodes.ToList(); int at = list.FindIndex(n => n.kind == "ending"); if (at < 0) at = list.Count;
                string id = HotelStoryProjects.NewId("event");
                var n = new InvestigationNode { id = id, title = "새 조사", chapter = Node?.chapter ?? "CHAPTER 1", location = Node?.location ?? p.locations[0].id, kind = "dialogue", purpose = "주변을 살펴보자.", x = .5f, y = .5f, requires = at == 0 ? Array.Empty<string>() : new[] { list[at - 1].id } };
                if (at < list.Count) list[at].requires = list[at].requires.Concat(new[] { id }).Distinct().ToArray();
                list.Insert(at, n); p.nodes = list.ToArray(); nodeId = id; selected = at; page = selected / view.rows.Length; Save(); Render(); return;
            }
            if (tab == 5) SetLines(Append(Lines, new DialogueLine { speaker = "you", pose = "", text = "새 대사를 입력하세요." }));
            if (tab == 6) p.riddles = Append(p.riddles, NewRiddle());
            if (tab == 7)
            {
                string actor = Node?.before.FirstOrDefault()?.speaker ?? "you";
                selectedKey = "character." + actor + "." + HotelStoryProjects.NewId("pose");
                Project.images = Append(Project.images, new ImageOverride { key = selectedKey, file = "" });
            }
            selected = Math.Max(0, Rows().Length - 1); page = selected / view.rows.Length; Save(); Render();
        }
        private RiddleData NewRiddle() => new RiddleData { id = HotelStoryProjects.NewId("riddle"), title = "새 수수께끼", kind = "choice", question = "어느 설명이 맞을까요?", options = new[] { "첫 번째 설명", "두 번째 설명" }, answer = new[] { 0 }, explanation = "정답의 이유를 적으세요.", hints = new[] { "단서를 다시 살펴보세요." } };
        private void EnsureContent(InvestigationNode n)
        {
            if (n.kind == "stage" && !Project.pack.stages.Any(s => s.id == n.contentId))
            {
                var stage = new HotelStage { id = HotelStoryProjects.NewId("puzzle"), title = n.title + " 퍼즐", width = 5, height = 5, moves = 20, colors = 3, targetColor = 0, targetAmount = 10, fixedSeed = true, layout = Enumerable.Repeat("OOOOO", 5).ToArray() };
                Project.pack.stages = Append(Project.pack.stages, stage); n.contentId = stage.id;
            }
            else if (n.kind == "riddle" && !Project.pack.riddles.Any(r => r.id == n.contentId))
            {
                var riddle = NewRiddle(); Project.pack.riddles = Append(Project.pack.riddles, riddle); n.contentId = riddle.id;
            }
            else if (n.kind == "dialogue" || n.kind == "ending") n.contentId = "";
        }
        private void Delete()
        {
            Commit(); string token = tab + ":" + selected;
            if (deleteArm != token) { deleteArm = token; view.status.text = "삭제하려면 삭제 버튼을 한 번 더 누르세요. 다른 항목을 선택하면 취소됩니다."; return; }
            var p = Project.pack;
            if (tab == 1)
            {
                var room = p.locations[selected]; ContentValidation.Require(!p.nodes.Any(n => n.location == room.id), "이 장소를 사용하는 조사의 장소를 먼저 변경하세요.");
                ContentValidation.Require(p.locations.Length > 1, "장소는 하나 이상 필요합니다."); p.locations = p.locations.Where(l => l != room).ToArray();
            }
            if (tab == 2)
            {
                var actor = p.characters[selected]; ContentValidation.Require(actor.id != "you" && !p.nodes.SelectMany(n => n.before.Concat(n.after)).Any(l => l.speaker == actor.id), "주인공이거나 대사에서 사용 중인 인물입니다."); p.characters = p.characters.Where(c => c != actor).ToArray();
            }
            if (tab == 3)
            {
                var clue = p.clues[selected]; p.clues = p.clues.Where(c => c != clue).ToArray(); foreach (var n in p.nodes) n.rewards = n.rewards.Where(id => id != clue.id).ToArray();
            }
            if (tab == 4)
            {
                var node = p.nodes[selected]; p.nodes = p.nodes.Where(n => n != node).ToArray();
                foreach (var n in p.nodes) if (n.requires.Contains(node.id)) n.requires = n.requires.Where(id => id != node.id).Concat(node.requires).Distinct().Where(id => id != n.id).ToArray();
                foreach (var room in p.locations) if (room.unlockAfter == node.id) room.unlockAfter = "";
                selected = Math.Min(selected, Math.Max(0, p.nodes.Length - 1)); nodeId = p.nodes.ElementAtOrDefault(selected)?.id;
            }
            if (tab == 5) SetLines(Lines.Where((l, i) => i != selected).ToArray());
            if (tab == 6)
            {
                var riddle = p.riddles[selected]; ContentValidation.Require(!p.nodes.Any(n => n.kind == "riddle" && n.contentId == riddle.id), "조사에 연결된 수수께끼입니다. 조사 종류나 연결을 먼저 변경하세요."); p.riddles = p.riddles.Where(r => r != riddle).ToArray();
            }
            if (tab == 7) Project.images = Project.images.Where(i => i.key != selectedKey).ToArray();
            deleteArm = null; Save(); Render();
        }
        private void Move(int delta)
        {
            Commit(); int other = selected + delta;
            if (tab == 4 && other >= 0 && other < Project.pack.nodes.Length)
            {
                var a = Project.pack.nodes[selected]; Project.pack.nodes[selected] = Project.pack.nodes[other]; Project.pack.nodes[other] = a; selected = other; nodeId = a.id;
            }
            if (tab == 5 && other >= 0 && other < Lines.Length)
            {
                var lines = Lines; var a = lines[selected]; lines[selected] = lines[other]; lines[other] = a; SetLines(lines); selected = other;
            }
            page = selected / view.rows.Length; Save(); Render();
        }
        private void GoImage(string key)
        {
            tab = 7; selectedKey = key; selected = Math.Max(0, Array.IndexOf(ImageKeys(), key)); page = selected / view.rows.Length; Render();
        }
        private void Special()
        {
            Commit();
            if (tab == 1) { GoImage(Project.pack.locations[selected].image); return; }
            if (tab == 2) { GoImage("character." + Project.pack.characters[selected].id); return; }
            if (tab == 3) { GoImage("clue." + Project.pack.clues[selected].id); return; }
            if (tab != 4 || Node == null) return;
            if (Node.kind == "stage")
            {
                var stage = Project.pack.stages.First(s => s.id == Node.contentId);
                editPuzzle(stage, Project.proofs.FirstOrDefault(p => p.stageId == stage.id), ProjectFolder, draft => {
                    int index = Array.FindIndex(Project.pack.stages, s => s.id == stage.id); Project.pack.stages[index] = HotelPuzzleClearance.Clone(draft.stage);
                    Project.proofs = Project.proofs.Where(p => p.stageId != stage.id).Concat(draft.clearance == null ? Array.Empty<PuzzleClearance>() : new[] { HotelStoryProjects.Copy(draft.clearance) }).ToArray(); Save();
                });
            }
            else if (Node.kind == "riddle") { selected = Array.FindIndex(Project.pack.riddles, r => r.id == Node.contentId); tab = 6; page = selected / view.rows.Length; Render(); }
            else { tab = 5; selected = page = 0; Render(); }
        }
    }
}
