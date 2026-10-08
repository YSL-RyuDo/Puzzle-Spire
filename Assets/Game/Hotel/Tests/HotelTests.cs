using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Hotel.Tests
{
    public sealed class HotelTests
    {
        private string root;
        private string ContentPath => Path.Combine(Application.streamingAssetsPath, "Hotel", "base.json");
        private HotelPack Pack() => HotelStorage.ReadJson<HotelPack>(ContentPath);
        [SetUp] public void SetUp() { root = Path.Combine(Path.GetTempPath(), "HotelTests-" + Guid.NewGuid().ToString("N")); }
        [TearDown] public void TearDown() { if (Directory.Exists(root)) Directory.Delete(root, true); }

        [Test] public void BaseCampaign_HasTwelveStagesAndReachableEnding()
        {
            var pack = Pack(); ContentValidation.Validate(pack);
            Assert.That(pack.stages.Length, Is.EqualTo(12)); Assert.That(pack.riddles.Length, Is.EqualTo(4));
            var story = new InvestigationSession(pack, new HotelProgress());
            Assert.That(story.Complete(pack.nodes.Last().id), Is.False);
            while (story.Next != null) Assert.That(story.Complete(story.Next.id), Is.True);
            Assert.That(story.Progress.ending, Is.True);
            Assert.That(story.Progress.clues.Count, Is.EqualTo(pack.clues.Length));
            Assert.That(story.Complete(pack.nodes[0].id), Is.False);
        }
        [Test] public void Validator_RejectsRoomUnlockDeadlock()
        {
            var pack = Pack(); pack.locations[0].unlockAfter = pack.nodes[0].id;
            Assert.Throws<InvalidOperationException>(() => ContentValidation.Validate(pack));
        }
        [Test] public void Validator_RejectsImpossibleObstacleGoal()
        {
            var pack = Pack(); pack.stages[0].boxes = 999;
            Assert.Throws<InvalidOperationException>(() => ContentValidation.Validate(pack));
        }
        [Test] public void SaveBeforeVisualUpdate_StillLoadsWithoutNewSerializedClues()
        {
            Directory.CreateDirectory(root);
            var oldPack = Pack();
            foreach (var node in oldPack.nodes)
                node.inspection = node.discovery = node.discoveryTitle = node.discoveryText = null;
            string previousContent = Path.Combine(root, "before-visual-update.json");
            File.WriteAllText(previousContent, JsonUtility.ToJson(oldPack));
            using (var previous = new HotelStorage(previousContent, root))
            {
                var story = new InvestigationSession(previous.Pack, previous.NewProgress());
                story.Complete("arrival"); story.Complete("keys"); previous.Save(story.Progress);
            }
            using (var current = new HotelStorage(ContentPath, root))
            {
                var loaded = current.Load(out var warning);
                Assert.That(warning, Is.Null);
                Assert.That(loaded.completed, Does.Contain("keys"));
                Assert.That(loaded.clues, Is.Empty);
                Assert.That(current.Pack.nodes.Single(n => n.id == "keys").discovery, Is.EqualTo("thread"));
            }
        }
        [Test] public void Validator_RejectsNotebookOverflowIncludingVisualObservations()
        {
            var pack = Pack();
            pack.clues = pack.clues.Concat(Enumerable.Range(pack.clues.Length, 200 - pack.clues.Length).Select(i => new ClueData { id = "extra_" + i, title = "단서", description = "설명" })).ToArray();
            foreach (var node in pack.nodes.Take(5))
            { node.discovery = "thread"; node.discoveryTitle = "흰 실"; node.discoveryText = "관찰 내용"; }
            Assert.Throws<InvalidOperationException>(() => ContentValidation.Validate(pack));
        }
        [Test] public void Riddles_CheckOrderAndRejectIncompleteAnswers()
        {
            foreach (var riddle in Pack().riddles)
            {
                Assert.That(InvestigationSession.CheckAnswer(riddle, riddle.answer), Is.True);
                Assert.That(InvestigationSession.CheckAnswer(riddle, Array.Empty<int>()), Is.False);
                Assert.That(InvestigationSession.CheckAnswer(riddle, new[] { -1 }), Is.False);
            }
        }
        [Test] public void SaveLoad_RoundTripsAndRejectsUnreachableProgress()
        {
            using (var storage = new HotelStorage(ContentPath, root))
            {
                var story = new InvestigationSession(storage.Pack, storage.NewProgress());
                for (int i = 0; i < 5; i++) story.Complete(story.Next.id);
                storage.Save(story.Progress);
                var loaded = storage.Load(out var warning);
                Assert.That(warning, Is.Null); Assert.That(loaded.completed, Is.EqualTo(story.Progress.completed));
                loaded.completed.Add("ending"); Assert.Throws<InvalidOperationException>(() => storage.Save(loaded));
            }
        }
        [Test] public void SaveLoad_RecoversPreviousCheckpointFromBackup()
        {
            using (var storage = new HotelStorage(ContentPath, root))
            {
                var p = storage.NewProgress(); storage.Save(p);
                var story = new InvestigationSession(storage.Pack, p); story.Complete(story.Next.id); storage.Save(p);
                File.WriteAllText(storage.SavePath, "broken json");
                var recovered = storage.Load(out string warning);
                Assert.That(warning, Is.Not.Null); Assert.That(recovered.completed, Is.Empty);
            }
        }
        [Test] public void ExampleMod_OverridesStageAddsContentAndSeparatesSave()
        {
            using (var storage = new HotelStorage(ContentPath, root))
            {
                storage.Save(storage.NewProgress());
                storage.InstallExample(Path.Combine(Application.streamingAssetsPath, "Hotel", "ExampleMod"));
                Assert.That(storage.Mods.Count, Is.EqualTo(1)); Assert.That(storage.Mods[0].error, Is.Null);
                Assert.That(storage.TryActivate("moonlight.example", out _), Is.True);
                Assert.That(storage.Pack.stages.First().moves, Is.EqualTo(35));
                Assert.That(storage.Pack.stages.Length, Is.EqualTo(13)); Assert.That(storage.HasSave, Is.False);
                Assert.That(storage.Image("block.0"), Is.Not.Null);
                storage.Save(storage.NewProgress());
                Assert.That(storage.TryActivate("base", out _), Is.True); Assert.That(storage.HasSave, Is.True);
                Assert.That(storage.Pack.stages.Length, Is.EqualTo(12));
            }
        }
        [Test] public void ModLoader_ReportsBrokenPackWithoutBreakingBaseGame()
        {
            using (var storage = new HotelStorage(ContentPath, root))
            {
                string broken = Path.Combine(storage.ModsPath, "Broken");Directory.CreateDirectory(broken);
                File.WriteAllText(Path.Combine(broken,"manifest.json"),"{}");storage.RefreshMods();
                Assert.That(storage.Mods.Single().Valid, Is.False);
                Assert.That(storage.TryActivate("missing",out string error), Is.False);
                Assert.That(error, Is.Not.Empty);Assert.That(storage.PackId, Is.EqualTo("base"));
            }
        }
        [TestCase("../outside.json")]
        [TestCase("C:/outside.json")]
        public void ModPaths_CannotEscapePack(string path)
        { Assert.Throws<InvalidOperationException>(() => HotelStorage.ResolveFile(root, path)); }

        [Test] public void DeadBoard_RecoversWithoutUsingAMoveOrRemovingObstacles()
        {
            var s = Pack().stages[2];var p = new HotelPuzzle(s);
            for (int y=0;y<p.Board.Height;y++)for(int x=0;x<p.Board.Width;x++)
            {
                var at=new GridPosition(x,y);if(!p.Board.IsPlayable(at)||p.Board.GetBlock(at)==null)continue;
                p.Board.RemoveBlock(at);p.Board.PlaceBlock(at,new Block((BlockColor)((x+y)%2)));
            }
            Assert.That(p.FindMove(), Is.Null);Assert.That(p.EnsurePlayable(), Is.True);
            Assert.That(p.FindMove(),Is.Not.Null);Assert.That(p.Session.RemainingMoves,Is.EqualTo(s.moves));
            Assert.That(p.Board.GetObstacle(new GridPosition(2,2),ObstacleLayer.Blocking),Is.Not.Null);
        }
        [Test] public void PuzzleSequence_IsDeterministicIncludingSpecialCreation()
        {
            var stage=Pack().stages[0];var a=new HotelPuzzle(stage);var b=new HotelPuzzle(stage);
            for(int turn=0;turn<12&&!a.Cleared&&!a.Failed;turn++)
            {
                var move=a.FindMove().Value;a.Play(move);b.Play(move);
                Assert.That(a.Session.RemainingMoves,Is.EqualTo(b.Session.RemainingMoves));
                for(int y=0;y<stage.height;y++)for(int x=0;x<stage.width;x++)
                {
                    var at=new GridPosition(x,y);var first=a.Board.GetBlock(at);var second=b.Board.GetBlock(at);
                    Assert.That(first?.Color,Is.EqualTo(second?.Color));Assert.That(first?.Type,Is.EqualTo(second?.Type));
                }
            }
        }
    }
}
