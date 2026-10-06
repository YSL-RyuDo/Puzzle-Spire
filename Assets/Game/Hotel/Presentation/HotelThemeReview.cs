using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private IEnumerator ReviewThemes()
        {
            string output = Path.Combine(Application.dataPath, "..", "ThemeReview");
            Directory.CreateDirectory(output);
            HotelCapture.PrepareHiddenPlayer(view);
            ContentValidation.Require(storage.Mods.Any(m => m.Valid && m.manifest.id == "moonlight.classic"), "Bundled classic theme missing");
            NewGame();
            while (story.Next != null) ContentValidation.Require(story.Complete(story.Next.id), "Fixture progression failed");
            storage.Save(story.Progress);
            string originalSave = File.ReadAllText(storage.SavePath);
            foreach (string theme in new[] { "base", "moonlight.classic", "base" })
            {
                ShowCatalog("mods");
                var button = view.catalogRows.First(b => b.gameObject.activeSelf && b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains(theme == "base" ? "기본 이야기" : "클래식 일러스트"));
                PressForSmoke(button);
                ContentValidation.Require(storage.PackId == theme, "MOD menu did not apply selection");
                ContentValidation.Require(File.ReadAllText(storage.SavePath) == originalSave, "Theme modified or lost base progress");
                ContinueGame();
                ContentValidation.Require(story.Progress.ending && story.Progress.completed.Count == storage.Pack.nodes.Length, "Theme did not retain ending progress");
                ShowNotebook(true); yield return null; yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-collection.png"));
                PressForSmoke(view.clueButtons[0]);
                ContentValidation.Require(Presentation.notebookImage.sprite == (theme == "base" ? Presentation.Evidence("thread") : storage.Image("clue.thread")), "Evidence theme mismatch");
                yield return null; yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-detail.png"));
                PressForSmoke(view.collection.closeDetail);
                story.Progress.location = "lobby"; ShowExplore();
                ShowDialogue(new[] {
                    new DialogueLine { speaker = "you", text = "빈 열쇠 자리 옆에 실이 걸려 있어요. 누군가 서둘러 가져간 걸까요?", pose = "curious" },
                    new DialogueLine { speaker = "front", text = "아까 수건을 정리했는데… 그때까지는 분명 있었어요.", pose = "worried" }
                }, ShowExplore);
                view.stagePresentation.CompleteText();
                ContentValidation.Require(view.speakerName.text == storage.Pack.characters.First(c => c.id == "you").name, "Protagonist nameplate is stale");
                yield return new WaitForSecondsRealtime(.5f); yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-dialogue.png"));
                NextDialogue(); view.stagePresentation.CompleteText();
                ContentValidation.Require(view.speakerName.text == storage.Pack.characters.First(c => c.id == "front").name, "Partner nameplate is stale");
                yield return new WaitForSecondsRealtime(.25f); yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-partner.png"));
                while (view.dialoguePanel.activeSelf)
                { view.stagePresentation.CompleteText(); NextDialogue(); yield return null; }
                while (busy) yield return null;
                activeNode = storage.Pack.nodes.First(n => n.inspection == "cabinet");
                ShowInspection(activeNode, false, ShowExplore);
                ContentValidation.Require(Presentation.inspectionImage.sprite == ThemeCabinet, "Cabinet theme mismatch");
                yield return null; yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-inspection.png"));
                ShowExplore(); StartPuzzle(storage.Pack.stages.First(), false, false);
                for (int i = 5; i < 13; i++) ContentValidation.Require(ThemePiece(i) == (theme == "base" ? Presentation.Piece(i) : storage.Image(HotelThemeKeys.Pieces[i])), "Special/obstacle theme mismatch");
                ContentValidation.Require(Presentation.puzzleScene.sprite == ThemeCabinet, "Puzzle context theme mismatch");
                yield return null; yield return new WaitForEndOfFrame();
                HotelCapture.Save(view, Path.Combine(output, theme + "-puzzle.png"));
                foreach (var binding in GetComponent<HotelThemeView>().backgrounds)
                {
                    if (binding.image == Presentation.puzzleScene || binding.image == Presentation.inspectionImage) continue;
                    ContentValidation.Require(binding.image.sprite == (storage.Image(binding.key) ?? binding.defaultSprite), "Authored background binding mismatch");
                }
            }
            File.WriteAllText(Path.Combine(output, "result.txt"), "PASS: real MOD menu base > classic > base; base save byte-for-byte preserved; ending continue; portraits, inspection, collection and puzzle renders; special/obstacle and static backdrop overrides. Synthetic completed-story fixture, not a campaign recording.");
            Debug.Log("HOTEL_THEME_REVIEW_PASSED"); Application.Quit(0);
        }
    }
}
