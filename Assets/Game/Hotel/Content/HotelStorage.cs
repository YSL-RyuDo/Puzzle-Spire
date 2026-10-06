using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed class ModEntry
    {
        public ModManifest manifest;
        public string directory, error;
        public HotelPack pack;
        public bool Valid => string.IsNullOrEmpty(error);
    }
    [Serializable] public sealed class HotelPreferences
    {
        public string activeMod = "base";
        public bool sound = true;
    }

    public sealed class HotelStorage : IDisposable
    {
        public string Root { get; }
        public string ModsPath => Path.Combine(Root, "Mods");
        public HotelPreferences Preferences { get; private set; }
        public HotelPack BasePack { get; }
        public HotelPack Pack { get; private set; }
        public string PackId { get; private set; } = "base";
        public string PackVersion { get; private set; } = "1.0.0";
        public string PackName { get; private set; } = "기본 이야기";
        public string Warning { get; private set; }
        public List<ModEntry> Mods { get; } = new List<ModEntry>();
        public bool IsTheme { get; private set; }
        private string SavePackId => IsTheme ? "base" : PackId;
        private string SavePackVersion => IsTheme ? "1.0.0" : PackVersion;
        private readonly Dictionary<string, string> imageFiles = new Dictionary<string, string>();
        private readonly Dictionary<string, ImageOverride> imageRegions = new Dictionary<string, ImageOverride>();
        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public HotelStorage(string contentPath, string root, string bundledThemes = null)
        {
            Root = root;
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(ModsPath);
            BasePack = ReadJson<HotelPack>(contentPath);
            ContentValidation.Validate(BasePack);
            Pack = BasePack;
            try { Preferences = File.Exists(PreferencePath) ? ReadJson<HotelPreferences>(PreferencePath) : new HotelPreferences(); }
            catch (Exception e) { Preferences = new HotelPreferences(); Warning = "설정을 읽지 못해 기본 설정을 사용합니다: " + e.Message; }
            if (bundledThemes != null && Directory.Exists(bundledThemes)) InstallBundledThemes(bundledThemes);
            RefreshMods();
            if (!TryActivate(Preferences.activeMod, out string error)) Warning = error;
        }
        private string PreferencePath => Path.Combine(Root, "preferences.json");
        public string SavePath => Path.Combine(Root, "Saves", SavePackId + ".json");
        public bool HasSave => File.Exists(SavePath) || File.Exists(SavePath + ".bak");
        public static T ReadJson<T>(string path)
        {
            var info = new FileInfo(path);
            ContentValidation.Require(info.Exists && info.Length <= 2 * 1024 * 1024, "파일이 없거나 2MB보다 큽니다: " + path);
            var data = JsonUtility.FromJson<T>(File.ReadAllText(path));
            ContentValidation.Require(data != null, "빈 JSON입니다: " + path);
            return data;
        }
        public static string ResolveFile(string root, string relative)
        {
            ContentValidation.Require(!string.IsNullOrWhiteSpace(relative) && !Path.IsPathRooted(relative), "MOD 파일은 상대 경로여야 합니다.");
            string basePath = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string result = Path.GetFullPath(Path.Combine(basePath, relative));
            ContentValidation.Require(result.StartsWith(basePath, StringComparison.OrdinalIgnoreCase), "MOD 폴더 밖의 파일은 사용할 수 없습니다.");
            // Do not follow a junction or symlink out of the selected pack.
            for (var current = new FileInfo(result).Directory; current != null && current.FullName.Length >= basePath.Length - 1; current = current.Parent)
                if (current.Exists) ContentValidation.Require((current.Attributes & FileAttributes.ReparsePoint) == 0, "MOD 연결 폴더는 지원하지 않습니다.");
            if (File.Exists(result)) ContentValidation.Require((File.GetAttributes(result) & FileAttributes.ReparsePoint) == 0, "MOD 연결 파일은 지원하지 않습니다.");
            return result;
        }
        public void RefreshMods()
        {
            Mods.Clear();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "base" };
            foreach (string dir in Directory.GetDirectories(ModsPath).OrderBy(x => x))
            {
                var entry = new ModEntry { directory = dir };
                try
                {
                    entry.manifest = ReadJson<ModManifest>(ResolveFile(dir, "manifest.json"));
                    var m = entry.manifest;
                    ContentValidation.Require(m.schemaVersion == 1 && ContentValidation.IsId(m.id) && ids.Add(m.id) && !string.IsNullOrWhiteSpace(m.version), "MOD ID·버전 오류 또는 중복 ID입니다.");
                    ContentValidation.Require(string.IsNullOrEmpty(m.kind) || m.kind == "content" || m.kind == "theme", "지원하지 않는 MOD 종류입니다.");
                    ContentValidation.Require(m.kind != "theme" || string.IsNullOrWhiteSpace(m.contentFile), "테마 MOD에는 이야기 변경 파일을 넣을 수 없습니다.");
                    entry.pack = JsonUtility.FromJson<HotelPack>(JsonUtility.ToJson(BasePack));
                    if (!string.IsNullOrWhiteSpace(m.contentFile))
                    {
                        var patch = ReadJson<HotelPack>(ResolveFile(dir, m.contentFile));
                        ContentValidation.Require(patch.schemaVersion == 1, "지원하지 않는 MOD 데이터 버전입니다.");
                        Overlay(entry.pack, patch);
                    }
                    ContentValidation.Validate(entry.pack);
                    ContentValidation.Require(m.images != null, "images 목록이 필요합니다.");
                    var keys = new HashSet<string>();
                    foreach (var image in m.images)
                    {
                        ContentValidation.Require(image != null && !string.IsNullOrWhiteSpace(image.key) && keys.Add(image.key), "이미지 키 중복/오류입니다.");
                        ValidatePng(ResolveFile(dir, image.file), image);
                    }
                }
                catch (Exception e) { entry.error = e.Message; }
                Mods.Add(entry);
            }
        }
        private static T[] Merge<T>(T[] original, T[] changes, Func<T, string> id)
        {
            if (changes == null || changes.Length == 0) return original;
            var list = original.ToList(); var changed = new HashSet<string>();
            foreach (var item in changes)
            {
                ContentValidation.Require(item != null && ContentValidation.IsId(id(item)) && changed.Add(id(item)), "MOD 콘텐츠 ID 중복/오류입니다.");
                int index = list.FindIndex(x => id(x) == id(item));
                if (index < 0) list.Add(item); else list[index] = item;
            }
            return list.ToArray();
        }
        private static void Overlay(HotelPack target, HotelPack patch)
        {
            target.locations = Merge(target.locations, patch.locations, x => x.id);
            target.characters = Merge(target.characters, patch.characters, x => x.id);
            target.clues = Merge(target.clues, patch.clues, x => x.id);
            target.nodes = Merge(target.nodes, patch.nodes, x => x.id);
            target.stages = Merge(target.stages, patch.stages, x => x.id);
            target.riddles = Merge(target.riddles, patch.riddles, x => x.id);
        }
        public bool TryActivate(string id, out string error)
        {
            error = null;
            var entry = id == "base" ? null : Mods.FirstOrDefault(x => x.manifest != null && x.manifest.id == id);
            if (id != "base" && (entry == null || !entry.Valid))
            {
                error = "선택했던 MOD를 불러올 수 없습니다. MOD 관리에서 확인해 주세요. " + id + "\n" + entry?.error;
                return false;
            }
            ClearSprites(); imageFiles.Clear(); imageRegions.Clear();
            Pack = entry?.pack ?? BasePack;
            PackId = entry?.manifest.id ?? "base";
            PackVersion = entry?.manifest.version ?? "1.0.0";
            PackName = entry?.manifest.name ?? "기본 이야기";
            IsTheme = entry?.manifest.kind == "theme";
            if (entry != null) foreach (var image in entry.manifest.images)
            { imageFiles[image.key] = ResolveFile(entry.directory, image.file); imageRegions[image.key] = image; }
            Preferences.activeMod = PackId;
            return true;
        }
        public void SavePreferences() => AtomicWrite(PreferencePath, JsonUtility.ToJson(Preferences, true));
        public HotelProgress NewProgress() => new HotelProgress { packId = SavePackId, packVersion = SavePackVersion, location = Pack.locations[0].id };
        public void Save(HotelProgress progress)
        {
            ValidateProgress(progress);
            AtomicWrite(SavePath, JsonUtility.ToJson(progress, true));
        }
        public HotelProgress Load(out string warning)
        {
            warning = null;
            try
            {
                var p = ReadJson<HotelProgress>(SavePath); ValidateProgress(p); return p;
            }
            catch (Exception primary)
            {
                try
                {
                    var p = ReadJson<HotelProgress>(SavePath + ".bak"); ValidateProgress(p);
                    warning = "최근 저장을 읽지 못해 이전 백업에서 복구했습니다."; return p;
                }
                catch { throw new InvalidOperationException("저장을 불러올 수 없습니다. 기존 파일은 보존됩니다.\n" + primary.Message); }
            }
        }
        private void ValidateProgress(HotelProgress p)
        {
            ContentValidation.Require(p != null && p.schemaVersion == 1 && p.packId == SavePackId && p.packVersion == SavePackVersion && p.completed != null && p.clues != null, "저장 버전이나 MOD 버전이 맞지 않습니다.");
            ContentValidation.Require(p.completed.Distinct().Count() == p.completed.Count && p.clues.Distinct().Count() == p.clues.Count, "중복된 진행 기록입니다.");
            var replay = new InvestigationSession(Pack, NewProgress());
            var pending = new HashSet<string>(p.completed);
            while (pending.Count > 0)
            {
                var next = Pack.nodes.FirstOrDefault(n => pending.Contains(n.id) && replay.IsAvailable(n));
                ContentValidation.Require(next != null, "저장된 조사 순서가 현재 콘텐츠와 맞지 않습니다.");
                replay.Complete(next.id); pending.Remove(next.id);
            }
            var location = Pack.locations.FirstOrDefault(l => l.id == p.location);
            ContentValidation.Require(location != null && replay.IsUnlocked(location) && new HashSet<string>(replay.Progress.clues).SetEquals(p.clues) && replay.Progress.ending == p.ending, "단서 또는 장소 진행 기록이 일치하지 않습니다.");
        }
        public static void AtomicWrite(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temp = path + ".tmp";
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
            { writer.Write(text); writer.Flush(); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
            else File.Move(temp, path);
        }
        public void InstallExample(string source)
        {
            string target = Path.Combine(ModsPath, "MoonlightExample");
            ContentValidation.Require(!Directory.Exists(target), "샘플 MOD가 이미 설치되어 있습니다. 기존 파일은 덮어쓰지 않습니다.");
            Directory.CreateDirectory(target);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                string relative = file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destination = ResolveFile(target, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)); File.Copy(file, destination);
            }
            RefreshMods();
        }
        private void InstallBundledThemes(string source)
        {
            foreach (var folder in Directory.GetDirectories(source))
            {
                string target = ResolveFile(ModsPath, Path.GetFileName(folder));
                // User edits to an installed pack are never replaced on launch.
                if (Directory.Exists(target)) continue;
                foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                    string relative = file.Substring(folder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string destination = ResolveFile(target, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)); File.Copy(file, destination);
                }
            }
        }
        public static void ValidatePng(string path, ImageOverride region = null)
        {
            var info = new FileInfo(path);
            ContentValidation.Require(info.Exists && info.Length >= 24 && info.Length <= 8 * 1024 * 1024 && Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase), "PNG 파일이 없거나 8MB보다 큽니다.");
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                byte[] header = reader.ReadBytes(24);
                ContentValidation.Require(header.Take(8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}), "PNG 형식이 아닙니다.");
                long Width(int offset) => ((long)header[offset] << 24) | ((long)header[offset + 1] << 16) | ((long)header[offset + 2] << 8) | header[offset + 3];
                ContentValidation.Require(Width(16) > 0 && Width(16) <= 4096 && Width(20) > 0 && Width(20) <= 4096, "이미지는 최대 4096×4096입니다.");
                if (region != null && (region.x != 0 || region.y != 0 || region.width != 0 || region.height != 0))
                    ContentValidation.Require(region.x >= 0 && region.y >= 0 && region.width > 0 && region.height > 0 && (long)region.x + region.width <= Width(16) && (long)region.y + region.height <= Width(20), "이미지 영역이 PNG 범위를 벗어났습니다.");
            }
        }
        public Sprite Image(string key)
        {
            if (sprites.TryGetValue(key, out var cached)) return cached;
            if (!imageFiles.TryGetValue(key, out string path)) return null;
            try
            {
                var region = imageRegions[key]; ValidatePng(path, region);
                if (!textures.TryGetValue(path, out var texture))
                {
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(File.ReadAllBytes(path), true)) { Release(texture); throw new IOException("이미지를 해석할 수 없습니다."); }
                    texture.wrapMode = TextureWrapMode.Clamp; textures[path] = texture;
                }
                var rect = region.width > 0 ? new Rect(region.x, region.y, region.width, region.height) : new Rect(0, 0, texture.width, texture.height);
                var sprite = Sprite.Create(texture, rect, Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
                sprites[key] = sprite; return sprite;
            }
            catch (Exception e) { Warning = "MOD 이미지 대신 기본 그림을 표시합니다: " + e.Message; sprites[key] = null; return null; }
        }
        private void ClearSprites()
        {
            foreach (var sprite in sprites.Values) if (sprite != null) Release(sprite);
            foreach (var texture in textures.Values) if (texture != null) Release(texture);
            textures.Clear();
            sprites.Clear();
        }
        private static void Release(UnityEngine.Object value)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
        public void Dispose() => ClearSprites();
    }
}
