using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private HotelStorage studioOriginalStorage;
        private void OpenStoryStudio()
        {
            screen = "studio"; view.Show(view.storyStudio.View.screen);
            view.storyStudio.Open(storage, StudioDefaultImage, StartStudioPreview, EditStoryPuzzle, ShowTitle);
        }
        private Sprite StudioDefaultImage(string key)
        {
            if (string.IsNullOrEmpty(key)) return defaultBackgrounds[0];
            if (key.StartsWith("background."))
            {
                int i = Array.FindIndex(storage.BasePack.locations, l => l.image == key || "background." + l.id == key);
                return defaultBackgrounds[Math.Max(0, i) % defaultBackgrounds.Length];
            }
            if (key.StartsWith("character."))
            {
                var parts = key.Split('.'); int i = Array.FindIndex(storage.BasePack.characters, c => c.id == parts[1]);
                return (parts.Length > 2 ? Presentation.Pose(parts[1], parts[2]) : null) ?? defaultPortraits[Math.Max(0, i) % defaultPortraits.Length];
            }
            int piece = Array.IndexOf(HotelThemeKeys.Pieces, key);
            return piece >= 0 ? (piece < 5 ? defaultBlockSprites[piece] : Presentation.Piece(piece)) : view.collection.fallback;
        }
        private void EditStoryPuzzle(HotelStage source, PuzzleClearance proof, string folder, Action<PuzzleWorkshopDraft> saved)
        {
            screen = "workshop"; view.Show(view.workshop.View.screen);
            var draft = new PuzzleWorkshopDraft { stage = HotelPuzzleClearance.Clone(source), clearance = proof == null ? null : HotelStoryProjects.Copy(proof) };
            view.workshop.OpenProject(storage, Path.Combine(folder, "Puzzles", source.id + ".json"), draft, i => i < 5 ? defaultBlockSprites[i] : Presentation.Piece(i), saved, OpenStoryStudio);
        }
        private void StartStudioPreview(string node)
        {
            ContentValidation.Require(studioOriginalStorage == null, "이미 이야기를 시험 중입니다.");
            var studio = view.storyStudio;
            HotelStoryProjects.Validate(studio.Project, studio.ProjectFolder, false);
            var preview = new HotelStorage(Path.Combine(Application.streamingAssetsPath, "Hotel", "base.json"), Path.Combine(Path.GetTempPath(), "PuzzleSpire-StoryPreview-" + Guid.NewGuid().ToString("N")));
            try
            {
                string id = HotelStoryProjects.Install(studio.Project, studio.ProjectFolder, preview, false);
                ContentValidation.Require(preview.TryActivate(id, out var error), error);
            }
            catch { preview.Dispose(); throw; }
            studioOriginalStorage = storage; storage = preview; view.stagePresentation.modPortraitMirror = storage.DialogueMirror;
            GetComponent<HotelThemeView>()?.Apply(storage);
            view.studioReturnButton.gameObject.SetActive(true);
            NewGame();
            if (!string.IsNullOrEmpty(node))
            {
                var selectedNode = storage.Pack.nodes.First(n => n.id == node);
                while (!story.IsAvailable(selectedNode) && story.Next != null && !story.Progress.ending) story.Complete(story.Next.id);
                ContentValidation.Require(story.IsAvailable(selectedNode), "선택한 조사에 도달할 수 없습니다.");
                story.Progress.location = selectedNode.location; roomPage = Array.FindIndex(storage.Pack.locations, l => l.id == selectedNode.location) / view.locationButtons.Length;
                ShowExplore(); BeginNode(selectedNode);
            }
        }
        private void EndStudioPreview()
        {
            if (studioOriginalStorage == null) return;
            view.stagePresentation.CancelDialogue(); storage.Dispose(); storage = studioOriginalStorage; studioOriginalStorage = null;
            view.stagePresentation.modPortraitMirror = storage.DialogueMirror;
            GetComponent<HotelThemeView>()?.Apply(storage); story = null; puzzle = null; activeNode = null; busy = false;
            view.studioReturnButton.gameObject.SetActive(false); OpenStoryStudio();
        }
    }
}
