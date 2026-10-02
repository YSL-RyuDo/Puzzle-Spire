using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PuzzleSpire.Hotel
{
    [Serializable] public sealed class HotelPack
    {
        public int schemaVersion = 1;
        public string title;
        public LocationData[] locations = Array.Empty<LocationData>();
        public CharacterData[] characters = Array.Empty<CharacterData>();
        public ClueData[] clues = Array.Empty<ClueData>();
        public InvestigationNode[] nodes = Array.Empty<InvestigationNode>();
        public HotelStage[] stages = Array.Empty<HotelStage>();
        public RiddleData[] riddles = Array.Empty<RiddleData>();
    }
    [Serializable] public sealed class LocationData
    {
        public string id, name, subtitle, unlockAfter, image;
    }
    [Serializable] public sealed class CharacterData
    {
        public string id, name, role, color, portrait;
    }
    [Serializable] public sealed class ClueData
    {
        public string id, title, description, image;
    }
    [Serializable] public sealed class DialogueLine
    {
        public string speaker, text, pose;
    }
    [Serializable] public sealed class InvestigationNode
    {
        public string id, chapter, location, title, purpose, kind, contentId;
        public float x, y;
        public string[] requires = Array.Empty<string>();
        public string[] rewards = Array.Empty<string>();
        public DialogueLine[] before = Array.Empty<DialogueLine>();
        public DialogueLine[] after = Array.Empty<DialogueLine>();
        // Optional visual observations are derived from completed nodes, preserving existing saves.
        public string inspection, discovery, discoveryTitle, discoveryText;
    }
    [Serializable] public sealed class HotelStage
    {
        public string id, title;
        public int width = 7, height = 7, moves = 22, seed = 1001, colors = 4;
        public int targetColor, targetAmount = 12, boxes, ice;
        public string[] layout = Array.Empty<string>();
        public ObstacleData[] obstacles = Array.Empty<ObstacleData>();
    }
    [Serializable] public sealed class ObstacleData
    {
        public int x, y, durability = 1;
        public string type;
    }
    [Serializable] public sealed class RiddleData
    {
        public string id, title, kind, question, explanation;
        public string[] options = Array.Empty<string>();
        public int[] answer = Array.Empty<int>();
        public string[] hints = Array.Empty<string>();
    }
    [Serializable] public sealed class ModManifest
    {
        public int schemaVersion = 1;
        public string id, name, version, author, description, contentFile;
        public ImageOverride[] images = Array.Empty<ImageOverride>();
    }
    [Serializable] public sealed class ImageOverride
    {
        public string key, file;
    }

    public static class ContentValidation
    {
        public static bool IsId(string id) => id != null && Regex.IsMatch(id, "^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,79}$");
        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
        private static HashSet<string> Ids<T>(T[] items, Func<T, string> id, string type)
        {
            Require(items != null, type + " 목록이 없습니다.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                Require(item != null, type + "에 빈 항목이 있습니다.");
                var key = id(item);
                Require(IsId(key) && ids.Add(key), type + " ID가 잘못되었거나 중복됩니다: " + key);
            }
            return ids;
        }
        public static void Validate(HotelPack pack)
        {
            Require(pack != null && pack.schemaVersion == 1, "지원하지 않는 콘텐츠 버전입니다.");
            var locations = Ids(pack.locations, x => x.id, "장소");
            var characters = Ids(pack.characters, x => x.id, "인물");
            var clues = Ids(pack.clues, x => x.id, "단서");
            var nodes = Ids(pack.nodes, x => x.id, "조사");
            var stages = Ids(pack.stages, x => x.id, "스테이지");
            var riddles = Ids(pack.riddles, x => x.id, "수수께끼");
            Require(locations.Count > 0 && nodes.Count > 0, "장소와 조사가 필요합니다.");
            Require(locations.Count <= 3 && clues.Count <= 12, "이 UI 버전은 장소 3곳, 단서 12개까지 지원합니다.");
            Require(clues.Count + pack.nodes.Count(n => !string.IsNullOrEmpty(n.discovery)) <= 12, "단서와 관찰 기록은 합쳐서 12개까지 지원합니다.");
            Require(nodes.Count <= 200 && stages.Count <= 200 && riddles.Count <= 100, "콘텐츠 개수 제한을 초과했습니다.");
            foreach (var l in pack.locations)
                Require(string.IsNullOrEmpty(l.unlockAfter) || nodes.Contains(l.unlockAfter), "장소 해금 조건을 찾을 수 없습니다: " + l.id);
            foreach (var n in pack.nodes)
            {
                Require(locations.Contains(n.location), "조사 장소를 찾을 수 없습니다: " + n.id);
                Require(n.requires != null && n.rewards != null && n.before != null && n.after != null, "조사 목록 필드가 비었습니다: " + n.id);
                Require(n.requires.All(nodes.Contains) && n.rewards.All(clues.Contains), "조사 조건/단서 ID 오류: " + n.id);
                Require(string.IsNullOrEmpty(n.discovery) || (!string.IsNullOrWhiteSpace(n.discoveryTitle) && !string.IsNullOrWhiteSpace(n.discoveryText)), "발견 장면의 제목과 관찰 내용이 필요합니다: " + n.id);
                Require(n.x >= 0 && n.x <= 1 && n.y >= 0 && n.y <= 1, "조사 좌표 범위 오류: " + n.id);
                Require(n.kind == "dialogue" || n.kind == "ending" || (n.kind == "stage" && stages.Contains(n.contentId)) || (n.kind == "riddle" && riddles.Contains(n.contentId)), "조사 종류/콘텐츠 오류: " + n.id);
                foreach (var line in n.before.Concat(n.after))
                    Require(line != null && characters.Contains(line.speaker) && !string.IsNullOrWhiteSpace(line.text), "대사/화자 오류: " + n.id);
            }
            // Include location gates in reachability, so a room cannot depend on an event inside itself.
            var reached = new HashSet<string>();
            bool changed;
            do
            {
                changed = false;
                foreach (var n in pack.nodes)
                {
                    var room = pack.locations.First(x => x.id == n.location);
                    if (!reached.Contains(n.id) && n.requires.All(reached.Contains) &&
                        (string.IsNullOrEmpty(room.unlockAfter) || reached.Contains(room.unlockAfter)))
                        changed |= reached.Add(n.id);
                }
            } while (changed);
            Require(reached.Count == nodes.Count, "해금 조건이 순환하거나 도달할 수 없는 조사가 있습니다.");
            Require(pack.nodes.Any(x => x.kind == "ending"), "결말 조사가 필요합니다.");
            foreach (var s in pack.stages) ValidateStage(s);
            foreach (var r in pack.riddles)
            {
                Require(r.options != null && r.options.Length >= 2 && r.options.Length <= 6 && r.options.All(x => !string.IsNullOrWhiteSpace(x)), "선택지 오류: " + r.id);
                Require(r.hints != null && r.answer != null && r.answer.Length > 0 && r.answer.All(x => x >= 0 && x < r.options.Length), "정답 오류: " + r.id);
                Require((r.kind == "choice" && r.answer.Length == 1) || (r.kind == "order" && r.answer.Length == r.options.Length && r.answer.Distinct().Count() == r.options.Length), "문제 형식 오류: " + r.id);
            }
        }
        public static void ValidateStage(HotelStage s)
        {
            Require(s.width >= 3 && s.width <= 9 && s.height >= 3 && s.height <= 9 && s.moves > 0 && s.moves <= 100 && s.colors >= 2 && s.colors <= 5, "보드 설정 범위 오류: " + s.id);
            Require(s.targetColor >= 0 && s.targetColor < s.colors && s.targetAmount >= 0 && s.boxes >= 0 && s.ice >= 0 && s.targetAmount + s.boxes + s.ice > 0, "목표 오류: " + s.id);
            Require(s.layout != null && s.layout.Length == s.height && s.layout.All(r => r != null && r.Length == s.width && r.All(c => c == 'O' || c == 'X')), "보드 배치 오류: " + s.id);
            Require(s.obstacles != null, "장애물 목록 오류: " + s.id);
            var cells = new HashSet<string>();
            foreach (var o in s.obstacles)
            {
                Require(o != null && o.x >= 0 && o.x < s.width && o.y >= 0 && o.y < s.height, "장애물 좌표 오류: " + s.id);
                Require((o.type == "box" || o.type == "ice") && o.durability >= 1 && o.durability <= 3 && s.layout[s.height - 1 - o.y][o.x] == 'O' && cells.Add(o.x + ":" + o.y), "장애물 배치 오류: " + s.id);
            }
            Require(s.boxes <= s.obstacles.Count(o => o.type == "box") && s.ice <= s.obstacles.Count(o => o.type == "ice"), "배치보다 많은 장애물 목표: " + s.id);
            bool adjacent = false;
            for (int y = 0; y < s.height; y++) for (int x = 0; x < s.width; x++)
            {
                bool Free(int px, int py) => px < s.width && py < s.height && s.layout[s.height - 1 - py][px] == 'O' && !s.obstacles.Any(o => o.x == px && o.y == py && o.type == "box");
                if (Free(x, y) && (Free(x + 1, y) || Free(x, y + 1))) adjacent = true;
            }
            Require(adjacent, "연결 가능한 블록 칸이 필요합니다: " + s.id);
        }
    }

    [Serializable] public sealed class HotelProgress
    {
        public int schemaVersion = 1;
        public string packId = "base", packVersion = "1.0.0", location = "lobby";
        public List<string> completed = new List<string>();
        public List<string> clues = new List<string>();
        public bool ending;
    }

    public sealed class InvestigationSession
    {
        public HotelPack Pack { get; }
        public HotelProgress Progress { get; }
        public InvestigationSession(HotelPack pack, HotelProgress progress)
        {
            Pack = pack; Progress = progress;
        }
        public bool IsUnlocked(LocationData room) => string.IsNullOrEmpty(room.unlockAfter) || Progress.completed.Contains(room.unlockAfter);
        public bool IsAvailable(InvestigationNode node) => !Progress.completed.Contains(node.id) && node.requires.All(Progress.completed.Contains) && IsUnlocked(Pack.locations.First(l => l.id == node.location));
        public InvestigationNode Next => Pack.nodes.FirstOrDefault(IsAvailable);
        public bool Complete(string nodeId)
        {
            var node = Pack.nodes.FirstOrDefault(n => n.id == nodeId);
            if (node == null || !IsAvailable(node)) return false;
            Progress.completed.Add(node.id);
            foreach (var id in node.rewards) if (!Progress.clues.Contains(id)) Progress.clues.Add(id);
            if (node.kind == "ending") Progress.ending = true;
            return true;
        }
        public static bool CheckAnswer(RiddleData riddle, IReadOnlyList<int> selection) => selection != null && riddle.answer.SequenceEqual(selection);
    }
}
