using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        // Explicit UI fixtures, not a campaign playthrough. Uses an isolated temporary save.
        private IEnumerator ReviewCollection()
        {
            string output=Path.Combine(Application.dataPath,"..","CollectionReview",Screen.width+"x"+Screen.height);
            Directory.CreateDirectory(output);HotelCapture.PrepareHiddenPlayer(view);
            story=new InvestigationSession(storage.Pack,storage.NewProgress());ShowNotebook(false);
            yield return null;yield return new WaitForEndOfFrame();
            HotelCapture.Save(view,Path.Combine(output,"01-empty.png"));
            ContentValidation.Require(view.clueButtons.All(b=>!b.gameObject.activeSelf || !b.interactable),"Empty collection exposes clickable evidence");
            ContentValidation.Require(view.collection.cardImages.All(i=>!i.gameObject.activeSelf),"Empty collection leaks evidence artwork");
            ContentValidation.Require(view.collection.cardTitles.All(t=>t.text=="아직 발견하지 못한 기록"),"Empty collection leaks titles");
            while(!story.Progress.completed.Contains("keys"))ContentValidation.Require(story.Complete(story.Next.id),"Fixture completion failed");
            storage.Save(story.Progress);story=new InvestigationSession(storage.Pack,storage.Load(out _));ShowNotebook(false);
            yield return null;yield return new WaitForEndOfFrame();
            HotelCapture.Save(view,Path.Combine(output,"02-first-record.png"));
            PressForSmoke(view.clueButtons[0]);yield return null;yield return new WaitForEndOfFrame();
            ContentValidation.Require(Presentation.notebookImage.sprite==Presentation.Evidence("thread"),"Original thread illustration was not preserved");
            HotelCapture.Save(view,Path.Combine(output,"03-thread-detail.png"));PressForSmoke(view.collection.closeDetail);
            while(story.Next!=null)ContentValidation.Require(story.Complete(story.Next.id),"Fixture completion failed");
            ShowNotebook(true);yield return null;yield return new WaitForEndOfFrame();
            HotelCapture.Save(view,Path.Combine(output,"04-full-collection.png"));
            for(int i=0;i<view.clueButtons.Length;i++)
            {
                if(!view.clueButtons[i].gameObject.activeSelf)continue;
                PressForSmoke(view.clueButtons[i]);yield return null;yield return new WaitForEndOfFrame();
                ContentValidation.Require(Presentation.notebookImage.sprite!=null,"Missing collected illustration "+i);
                HotelCapture.Save(view,Path.Combine(output,"detail-"+i+".png"));PressForSmoke(view.collection.closeDetail);
            }
            PressForSmoke(view.notebookBack);ContentValidation.Require(view.endingScreen.activeSelf,"Ending return broken");
            // Three extra MOD-like records: 12 entries must remain reachable in 9 authored slots.
            var extra=Enumerable.Range(0,3).Select(i=>new ClueData{id="review_"+i,title="추가 기록 "+i,description="MOD 추가 기록의 페이지 표시 확인"}).ToArray();
            storage.Pack.clues=storage.Pack.clues.Concat(extra).ToArray();story.Progress.clues.AddRange(extra.Select(c=>c.id));
            ShowNotebook(true);yield return null;PressForSmoke(view.collection.nextPage);yield return null;yield return new WaitForEndOfFrame();
            ContentValidation.Require(view.clueButtons.Count(b=>b.gameObject.activeSelf)==3,"12-record pagination failed");
            HotelCapture.Save(view,Path.Combine(output,"05-extra-page.png"));
            PressForSmoke(view.clueButtons[2]);ContentValidation.Require(Presentation.notebookImage.sprite==view.collection.fallback,"Generic MOD fallback missing");
            PressForSmoke(view.collection.closeDetail);PressForSmoke(view.collection.previousPage);
            string mod=Path.Combine(storage.ModsPath,"collection-review");Directory.CreateDirectory(mod);
            File.Copy(Path.Combine(Application.streamingAssetsPath,"Hotel","ExampleMod","red-block.png"),Path.Combine(mod,"override.png"));
            File.WriteAllText(Path.Combine(mod,"content.json"),JsonUtility.ToJson(new HotelPack()));
            File.WriteAllText(Path.Combine(mod,"manifest.json"),JsonUtility.ToJson(new ModManifest
            {id="collection.review",name="Collection QA",version="1.0.0",author="QA",description="Isolated asset priority test",contentFile="content.json",images=new[]{new ImageOverride{key="clue.observation.keys",file="override.png"}}}));
            storage.RefreshMods();ContentValidation.Require(storage.TryActivate("collection.review",out var error),error);
            story=new InvestigationSession(storage.Pack,storage.NewProgress());
            while(!story.Progress.completed.Contains("keys"))story.Complete(story.Next.id);
            ShowNotebook(false);yield return null;PressForSmoke(view.clueButtons[0]);
            ContentValidation.Require(Presentation.notebookImage.sprite==storage.Image("clue.observation.keys"),"MOD record image did not override built-in evidence");
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: empty spoiler protection, saved observation, 9 illustrations and details, return, 12-record pagination, generic fallback, MOD image priority. UI fixture review, not campaign playthrough.");
            Debug.Log("HOTEL_COLLECTION_REVIEW_PASSED");Application.Quit(0);
        }
    }
}
