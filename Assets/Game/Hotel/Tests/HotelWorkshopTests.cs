using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Hotel.Tests
{
    public sealed class HotelWorkshopTests
    {
        private string root;
        [SetUp] public void Setup() => root = Path.Combine(Path.GetTempPath(), "HotelWorkshopTests-" + Guid.NewGuid().ToString("N"));
        [TearDown] public void Cleanup() { if (Directory.Exists(root)) Directory.Delete(root, true); }
        private static HotelStage Stage() => new HotelStage { id = "stage_01", title = "Test", width = 3, height = 3, moves = 20, colors = 2, seed = 1001, targetColor = 0, targetAmount = 1, fixedSeed = true, layout = new[] { "OOO", "OOO", "OOO" } };
        private static PuzzleClearance Solve(HotelStage stage)
        {
            var puzzle = new HotelPuzzle(stage); var clicks = new System.Collections.Generic.List<int>();
            while (!puzzle.Cleared && !puzzle.Failed)
            {
                var move = puzzle.FindMove().Value;
                clicks.Add((stage.height - 1 - move.Y) * stage.width + move.X); puzzle.Play(move);
            }
            Assert.That(puzzle.Cleared, Is.True, "Fixture must win through legal moves");
            return HotelPuzzleClearance.Certify(stage, clicks);
        }
        [Test] public void WinningReplayVerifiesAfterSerialization()
        {
            var stage = Stage(); var proof = Solve(stage);
            Assert.DoesNotThrow(() => HotelPuzzleClearance.Verify(stage, JsonUtility.FromJson<PuzzleClearance>(JsonUtility.ToJson(proof))));
        }
        [Test] public void DifferentWinningInputsAreAcceptedForTheSamePuzzle()
        {
            var s = Stage(); s.width = s.height = 5; s.layout = Enumerable.Repeat("OOOOO", 5).ToArray();
            int solutions = 0;
            for (int index = 0; index < 25; index++)
            {
                var puzzle = new HotelPuzzle(s);
                puzzle.Play(new GridPosition(index % 5, 4 - index / 5));
                if (!puzzle.Cleared) continue;
                Assert.DoesNotThrow(() => HotelPuzzleClearance.Certify(s, new[] { index })); solutions++;
            }
            Assert.That(solutions, Is.GreaterThan(1), "There is no prescribed click sequence for a published puzzle");
        }
        [Test] public void TitleChangePreservesProofButRuleChangeDoesNot()
        {
            var s = Stage(); var p = Solve(s); s.title = "Renamed";
            Assert.DoesNotThrow(() => HotelPuzzleClearance.Verify(s, p));
            s.moves++;
            Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.Verify(s, p));
        }
        [Test] public void EveryGameplaySettingAffectsFingerprint()
        {
            var original = Stage(); string hash = HotelPuzzleClearance.Fingerprint(original);
            Action<HotelStage>[] changes = { s => s.width++, s => s.height++, s => s.moves++, s => s.seed++, s => s.colors++, s => s.targetColor++, s => s.targetAmount++, s => s.boxes++, s => s.ice++, s => s.fixedSeed = false, s => s.layout[0] = "XOO", s => s.obstacles = new[] { new ObstacleData { x = 1, y = 1, type = "ice" } } };
            foreach (var change in changes) { var s = HotelPuzzleClearance.Clone(original); change(s); Assert.That(HotelPuzzleClearance.Fingerprint(s), Is.Not.EqualTo(hash)); }
        }
        [Test] public void EmptyInvalidAndPostWinClicksAreRejected()
        {
            var s = Stage(); var p = Solve(s);
            p.clicks = Array.Empty<int>(); Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.Verify(s, p));
            p.clicks = new[] { 999 }; Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.Verify(s, p));
            p = Solve(s); p.clicks = p.clicks.Concat(new[] { 0 }).ToArray(); Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.Verify(s, p));
        }
        [Test] public void IncompleteReplayDoesNotCountAsClear()
        {
            var s = Stage(); s.targetAmount = 999;
            var p = new HotelPuzzle(s).FindMove().Value;
            Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.Certify(s, new[] { (s.height - 1 - p.Y) * s.width + p.X }));
        }
        [Test] public void FixedSeedProducesSameBoardOnRetry()
        {
            var s = Stage(); var first = new HotelPuzzle(s); var retry = new HotelPuzzle(s, 19);
            for (int y = 0; y < s.height; y++) for (int x = 0; x < s.width; x++)
                Assert.That(retry.Board.GetBlock(new GridPosition(x, y)).Color, Is.EqualTo(first.Board.GetBlock(new GridPosition(x, y)).Color));
        }
        [Test] public void DraftRoundTripDropsStaleProof()
        {
            var s = Stage(); var d = new PuzzleWorkshopDraft { stage = s, clearance = Solve(s) };
            string path = Path.Combine(root, "draft.json"); HotelWorkshopFiles.SaveDraft(path, d);
            Assert.That(HotelWorkshopFiles.LoadDraft(path, s.id).clearance, Is.Not.Null);
            s.seed++; HotelWorkshopFiles.SaveDraft(path, d);
            Assert.That(HotelWorkshopFiles.LoadDraft(path, s.id).clearance, Is.Null);
        }
        [Test] public void AllExportedStagesNeedUniqueProofs()
        {
            var a = Stage(); var b = Stage(); b.id = "stage_02";
            var pack = new HotelPack { stages = new[] { a, b } };
            Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.VerifyPack(pack, new PuzzleClearanceFile { puzzles = new[] { Solve(a) } }));
            var proof = Solve(a);
            Assert.Throws<InvalidOperationException>(() => HotelPuzzleClearance.VerifyPack(pack, new PuzzleClearanceFile { puzzles = new[] { proof, proof } }));
        }
        [Test] public void ExportRequiresClearanceAndLoadsAsMod()
        {
            using (var storage = new HotelStorage("Assets/StreamingAssets/Hotel/base.json", root))
            {
                var s = Stage(); var draft = new PuzzleWorkshopDraft { stage = s };
                Assert.Throws<InvalidOperationException>(() => HotelWorkshopFiles.Export(storage, draft));
                Assert.That(Directory.GetDirectories(storage.ModsPath).Length, Is.Zero);
                draft.clearance = Solve(s); string zip = HotelWorkshopFiles.Export(storage, draft);
                using (var archive = ZipFile.OpenRead(zip)) Assert.That(archive.Entries.Count, Is.EqualTo(4));
                Assert.That(storage.Mods.Single().Valid, Is.True);
                Assert.That(storage.TryActivate(storage.Mods.Single().manifest.id, out _), Is.True);
                Assert.That(storage.Pack.stages.First().fixedSeed, Is.True);
                Assert.That(storage.Pack.stages.First().width, Is.EqualTo(3));
            }
        }
        [Test] public void EditedExportFailsReplayValidationOnImport()
        {
            using (var storage = new HotelStorage("Assets/StreamingAssets/Hotel/base.json", root))
            {
                var s = Stage(); HotelWorkshopFiles.Export(storage, new PuzzleWorkshopDraft { stage = s, clearance = Solve(s) });
                string path = Path.Combine(storage.Mods.Single().directory, "content.json");
                var pack = HotelStorage.ReadJson<HotelPack>(path); pack.stages[0].seed++;
                File.WriteAllText(path, JsonUtility.ToJson(pack)); storage.RefreshMods();
                Assert.That(storage.Mods.Single().Valid, Is.False);
            }
        }
    }
}
