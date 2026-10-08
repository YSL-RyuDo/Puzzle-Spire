using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleSpire.Hotel.Tests
{
    public sealed class HotelThemeTests
    {
        private string root;
        private string Content => Path.Combine(Application.streamingAssetsPath, "Hotel", "base.json");
        private string Themes => Path.Combine(Application.streamingAssetsPath, "Hotel", "Themes");
        [SetUp] public void Setup() { root = Path.Combine(Path.GetTempPath(), "HotelThemeTests-" + Guid.NewGuid().ToString("N")); }
        [TearDown] public void Cleanup() { if (Directory.Exists(root)) Directory.Delete(root, true); }
        [Test] public void BundledTheme_SharesBaseSaveAndSurvivesRestartWithoutOverwritingEdits()
        {
            using (var storage = new HotelStorage(Content, root, Themes))
            {
                var story = new InvestigationSession(storage.Pack, storage.NewProgress());
                story.Complete("arrival"); story.Complete("keys"); storage.Save(story.Progress);
                string path = storage.SavePath, bytes = File.ReadAllText(path);
                Assert.That(storage.TryActivate("moonlight.classic", out var error), Is.True, error);
                Assert.That(storage.IsTheme, Is.True); Assert.That(storage.SavePath, Is.EqualTo(path));
                Assert.That(storage.Load(out _).completed, Does.Contain("keys"));
                Assert.That(File.ReadAllText(path), Is.EqualTo(bytes));
                var entry = storage.Mods.Single();
                foreach (var image in entry.manifest.images) Assert.That(storage.Image(image.key), Is.Not.Null, image.key);
                var first = storage.Image("block.0"); var second = storage.Image("block.1");
                Assert.That(first.texture, Is.SameAs(second.texture), "Atlas should share one texture");
                Assert.That(first.rect, Is.Not.EqualTo(second.rect));
                storage.SavePreferences();
                string marker = Path.Combine(entry.directory, "user-edit.txt"); File.WriteAllText(marker, "retain me");
            }
            using (var storage = new HotelStorage(Content, root, Themes))
            {
                Assert.That(storage.PackId, Is.EqualTo("moonlight.classic"));
                Assert.That(File.ReadAllText(Path.Combine(storage.Mods.Single().directory, "user-edit.txt")), Is.EqualTo("retain me"));
                Assert.That(storage.TryActivate("base", out _), Is.True);
                Assert.That(storage.Load(out _).completed, Does.Contain("keys"));
            }
        }
        [Test] public void Theme_CannotChangeStoryAndUnknownKindsAreRejected()
        {
            using (var storage = new HotelStorage(Content, root))
            {
                string dir = Path.Combine(storage.ModsPath, "bad"); Directory.CreateDirectory(dir);
                var manifest = new ModManifest { id = "bad.theme", version = "1", kind = "theme", contentFile = "content.json" };
                File.WriteAllText(Path.Combine(dir, "manifest.json"), JsonUtility.ToJson(manifest)); storage.RefreshMods();
                Assert.That(storage.Mods.Single().Valid, Is.False);
                manifest.contentFile = null; manifest.kind = "unknown";
                File.WriteAllText(Path.Combine(dir, "manifest.json"), JsonUtility.ToJson(manifest)); storage.RefreshMods();
                Assert.That(storage.Mods.Single().Valid, Is.False);
            }
        }
        private void WithAuthoredGame(Action<HotelGame, HotelStorage> check)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/Hotel.unity", OpenSceneMode.Additive);
            try
            {
                var game = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<HotelGame>(true)).Single();
                using (var storage = new HotelStorage(Content, root, Themes))
                {
                    typeof(HotelGame).GetField("storage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(game, storage);
                    game.View.stagePresentation.modPortraitMirror = storage.DialogueMirror;
                    check(game, storage);
                }
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
        private static Sprite DialoguePortrait(HotelGame game, string id, string pose = null) =>
            (Sprite)typeof(HotelGame).GetMethod("Portrait", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, new object[] { id, pose });

        [Test] public void DialogueTheme_UnposedLinesUseModAndRemovingModRestoresInspectorPortraits()
        {
            WithAuthoredGame((game, storage) =>
            {
                var originals = storage.Pack.characters.ToDictionary(c => c.id, c => DialoguePortrait(game, c.id));
                Assert.That(storage.TryActivate("moonlight.classic", out var error), Is.True, error);
                foreach (var character in storage.Pack.characters)
                {
                    foreach (string pose in new[] { null, "", "unprovided-expression" })
                        Assert.That(DialoguePortrait(game, character.id, pose), Is.SameAs(storage.Image("character." + character.id)), character.id + ": unposed/unprovided expression must use MOD portrait");
                }
                foreach (var pair in new[] { ("you", "curious"), ("you", "found"), ("front", "worried"), ("front", "remember") })
                    Assert.That(DialoguePortrait(game, pair.Item1, pair.Item2), Is.SameAs(storage.Image("character." + pair.Item1 + "." + pair.Item2)));
                Assert.That(storage.TryActivate("base", out error), Is.True, error);
                foreach (var pair in originals) Assert.That(DialoguePortrait(game, pair.Key), Is.SameAs(pair.Value));
            });
        }
        [Test] public void DialoguePartner_DefaultAndClassicPortraitsFaceIntoConversation()
        {
            WithAuthoredGame((game, storage) =>
            {
                foreach (string theme in new[] { "base", "moonlight.classic", "base" })
                {
                    Assert.That(storage.TryActivate(theme, out var error), Is.True, error);
                    foreach (string id in new[] { "front", "keeper", "guest" })
                    {
                        var stage = game.View.stagePresentation;
                        stage.Open(DialoguePortrait(game, "you"), DialoguePortrait(game, id));
                        var left = stage.leftActor.rectTransform;
                        var right = stage.rightActor.rectTransform;
                        Assert.That(right.rect.height, Is.GreaterThanOrEqualTo(650), theme + ": " + id + " must not shrink inside a narrow slot");
                        Assert.That(right.anchoredPosition.y, Is.EqualTo(left.anchoredPosition.y).Within(.01f), "Portrait bottoms must share the authored baseline");
                        Assert.That(right.pivot.x, Is.EqualTo(.5f), "Mirroring must preserve the portrait centre");
                        Assert.That(right.pivot.y, Is.Zero, "Wide portraits must not float above the dialogue card");
                        float expected = theme == "base" && id != "guest" ? -1 : 1;
                        Assert.That(Mathf.Sign(stage.rightActor.rectTransform.localScale.x), Is.EqualTo(expected), theme + ": " + id);
                        if (id != "front") continue;
                        foreach (string pose in new[] { "worried", "remember", "" })
                        {
                            stage.Speak(false, DialoguePortrait(game, id, pose), "방향 확인", true);
                            Assert.That(Mathf.Sign(stage.rightActor.rectTransform.localScale.x), Is.EqualTo(expected), theme + ": " + id + "." + pose);
                        }
                    }
                }
            });
        }
        [Test] public void Story_UsesTwoDistinctPosesForEveryCharacterInBothThemes()
        {
            WithAuthoredGame((game, storage) =>
            {
                foreach (string theme in new[] { "base", "moonlight.classic" })
                {
                    Assert.That(storage.TryActivate(theme, out var error), Is.True, error);
                    foreach (var character in storage.Pack.characters)
                    {
                        var poses = storage.Pack.nodes.SelectMany(n => n.before.Concat(n.after)).Where(l => l.speaker == character.id).Select(l => l.pose).Distinct().ToArray();
                        Assert.That(poses.Length, Is.GreaterThanOrEqualTo(2), character.id + ": story must actually switch pose art");
                        var pictures = poses.Select(p => DialoguePortrait(game, character.id, p)).ToArray();
                        Assert.That(pictures.All(s => s != null), Is.True);
                        Assert.That(pictures.Distinct().Count(), Is.EqualTo(poses.Length), "Different poses must resolve to different pictures: " + theme + "/" + character.id);
                        foreach (var pose in poses)
                        {
                            var portrait = DialoguePortrait(game, character.id, pose);
                            if (theme != "base") Assert.That(portrait, Is.SameAs(storage.Image("character." + character.id + "." + pose)));
                            var stage = game.View.stagePresentation;
                            stage.Open(DialoguePortrait(game, "you"), DialoguePortrait(game, character.id));
                            stage.Speak(character.id == "you", portrait, "자세 확인", true);
                            float expected = theme == "base" && character.id != "you" && character.id != "guest" || theme != "base" && character.id == "you" && pose == "found" ? -1 : 1;
                            var actor = character.id == "you" ? stage.leftActor : stage.rightActor;
                            Assert.That(Mathf.Sign(actor.rectTransform.localScale.x), Is.EqualTo(expected), theme + "/" + character.id + "/" + pose);
                        }
                    }
                }
            });
        }
        [TestCase(-1, 0, 1, 1)] [TestCase(0, 0, 0, 1)] [TestCase(0, 0, 99999, 1)] [TestCase(int.MaxValue, 0, int.MaxValue, 1)]
        public void Atlas_RejectsInvalidRectangles(int x, int y, int w, int h)
        {
            string png = Path.Combine(Application.streamingAssetsPath, "Hotel", "ExampleMod", "red-block.png");
            Assert.Throws<InvalidOperationException>(() => HotelStorage.ValidatePng(png, new ImageOverride { x = x, y = y, width = w, height = h }));
        }
    }
}
