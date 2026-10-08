using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private IEnumerator ReviewWorkshop()
        {
            string output = Path.Combine(Application.dataPath, "..", "WorkshopReview"); Directory.CreateDirectory(output);
            HotelCapture.PrepareHiddenPlayer(view);
            yield return null;
            string failure = null;
            Application.LogCallback errors = (message, stack, type) => { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) failure = message + "\n" + stack; };
            Application.logMessageReceived += errors;
            var run = ReviewWorkshopSteps(output);
            while (failure == null)
            {
                bool next;
                try { next = run.MoveNext(); }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!next) break;
                yield return run.Current;
            }
            Application.logMessageReceived -= errors;
            File.WriteAllText(Path.Combine(output, "result.txt"), failure == null ? "PASS: authored UI, paint/undo/redo, draft reload, trial cannot certify, legal clear replay, cosmetic rename, gameplay edit invalidation, ZIP export, MOD reload, isolated base save, fixed retry seed, exported puzzle clear.\n" : "FAIL: " + failure);
            Debug.Log(failure == null ? "HOTEL_WORKSHOP_PASSED" : "HOTEL_WORKSHOP_FAILED: " + failure);
            Application.Quit(failure == null ? 0 : 2);
        }
        private IEnumerator ReviewWorkshopSteps(string output)
        {
            ContentValidation.Require(view.workshop != null, "Workshop not authored");
            storage.Save(storage.NewProgress()); string originalSave = File.ReadAllText(storage.SavePath);
            PressForSmoke(view.workshopButton); yield return null;
            var controller = view.workshop; var w = controller.View;
            ContentValidation.Require(!w.export.interactable, "Unverified export must be locked");
            string initial = HotelPuzzleClearance.Fingerprint(controller.Draft.stage);
            PressForSmoke(w.brushes[2]); PressForSmoke(w.cells[24]);
            ContentValidation.Require(HotelPuzzleClearance.Fingerprint(controller.Draft.stage) != initial, "Paint did not change board");
            PressForSmoke(w.undo); ContentValidation.Require(HotelPuzzleClearance.Fingerprint(controller.Draft.stage) == initial, "Undo failed");
            PressForSmoke(w.redo); ContentValidation.Require(HotelPuzzleClearance.Fingerprint(controller.Draft.stage) != initial, "Redo failed");
            PressForSmoke(w.undo);
            w.fields[3].text = "30"; w.fields[5].text = "0"; w.fields[6].text = "3"; w.fields[7].text = "0"; w.fields[8].text = "0";
            w.fields[0].text = "제작실 검증 퍼즐";
            PressForSmoke(w.save); PressForSmoke(w.reload); yield return null;
            ContentValidation.Require(controller.Draft.stage.title == "제작실 검증 퍼즐", "Draft reload failed");
            HotelCapture.Save(view, Path.Combine(output, "01-editor.png"));
            PressForSmoke(w.test); yield return null;
            for (int i = 0; i < 30 && !controller.Play.Cleared && !controller.Play.Failed; i++)
            {
                puzzle = controller.Play; stage = controller.Draft.stage;
                var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No trial move");
                PressForSmoke(w.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X]); yield return null;
            }
            ContentValidation.Require(controller.Play.Cleared && controller.Draft.clearance == null, "Trial must not certify");
            HotelCapture.Save(view, Path.Combine(output, "02-trial.png")); PressForSmoke(w.stop);
            for (int pass = 0; pass < 2; pass++)
            {
                PressForSmoke(w.verify); yield return null;
                HotelCapture.Save(view, Path.Combine(output, "03-verification-board.png"));
                for (int i = 0; i < 30 && !controller.Play.Cleared && !controller.Play.Failed; i++)
                {
                    puzzle = controller.Play; stage = controller.Draft.stage;
                    var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No verification move");
                    PressForSmoke(w.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X]); yield return null;
                }
                ContentValidation.Require(controller.Play.Cleared && controller.Draft.clearance != null, "Verification did not certify legal win");
                HotelCapture.Save(view, Path.Combine(output, "04-certified-play.png")); PressForSmoke(w.stop);
                if (pass == 0)
                {
                    w.fields[0].text = "직접 클리어한 퍼즐";
                    ContentValidation.Require(w.export.interactable, "Cosmetic rename invalidated proof");
                    w.fields[6].text = "4";
                    ContentValidation.Require(!w.export.interactable && controller.Draft.clearance == null, "Goal edit kept stale proof");
                    PressForSmoke(w.save); PressForSmoke(w.reload);
                    ContentValidation.Require(controller.Draft.clearance == null, "Reload resurrected stale proof");
                    HotelCapture.Save(view, Path.Combine(output, "05-invalidated.png"));
                }
            }
            PressForSmoke(w.export); yield return null;
            ContentValidation.Require(File.ReadAllText(storage.SavePath) == originalSave, "Workshop modified campaign save");
            ContentValidation.Require(Directory.GetFiles(Path.Combine(storage.Root, "Workshop", "Exports"), "*.zip").Length == 1, "ZIP was not exported");
            var mod = storage.Mods.Single(m => m.Valid && m.manifest.id.StartsWith("workshop."));
            HotelCapture.Save(view, Path.Combine(output, "06-exported.png"));
            // Keep a reviewable exported sample outside the temporary QA save.
            File.Copy(Directory.GetFiles(Path.Combine(storage.Root, "Workshop", "Exports"), "*.zip").Single(), Path.Combine(output, "Verified-Puzzle-Sample.zip"), true);
            PressForSmoke(w.back); PressForSmoke(view.modsButton); yield return null;
            var row = view.catalogRows.First(b => b.gameObject.activeSelf && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("직접 클리어한 퍼즐"));
            PressForSmoke(row); ContentValidation.Require(storage.PackId == mod.manifest.id && !storage.HasSave, "Exported MOD save isolation failed");
            HotelCapture.Save(view, Path.Combine(output, "07-mod-selected.png"));
            PressForSmoke(view.catalogBack); PressForSmoke(view.practiceButton); PressForSmoke(view.catalogRows[0]); yield return null;
            ContentValidation.Require(stage.fixedSeed && stage.targetAmount == 4 && stage.title == "직접 클리어한 퍼즐", "Exported puzzle was not loaded");
            string firstBoard = RecordedBoardSignature();
            PressForSmoke(view.retryButton); PressForSmoke(view.resultPrimary);
            ContentValidation.Require(RecordedBoardSignature() == firstBoard, "Published retry changed starting board");
            HotelCapture.Save(view, Path.Combine(output, "08-published-puzzle.png"));
            for (int i = 0; i < 100 && !puzzle.Cleared && !puzzle.Failed; i++)
            {
                while (busy) yield return null;
                var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No published move");
                PressForSmoke(view.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X].button);
                while (busy) yield return null;
            }
            ContentValidation.Require(puzzle.Cleared, "Exported puzzle was not cleared in game");
            HotelCapture.Save(view, Path.Combine(output, "09-published-clear.png"));
        }
    }
}
