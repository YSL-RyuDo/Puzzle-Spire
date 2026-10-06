using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        // Continues the recorded first investigation. All progress and switches use
        // the same player-facing controls; no synthetic rewards or forced wins.
        private IEnumerator RecordThemeSwitches()
        {
            string saved = File.ReadAllText(storage.SavePath);
            foreach (string id in new[] { "moonlight.classic", "base" })
            {
                RecordClick(view.exploreTitleButton); yield return new WaitForSeconds(1.2f);
                RecordClick(view.modsButton); yield return new WaitForSeconds(2);
                Checkpoint("theme-menu-" + id, "MOD menu before selecting " + id);
                var button = view.catalogRows.First(b => b.gameObject.activeSelf && b.GetComponentInChildren<Text>().text.Contains(id == "base" ? "기본 이야기" : "클래식 일러스트"));
                RecordClick(button); yield return new WaitForSeconds(2);
                ContentValidation.Require(storage.PackId == id, "Theme menu did not apply " + id);
                ContentValidation.Require(File.ReadAllText(storage.SavePath) == saved, "Theme changed save file");
                Checkpoint("theme-selected-" + id, "Selected " + storage.PackName);
                RecordClick(view.catalogBack); yield return new WaitForSeconds(1.2f);
                RecordClick(view.continueButton); yield return new WaitForSeconds(2);
                ContentValidation.Require(story.Progress.completed.Count == 2 && story.Progress.completed.Contains("keys"), "Theme lost earned investigation progress");
                Checkpoint("theme-resumed-" + id, "Same two completed investigations under " + id);
                RecordClick(view.notebookButton); yield return new WaitForSeconds(2);
                RecordClick(view.clueButtons[0]); yield return new WaitForSeconds(3);
                Checkpoint("theme-evidence-" + id, "Earned thread illustration under " + id);
                RecordClick(view.collection.closeDetail); yield return new WaitForSeconds(.6f);
                RecordClick(view.notebookBack); yield return new WaitForSeconds(.6f);
                RecordClick(view.hotspot); yield return null;
                int linesSeen = 0;
                while (view.dialoguePanel.activeSelf || screen == "inspection" || busy)
                {
                    if (busy || view.stagePresentation.IsRevealing) { yield return null; continue; }
                    if (view.dialoguePanel.activeSelf)
                    {
                        Checkpoint("theme-dialogue-" + id + "-" + linesSeen++, "Story dialogue under " + id);
                        yield return new WaitForSeconds(1.8f); RecordClick(view.dialogueNext);
                    }
                    else if (screen == "inspection") { yield return new WaitForSeconds(2); RecordClick(Presentation.inspectionContinue); }
                    yield return null;
                }
                ContentValidation.Require(screen == "puzzle" && !practice, "Next investigation board not opened");
                yield return new WaitForSeconds(2.5f);
                Checkpoint("theme-board-" + id, "Playable story board under " + id);
                var move = puzzle.FindMove(); ContentValidation.Require(move.HasValue, "No legal theme demo move");
                RecordClick(view.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X].button); recordedMoves++;
                while (busy) yield return null;
                yield return new WaitForSeconds(1.8f);
                ContentValidation.Require(!puzzle.Cleared && !puzzle.Failed, "Demo must leave the next investigation unfinished");
                RecordClick(view.puzzleBackButton); yield return new WaitForSeconds(1.3f);
                RecordClick(view.resultPrimary); yield return new WaitForSeconds(1.2f);
                ContentValidation.Require(screen == "explore" && File.ReadAllText(storage.SavePath) == saved, "Leaving demo board changed earned progress");
                Checkpoint("theme-return-" + id, "Unfinished board exited; earned progress unchanged");
            }
            yield return new WaitForSeconds(2);
        }
    }
}
