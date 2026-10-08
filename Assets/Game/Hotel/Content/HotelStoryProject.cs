using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    [Serializable] public sealed class HotelStoryProject
    {
        public int schemaVersion = 1;
        public string id, author = "이야기 제작자";
        public HotelPack pack;
        public ImageOverride[] images = Array.Empty<ImageOverride>();
        public PuzzleClearance[] proofs = Array.Empty<PuzzleClearance>();
    }
    public static class HotelStoryProjects
    {
        public static string NewId(string prefix) => prefix + "_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        [Serializable] private sealed class CopyBox<T> { public T value; }
        public static T Copy<T>(T value) => JsonUtility.FromJson<CopyBox<T>>(JsonUtility.ToJson(new CopyBox<T> { value = value })).value;
        public static HotelStoryProject Create(HotelPack source, bool copyStory)
        {
            var project = new HotelStoryProject { id = NewId("story"), pack = Copy(source) };
            if (copyStory)
            {
                project.pack.title += " · 내 이야기";
                foreach (var stage in project.pack.stages) stage.fixedSeed = true;
                return project;
            }
            project.pack = new HotelPack {
                title = "새로운 이야기", endingTitle = "이야기를 마치며", endingText = "우리의 첫 번째 이야기가 끝났습니다.",
                locations = new[] { new LocationData { id = "room1", name = "첫 번째 장소", subtitle = "새로운 사건이 시작되는 곳", image = "background.lobby" } },
                characters = Copy(source.characters),
                nodes = new[] {
                    new InvestigationNode { id = "intro", title = "이야기의 시작", chapter = "CHAPTER 1", location = "room1", kind = "dialogue", purpose = "주변을 살펴보자.", x = .5f, y = .5f, before = new[] { new DialogueLine { speaker = "you", text = "이곳에서 어떤 일이 기다리고 있을까?", pose = "curious" } } },
                    new InvestigationNode { id = "finale", title = "이야기의 끝", chapter = "CHAPTER 1", location = "room1", kind = "ending", purpose = "마지막 이야기를 확인하자.", x = .5f, y = .5f, requires = new[] { "intro" }, before = new[] { new DialogueLine { speaker = "you", text = "모든 조사를 마쳤어요.", pose = "found" } } }
                }
            };
            return project;
        }
        public static void Save(string folder, HotelStoryProject project)
        {
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "project.json"), temp = path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(project, true));
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak"); else File.Move(temp, path);
        }
        public static void Validate(HotelStoryProject project, string folder, bool publishing)
        {
            ContentValidation.Require(project != null && project.schemaVersion == 1 && ContentValidation.IsId(project.id), "프로젝트 형식이 올바르지 않습니다.");
            ContentValidation.Validate(project.pack);
            ContentValidation.Require(!string.IsNullOrWhiteSpace(project.pack.title), "이야기 제목을 입력하세요.");
            ContentValidation.Require(project.pack.characters.Any(c => c.id == "you"), "주인공(you)이 필요합니다.");
            ContentValidation.Require(project.pack.nodes.Count(n => n.kind == "ending") == 1, "마지막 엔딩 조사는 하나로 설정하세요.");
            // Ensure every required event can be played before the ending, not merely reachable in a graph.
            var progress = new HotelProgress { location = project.pack.locations[0].id };
            var session = new InvestigationSession(project.pack, progress);
            while (!progress.ending && session.Next != null) session.Complete(session.Next.id);
            ContentValidation.Require(progress.ending && progress.completed.Count == project.pack.nodes.Length, "엔딩보다 뒤에 남는 조사가 있습니다. 선행 조사나 순서 연결을 확인하세요.");
            ContentValidation.Require(project.images != null && project.images.All(i => i != null) && project.images.Select(i => i.key).Distinct().Count() == project.images.Length, "그림 슬롯이 중복되거나 비어 있습니다.");
            foreach (var image in project.images) HotelStorage.ValidatePng(HotelStorage.ResolveFile(folder, image.file), image);
            if (publishing) HotelPuzzleClearance.VerifyPack(PublishedPack(project), PublishedProofs(project));
        }
        public static HotelPack PublishedPack(HotelStoryProject project)
        {
            var pack = Copy(project.pack);
            pack.stages = pack.stages.Where(s => pack.nodes.Any(n => n.kind == "stage" && n.contentId == s.id)).ToArray();
            pack.riddles = pack.riddles.Where(r => pack.nodes.Any(n => n.kind == "riddle" && n.contentId == r.id)).ToArray();
            return pack;
        }
        private static PuzzleClearanceFile PublishedProofs(HotelStoryProject project) => new PuzzleClearanceFile {
            puzzles = project.proofs.Where(p => p != null && project.pack.nodes.Any(n => n.kind == "stage" && n.contentId == p.stageId)).ToArray()
        };
        public static void Chain(HotelPack pack)
        {
            for (int i = 0; i < pack.nodes.Length; i++) pack.nodes[i].requires = i == 0 ? Array.Empty<string>() : new[] { pack.nodes[i - 1].id };
        }
        public static string Install(HotelStoryProject project, string projectFolder, HotelStorage destination, bool publishing)
        {
            Validate(project, projectFolder, publishing);
            string id = (publishing ? "adventure." : "preview.") + Guid.NewGuid().ToString("N");
            string target = Path.Combine(destination.ModsPath, id); Directory.CreateDirectory(target);
            var images = Copy(project.images);
            foreach (var image in images)
            {
                string source = HotelStorage.ResolveFile(projectFolder, image.file);
                string relative = "Images/" + Path.GetFileName(image.file);
                string output = HotelStorage.ResolveFile(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(output)); File.Copy(source, output, true); image.file = relative;
            }
            var manifest = new ModManifest { id = id, name = project.pack.title, author = project.author, version = "1.0.0", kind = "adventure", contentFile = "content.json", images = images, clearanceFile = publishing ? "clearance.json" : "", description = "독립 이야기 · " + project.pack.nodes.Length + "개 조사 / " + project.pack.locations.Length + "개 장소" };
            File.WriteAllText(Path.Combine(target, "content.json"), JsonUtility.ToJson(PublishedPack(project), true));
            if (publishing) File.WriteAllText(Path.Combine(target, "clearance.json"), JsonUtility.ToJson(PublishedProofs(project), true));
            File.WriteAllText(Path.Combine(target, "manifest.json"), JsonUtility.ToJson(manifest, true));
            File.WriteAllText(Path.Combine(target, "README.txt"), project.pack.title + "\n폴더를 MOD 폴더에 넣고 게임의 MOD 관리에서 선택한 뒤 새 조사를 시작하세요.\n기존 호텔 이야기와 섞이지 않는 독립 이야기이며 별도 저장을 사용합니다.\n");
            destination.RefreshMods();
            ContentValidation.Require(destination.Mods.Any(m => m.manifest?.id == id && m.Valid), "만든 이야기를 게임에서 불러오지 못했습니다.");
            return id;
        }
        public static string Export(HotelStoryProject project, string folder, HotelStorage storage)
        {
            string id = Install(project, folder, storage, true);
            string target = Path.Combine(storage.ModsPath, id);
            string exports = Path.Combine(storage.Root, "StoryStudio", "Exports"); Directory.CreateDirectory(exports);
            string zip = Path.Combine(exports, id + ".zip");
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
                foreach (string file in Directory.GetFiles(target, "*", SearchOption.AllDirectories))
                    archive.CreateEntryFromFile(file, id + "/" + file.Substring(target.Length + 1).Replace('\\', '/'));
            return zip;
        }
        public static ImageOverride ImportPng(HotelStoryProject project, string folder, string key, string source)
        {
            HotelStorage.ValidatePng(source);
            string relative = "Images/" + Guid.NewGuid().ToString("N") + ".png";
            string target = HotelStorage.ResolveFile(folder, relative); Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(source, target);
            var image = new ImageOverride { key = key, file = relative };
            project.images = project.images.Where(i => i.key != key).Concat(new[] { image }).ToArray();
            Save(folder, project); return image;
        }
    }
}
