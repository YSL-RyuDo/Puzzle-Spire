using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel.Editor
{
    public static partial class HotelSceneBuilder
    {
        private const string ClassicFolder = "Assets/StreamingAssets/Hotel/Themes/ClassicIllustrated";
        [MenuItem("Puzzle Spire/손그림 기본 테마 적용 · 기존 그림 MOD 보존 (최초 1회)")]
        public static void UpgradeMystery()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = UnityEngine.Object.FindFirstObjectByType<HotelView>();
            if (v.investigation.pieces[0].name.StartsWith("Mystery")) { Debug.Log("Mystery art already authored; preserving Inspector edits."); return; }
            Directory.CreateDirectory("Builds/Backups");
            File.Copy(ScenePath, "Builds/Backups/Hotel-before-mystery-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
            var game = new SerializedObject(v.GetComponent<HotelGame>());
            var old = new Dictionary<string, Sprite>();
            string[] rooms = { "lobby", "corridor", "archive" };
            string[] people = { "you", "front", "keeper", "guest" };
            string[] poses = { "you.curious", "you.found", "front.worried", "front.remember" };
            for (int i = 0; i < rooms.Length; i++) old.Add("background." + rooms[i], v.locationImages[i].sprite);
            for (int i = 0; i < people.Length; i++) old.Add("character." + people[i], (Sprite)game.FindProperty("defaultPortraits").GetArrayElementAtIndex(i).objectReferenceValue);
            for (int i = 0; i < poses.Length; i++) old.Add("character." + poses[i], v.investigation.poses[i]);
            for (int i = 0; i < HotelThemeKeys.Pieces.Length; i++) old.Add(HotelThemeKeys.Pieces[i], v.investigation.pieces[i]);
            old.Add("inspection.cabinet", v.investigation.cabinet);
            for (int i = 0; i < v.collection.illustrationIds.Length; i++) old.Add("clue." + v.collection.illustrationIds[i], v.collection.illustrations[i]);
            old.Add("clue.fallback", v.collection.fallback);
            ExportClassicTheme(old);

            var backgrounds = new[] { ImportBackground("MysteryLobby"), ImportBackground("MysteryCorridor"), ImportBackground("MysteryArchive"), ImportBackground("MysteryCabinet") };
            var characters = ImportAtlas("MysteryCharacters", 2, 2);
            var expressions = ImportAtlas("MysteryPoses", 2, 2);
            var pieces = ImportAtlas("MysteryPieces", 4, 4);
            var evidence = ImportAtlas("MysteryEvidence", 4, 2);
            var replacements = new Dictionary<Sprite, Sprite>();
            for (int i = 0; i < 3; i++) replacements[old["background." + rooms[i]]] = backgrounds[i];
            for (int i = 0; i < 4; i++)
            {
                replacements[old["character." + people[i]]] = characters[i];
                replacements[old["character." + poses[i]]] = expressions[i];
                game.FindProperty("defaultPortraits").GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }
            for (int i = 0; i < pieces.Length; i++) replacements[old[HotelThemeKeys.Pieces[i]]] = pieces[i];
            for (int i = 0; i < 5; i++) game.FindProperty("defaultBlockSprites").GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            replacements[v.investigation.cabinet] = backgrounds[3];
            for (int i = 0; i < v.collection.illustrations.Length; i++) replacements[v.collection.illustrations[i]] = evidence[i];
            replacements[v.collection.fallback] = evidence[7];
            var theme = v.GetComponent<HotelThemeView>() ?? v.gameObject.AddComponent<HotelThemeView>();
            var bindings = new List<HotelThemeView.Binding>();
            foreach (var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!image.sprite || !replacements.TryGetValue(image.sprite, out var replacement)) continue;
                string key = old.FirstOrDefault(pair => pair.Value == image.sprite && (pair.Key.StartsWith("background.") || pair.Key == "inspection.cabinet")).Key;
                image.sprite = replacement;
                if (key != null) bindings.Add(new HotelThemeView.Binding { key = key, image = image, defaultSprite = replacement });
                EditorUtility.SetDirty(image);
            }
            theme.backgrounds = bindings.ToArray();
            v.stagePresentation.mirrorOnRight = characters.Concat(expressions).ToArray();
            EditorUtility.SetDirty(v.stagePresentation);
            v.investigation.pieces = pieces; v.investigation.poses = expressions; v.investigation.cabinet = backgrounds[3];
            v.collection.illustrations = evidence.Take(7).ToArray(); v.collection.fallback = evidence[7];
            game.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(v.investigation); EditorUtility.SetDirty(v.collection); EditorUtility.SetDirty(theme);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_MYSTERY_ART_AUTHORED: " + old.Count + " original image keys preserved as theme MOD.");
        }
        public static void UpgradeMysteryAndBuild() { UpgradeMystery(); CreateAndBuild(); }
        public static void PolishMysteryAndBuild()
        {
            ImportAtlas("MysteryPieces", 4, 4); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath);
            v = UnityEngine.Object.FindFirstObjectByType<HotelView>();
            var actor = v.stagePresentation.rightActor.rectTransform;
            // Mirror around the image centre, preserving its authored world-space bounds.
            // A top-left pivot would shift the mirrored portrait by its entire width.
            var position = actor.anchoredPosition;
            position.x += (.5f - actor.pivot.x) * actor.rect.width;
            actor.pivot = new Vector2(.5f, actor.pivot.y); actor.anchoredPosition = position;
            EditorUtility.SetDirty(actor);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            CreateAndBuild();
        }
        public static void RefineMysterySlicesAndBuild()
        {
            ImportAtlas("MysteryCharacters", 2, 2); ImportAtlas("MysteryPoses", 2, 2);
            ImportAtlas("MysteryPieces", 4, 4); ImportAtlas("MysteryEvidence", 4, 2);
            AssetDatabase.SaveAssets(); CreateAndBuild();
        }
        private static void ExportClassicTheme(Dictionary<string, Sprite> images)
        {
            if (File.Exists(Path.Combine(ClassicFolder, "manifest.json")))
            { Debug.Log("Existing Classic theme retained."); return; }
            Directory.CreateDirectory(Path.Combine(ClassicFolder, "Images"));
            var overrides = new List<ImageOverride>();
            foreach (var pair in images)
            {
                var sprite = pair.Value;
                if (!sprite) throw new InvalidOperationException("Missing theme image: " + pair.Key);
                string source = AssetDatabase.GetAssetPath(sprite.texture);
                string relative = "Images/" + Path.GetFileName(source);
                string destination = Path.Combine(ClassicFolder, relative);
                if (!File.Exists(destination)) File.Copy(source, destination);
                var importer = (TextureImporter)AssetImporter.GetAtPath(source);
                importer.GetSourceTextureWidthAndHeight(out int sw, out int sh);
                float sx = sw / (float)sprite.texture.width, sy = sh / (float)sprite.texture.height;
                int x = Mathf.RoundToInt(sprite.rect.xMin * sx), y = Mathf.RoundToInt(sprite.rect.yMin * sy);
                int right = Mathf.Min(sw, Mathf.RoundToInt(sprite.rect.xMax * sx));
                int top = Mathf.Min(sh, Mathf.RoundToInt(sprite.rect.yMax * sy));
                overrides.Add(new ImageOverride { key = pair.Key, file = relative, x = x, y = y, width = right - x, height = top - y });
            }
            var manifest = new ModManifest { id = "moonlight.classic", name = "클래식 일러스트 · 기존 그림", version = "1.0.0", kind = "theme", author = "Puzzle Spire", description = "기존 인물·배경·블록·수집품으로 바꿉니다. 기본 진행 유지.", images = overrides.ToArray() };
            File.WriteAllText(Path.Combine(ClassicFolder, "manifest.json"), JsonUtility.ToJson(manifest, true));
            File.WriteAllText(Path.Combine(ClassicFolder, "README.txt"), "잔월 호텔 테마 MOD\n시작 화면 > MOD 관리 > 클래식 일러스트 선택.\n이 폴더를 MOD 폴더에 넣으면 다른 PC에서도 적용됩니다. 기본 이야기의 저장을 함께 사용합니다.\n이미지는 AI 생성 그림입니다. manifest.json의 영역 좌표는 원본 PNG의 왼쪽 아래 기준입니다.\n");
            Directory.CreateDirectory("Builds/Mods");
            string zip = "Builds/Mods/ClassicIllustrated-1.0.0.zip";
            if (!File.Exists(zip)) ZipFile.CreateFromDirectory(ClassicFolder, zip, System.IO.Compression.CompressionLevel.Optimal, true);
            AssetDatabase.Refresh();
        }
    }
}
