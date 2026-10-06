using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
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
        [TestCase(-1, 0, 1, 1)] [TestCase(0, 0, 0, 1)] [TestCase(0, 0, 99999, 1)] [TestCase(int.MaxValue, 0, int.MaxValue, 1)]
        public void Atlas_RejectsInvalidRectangles(int x, int y, int w, int h)
        {
            string png = Path.Combine(Application.streamingAssetsPath, "Hotel", "ExampleMod", "red-block.png");
            Assert.Throws<InvalidOperationException>(() => HotelStorage.ValidatePng(png, new ImageOverride { x = x, y = y, width = w, height = h }));
        }
    }
}
