using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private IEnumerator ReviewStudio()
        {
            string output = Path.Combine(Application.dataPath, "..", "StoryStudioReview"); Directory.CreateDirectory(output);
            HotelCapture.PrepareHiddenPlayer(view); yield return null;
            string failure = null;
            Application.LogCallback errors = (message, stack, type) => { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) failure = message + "\n" + stack; };
            Application.logMessageReceived += errors;
            var run = ReviewStudioSteps(output);
            while (failure == null)
            {
                bool next; try { next = run.MoveNext(); } catch (Exception e) { failure = e.ToString(); break; }
                if (!next) break; yield return run.Current;
            }
            Application.logMessageReceived -= errors;
            File.WriteAllText(Path.Combine(output, "result.txt"), failure == null ? "PASS: authored UI, independent story, four locations, two chapters, investigation and clue, before/after dialogue, PNG import/crop, certified puzzle, riddle, isolated preview to ending, ZIP export and adventure reload.\n" : "FAIL: " + failure);
            Debug.Log(failure == null ? "HOTEL_STUDIO_PASSED" : "HOTEL_STUDIO_FAILED: " + failure); Application.Quit(failure == null ? 0 : 2);
        }
        private IEnumerator ReviewStudioSteps(string output)
        {
            storage.Save(storage.NewProgress()); string originalSave = File.ReadAllText(storage.SavePath);
            PressForSmoke(view.storyStudioButton); yield return null;
            var controller = view.storyStudio; var w = controller.View;
            void Tab(int i) => PressForSmoke(w.tabs[i]);
            void Save() => PressForSmoke(w.save);
            void Choose(int field, int value) { PressForSmoke(w.fieldPickers[field]); PressForSmoke(w.pickerRows[value]); }
            void Capture(string name) => HotelCapture.Save(view, Path.Combine(output, name + ".png"));
            var args = Environment.GetCommandLineArgs(); int artArg = Array.IndexOf(args, "-hotel-studio-art");
            string art = artArg >= 0 ? args[artArg + 1] : Path.Combine(Application.streamingAssetsPath, "Hotel/Themes/ClassicIllustrated/Images");
            w.fields[0].text = "비 오는 역의 마지막 편지"; w.fields[2].text = "두 번째 장의 끝";
            w.longText.text = "마지막 편지는 주인에게 돌아갔다. 이제 새로운 이야기가 시작된다."; Save(); Capture("01-project");
            Tab(1); w.fields[0].text = "역 대합실"; Save(); PressForSmoke(w.special);
            w.fields[1].text = Path.Combine(art, "Corridor-PC.png"); PressForSmoke(w.importImage);
            ContentValidation.Require(controller.Project.images.Length == 1, "Background import failed: " + w.status.text); Capture("02-background");
            Tab(1);
            for (int i = 0; i < 3; i++) { PressForSmoke(w.add); w.fields[0].text = new[] { "매표소", "승강장", "보관실" }[i]; Save(); }
            Tab(2); w.fields[0].text = "편지 조사원"; Save(); PressForSmoke(w.special);
            w.fields[1].text = Path.Combine(art, "Characters-PC.png"); PressForSmoke(w.importImage);
            w.fields[4].text = "543"; w.fields[5].text = "724"; Save();
            ContentValidation.Require(controller.Project.images.Last().width == 543, "Portrait crop failed"); Capture("03-portrait");
            Tab(3); PressForSmoke(w.add); w.fields[0].text = "오래된 편지"; w.longText.text = "받는 사람의 이름이 지워진 편지. 보관함의 단서다."; Save();
            Tab(4); PressForSmoke(w.add); w.fields[0].text = "잠긴 우편함"; w.fields[1].text = "CHAPTER 2 · 마지막 편지"; Save();
            Choose(2, 3); Choose(3, 1); Choose(6, 1);
            w.fields[7].text = "62"; w.fields[8].text = "48"; Save(); Capture("04-investigation");
            PressForSmoke(w.dialogue); PressForSmoke(w.add); w.longText.text = "우편함 안에 마지막 편지가 있어. 같은 블록을 모아 열어 보자."; Save();
            PressForSmoke(w.textMode); PressForSmoke(w.add); w.longText.text = "찾았다! 이제 편지의 수신인을 확인하면 돼."; Save(); Capture("05-dialogue");
            Tab(4); PressForSmoke(w.special); yield return null;
            ContentValidation.Require(screen == "workshop", "Linked puzzle editor did not open");
            var workshop = view.workshop; var pw = workshop.View;
            pw.fields[0].text = "우편함 열기"; pw.fields[3].text = "30"; pw.fields[6].text = "3"; PressForSmoke(pw.save); PressForSmoke(pw.verify);
            for (int i = 0; i < 30 && !workshop.Play.Cleared && !workshop.Play.Failed; i++)
            {
                puzzle = workshop.Play; stage = workshop.Draft.stage; var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No puzzle move");
                PressForSmoke(pw.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X]); yield return null;
            }
            ContentValidation.Require(workshop.Draft.clearance != null, "Story puzzle was not certified"); Capture("06-certified-puzzle");
            PressForSmoke(pw.stop); PressForSmoke(pw.back); yield return null;
            ContentValidation.Require(controller.Project.proofs.Length == 1, "Story did not receive puzzle certificate");
            PressForSmoke(w.add); w.fields[0].text = "편지의 수신인"; Save(); Choose(3, 2); PressForSmoke(w.special);
            w.fields[0].text = "편지를 기다리는 사람"; w.fields[2].text = "2"; w.fields[3].text = "매표원"; w.fields[4].text = "역장";
            w.longText.text = "편지에는 ‘마지막 열차를 지키는 사람에게’라고 적혀 있다. 누구의 편지일까?"; Save(); Capture("07-riddle");
            Tab(4); PressForSmoke(w.chain); Save();
            ContentValidation.Require(controller.Project.pack.nodes.Length == 4, "Incorrect story graph");
            HotelStoryProjects.Validate(controller.Project, controller.ProjectFolder, true);
            PressForSmoke(w.trial); yield return null;
            ContentValidation.Require(studioOriginalStorage != null && storage.Pack.nodes.Length == 4, "Preview did not replace base story");
            // Exercise the fourth room using actual paged location controls.
            PressForSmoke(view.roomNext); PressForSmoke(view.locationButtons[0]);
            while (busy) yield return null;
            ContentValidation.Require(story.Progress.location == controller.Project.pack.locations[3].id, "Fourth room navigation failed");
            MoveLocation(controller.Project.pack.locations[0].id);
            while (busy) yield return null;
            int safety = 0; double deadline = Time.realtimeSinceStartupAsDouble + 100;
            while (!story.Progress.ending && safety < 3000 && Time.realtimeSinceStartupAsDouble < deadline)
            {
                yield return null; if (busy) continue; safety++;
                if (view.dialoguePanel.activeSelf) { yield return new WaitForSecondsRealtime(.25f); Capture("08-play-dialogue"); PressForSmoke(view.dialogueNext); }
                else if (view.resultPanel.activeSelf) PressForSmoke(view.resultPrimary);
                else if (screen == "inspection") PressForSmoke(Presentation.inspectionContinue);
                else if (screen == "explore") { var next = story.Next; ContentValidation.Require(next != null, "No next investigation"); if (story.Progress.location != next.location) MoveLocation(next.location); else PressForSmoke(view.hotspot); }
                else if (screen == "puzzle") { var best = BestSmokeMove(); ContentValidation.Require(best.HasValue, "No play move"); PressForSmoke(view.cells[(stage.height - 1 - best.Value.Y) * stage.width + best.Value.X].button); }
                else if (screen == "riddle") { foreach (int answer in riddle.answer) PressForSmoke(view.options[answer]); PressForSmoke(view.submitAnswer); }
            }
            ContentValidation.Require(story.Progress.ending && story.Progress.completed.Count == 4 && story.Progress.clues.Count == 1, "Custom story did not reach complete ending: " + screen + "/" + activeNode?.id + " completed=" + story.Progress.completed.Count + " busy=" + busy + " clicks=" + safety);
            ContentValidation.Require(view.endingHeading.text == "두 번째 장의 끝", "Custom ending title missing"); Capture("09-play-ending");
            PressForSmoke(view.studioReturnButton); yield return null;
            ContentValidation.Require(studioOriginalStorage == null && File.ReadAllText(storage.SavePath) == originalSave, "Preview changed real save");
            PressForSmoke(w.export); yield return null;
            var mod = storage.Mods.Single(m => m.Valid && m.manifest.id.StartsWith("adventure."));
            string zip = Directory.GetFiles(Path.Combine(storage.Root, "StoryStudio/Exports"), "*.zip").Single();
            File.Copy(zip, Path.Combine(output, "New-Story-Sample-MOD.zip"), true); Capture("10-exported");
            PressForSmoke(w.back); PressForSmoke(view.modsButton);
            PressForSmoke(view.catalogRows.First(b => b.gameObject.activeSelf && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(mod.manifest.name)));
            ContentValidation.Require(storage.PackId == mod.manifest.id && storage.Pack.nodes.Length == 4 && storage.Pack.locations.Length == 4 && storage.Pack.stages.Length == 1, "Exported story reload failed");
            HotelPuzzleClearance.VerifyPack(storage.Pack, HotelStorage.ReadJson<PuzzleClearanceFile>(Path.Combine(storage.ModsPath, mod.manifest.id, "clearance.json")));
            Capture("11-mod-selected");
        }
    }
}
