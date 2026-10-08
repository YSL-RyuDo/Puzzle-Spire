using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace PuzzleSpire.Hotel.Tests
{
    public sealed class HotelStoryStudioTests
    {
        private string root;
        private string BasePath => Path.Combine(Application.streamingAssetsPath, "Hotel", "base.json");
        private HotelStoryProject New() => HotelStoryProjects.Create(HotelStorage.ReadJson<HotelPack>(BasePath), false);
        [SetUp] public void Setup() => root = Path.Combine(Path.GetTempPath(), "StoryStudioTests-" + Guid.NewGuid().ToString("N"));
        [TearDown] public void Cleanup() { if (Directory.Exists(root)) Directory.Delete(root, true); }
        [Test] public void ProjectRoundTripKeepsNestedDialogueAndIndependentArrays()
        {
            var p = New(); var clone = HotelStoryProjects.Copy(p); clone.pack.nodes[0].before[0].text = "Changed";
            Assert.That(p.pack.nodes[0].before[0].text, Is.Not.EqualTo("Changed"));
            HotelStoryProjects.Save(root, clone); HotelStoryProjects.Save(root, clone);
            var read = HotelStorage.ReadJson<HotelStoryProject>(Path.Combine(root, "project.json"));
            Assert.That(read.pack.nodes[0].before[0].text, Is.EqualTo("Changed"));
            Assert.That(read.pack.stages, Is.Empty); Assert.That(read.images, Is.Empty);
        }
        [Test] public void DialogueOnlyAdventureExportsAndReplacesBaseStoryWithIsolatedSave()
        {
            var p = New();
            using (var store = new HotelStorage(BasePath, root))
            {
                store.Save(store.NewProgress()); string save = File.ReadAllText(store.SavePath);
                string zip = HotelStoryProjects.Export(p, root, store);
                using (var archive = ZipFile.OpenRead(zip)) Assert.That(archive.Entries.Count, Is.EqualTo(4));
                var mod = store.Mods.Single(m => m.manifest.id.StartsWith("adventure."));
                Assert.That(store.TryActivate(mod.manifest.id, out var error), Is.True, error);
                Assert.That(store.Pack.nodes.Length, Is.EqualTo(2)); Assert.That(store.Pack.stages, Is.Empty);
                Assert.That(store.HasSave, Is.False);
                var progress = store.NewProgress(); var session = new InvestigationSession(store.Pack, progress);
                while (session.Next != null) session.Complete(session.Next.id);
                Assert.That(progress.ending, Is.True); store.Save(progress);
                Assert.That(store.TryActivate("base", out _), Is.True); Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(save));
            }
        }
        [Test] public void EndingBeforeRemainingInvestigationIsRejected()
        {
            var p = New(); p.pack.nodes = p.pack.nodes.Concat(new[] { new InvestigationNode { id = "late", kind = "dialogue", location = "room1", requires = new[] { "finale" } } }).ToArray();
            Assert.Throws<InvalidOperationException>(() => HotelStoryProjects.Validate(p, root, false));
        }
        [Test] public void FourthLocationAndSecondChapterAreReachable()
        {
            var p = New();
            p.pack.locations = p.pack.locations.Concat(Enumerable.Range(2, 3).Select(i => new LocationData { id = "room" + i, name = "Room " + i })).ToArray();
            p.pack.nodes[1].location = "room4"; p.pack.nodes[1].chapter = "CHAPTER 2";
            Assert.DoesNotThrow(() => HotelStoryProjects.Validate(p, root, true));
            p.pack.locations[3].unlockAfter = "finale";
            Assert.Throws<InvalidOperationException>(() => HotelStoryProjects.Validate(p, root, false));
        }
        [Test] public void UnusedDraftPuzzleDoesNotBlockPublishingButLinkedPuzzleNeedsProof()
        {
            var p = New(); var source = HotelStorage.ReadJson<HotelPack>(BasePath).stages[0];
            p.pack.stages = new[] { source };
            Assert.DoesNotThrow(() => HotelStoryProjects.Validate(p, root, true));
            Assert.That(HotelStoryProjects.PublishedPack(p).stages, Is.Empty);
            p.pack.nodes[0].kind = "stage"; p.pack.nodes[0].contentId = source.id;
            Assert.Throws<InvalidOperationException>(() => HotelStoryProjects.Validate(p, root, true));
        }
        [Test] public void ImportedImageSurvivesOriginalRemovalAndKeepsCropInMod()
        {
            Directory.CreateDirectory(root); string source = Path.Combine(root, "source.png");
            var texture = new Texture2D(8, 8); File.WriteAllBytes(source, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            var p = New(); var art = HotelStoryProjects.ImportPng(p, Path.Combine(root, "project"), "character.you", source);
            art.x = 2; art.y = 1; art.width = 4; art.height = 5; art.flipInDialogue = true; File.Delete(source);
            using (var store = new HotelStorage(BasePath, Path.Combine(root, "runtime")))
            {
                string id = HotelStoryProjects.Install(p, Path.Combine(root, "project"), store, true);
                Assert.That(store.TryActivate(id, out var error), Is.True, error);
                var image = store.Mods.Single(m => m.manifest.id == id).manifest.images.Single();
                Assert.That(image.width, Is.EqualTo(4)); Assert.That(image.flipInDialogue, Is.True);
            }
            art.width = 9; Assert.Throws<InvalidOperationException>(() => HotelStoryProjects.Validate(p, Path.Combine(root, "project"), false));
        }
    }
}
