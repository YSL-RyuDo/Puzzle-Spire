using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private HotelVideoWriter video;
        private string recordingFolder, recordingFailure;
        private bool recordingDone;
        private Coroutine recordedPlay;
        private int recordedClicks, recordedMoves, recordedRetries;
        private readonly HashSet<string> recordedStages=new HashSet<string>();
        private readonly HashSet<string> recordedRiddles=new HashSet<string>();
        private readonly HashSet<string> recordedLines=new HashSet<string>();
        private readonly HashSet<string> recordedScreens=new HashSet<string>();

        private void BeginFullPlayRecording()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-hotel-encoder");
            if(index<0 || index+1>=args.Length)throw new ArgumentException("-hotel-encoder path is required");
            recordingFolder=Path.Combine(Application.dataPath,"..",Environment.GetCommandLineArgs().Contains("-hotel-first-slice") ? "FirstInvestigation" : "FullPlay");Directory.CreateDirectory(recordingFolder);
            Directory.CreateDirectory(Path.Combine(recordingFolder,"Checkpoints"));
            File.WriteAllText(Path.Combine(recordingFolder,"timeline.tsv"),"seconds\tevent\n");
            File.WriteAllText(Path.Combine(recordingFolder,"encoder.log"),"");
            storage.Preferences.sound=false; // No fabricated audio in the video.
            Time.captureFramerate=HotelVideoWriter.Fps;HotelPresentationClock.Recording=true;
            Application.logMessageReceived+=RecordingLog;
            video=new HotelVideoWriter(view,args[index+1],Path.Combine(recordingFolder,"MoonlitHotel-FullPlay.mp4"));
            StartCoroutine(CaptureFullPlay());recordedPlay=StartCoroutine(PlayFullCampaign());
        }
        private void RecordingLog(string message,string stack,LogType type)
        {
            if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert)
                recordingFailure=message+"\n"+stack;
        }
        private void Timeline(string message)
        {
            File.AppendAllText(Path.Combine(recordingFolder,"timeline.tsv"),
                (video.Frames/(double)HotelVideoWriter.Fps).ToString("F3",CultureInfo.InvariantCulture)+"\t"+message.Replace('\n',' ').Replace('\t',' ')+"\n");
        }
        private void RecordClick(Button button)
        {
            Timeline("click: "+button.name);recordedClicks++;PressForSmoke(button);
        }
        private void Checkpoint(string key,string description)
        {
            if(!recordedScreens.Add(key))return;
            Timeline(description);
            HotelCapture.Save(view,Path.Combine(recordingFolder,"Checkpoints",key+".png"));
            Debug.Log("HOTEL_RECORD_PROGRESS: "+description+" | "+story?.Progress.completed.Count+"/20 | "+video.Frames/HotelVideoWriter.Fps+"s");
        }
        private IEnumerator CaptureFullPlay()
        {
            while(!recordingDone && recordingFailure==null)
            {
                yield return new WaitForEndOfFrame();
                try { video.Capture(); }
                catch(Exception e){recordingFailure=e.ToString();}
            }
            if(recordedPlay!=null)StopCoroutine(recordedPlay);
            Application.logMessageReceived-=RecordingLog;
            try { video.Dispose(); }
            catch(Exception e){recordingFailure=e.ToString();}
            Time.captureFramerate=0;HotelPresentationClock.Recording=false;
            bool firstSlice = Environment.GetCommandLineArgs().Contains("-hotel-first-slice");
            if(recordingFailure==null && !firstSlice)
            {
                try { AuditModAndPractice(); }
                catch(Exception e){recordingFailure=e.ToString();}
            }
            string report=recordingFailure==null
                ? "PASS\nInvestigations: "+story.Progress.completed.Count+"\nStages completed: "+recordedStages.Count+
                  "\nRiddles completed: "+recordedRiddles.Count+"\nDialogue lines shown: "+recordedLines.Count+
                  "\nLegal board moves: "+recordedMoves+"\nRetries: "+recordedRetries+"\nVerified UI clicks: "+recordedClicks+
                  "\nDuration seconds: "+(video.Frames/(double)HotelVideoWriter.Fps).ToString("F3",CultureInfo.InvariantCulture)+
                  (firstSlice ? "\nFirst investigation, discovery, illustrated notebook and save/resume: PASS\n" : "\nSave/resume, all clues, wrong answers, hints, practice exit, MOD isolation: PASS\n") + "Video: 24 fps, silent, uninterrupted automated UI play; no forced wins.\n"
                : "FAIL\n"+recordingFailure;
            File.WriteAllText(Path.Combine(recordingFolder,"verification.txt"),report);
            Debug.Log(recordingFailure==null ? "HOTEL_FULL_PLAY_PASSED\n"+report : "HOTEL_FULL_PLAY_FAILED\n"+report);
            Application.Quit(recordingFailure==null?0:2);
        }
        private IEnumerator PlayFullCampaign()
        {
            bool firstSlice = Environment.GetCommandLineArgs().Contains("-hotel-first-slice");
            yield return new WaitForSeconds(2);
            Checkpoint("00-title","New game");RecordClick(view.newButton);
            float deadline=Time.time+1800;int actions=0;
            while(!story.Progress.ending && Time.time<deadline && actions<4000)
            {
                yield return null;
                // The final dialogue's exit coroutine may finish during this yield.
                if(story.Progress.ending || (firstSlice && story.Progress.completed.Contains("keys")))break;
                if(busy)continue;
                if(view.dialoguePanel.activeSelf)
                {
                    while(view.stagePresentation.IsRevealing)yield return null;
                    string id=activeNode.id+"-"+linePhase()+"-"+dialogueIndex;
                    recordedLines.Add(id);
                    Checkpoint("dialogue-"+id,"Dialogue: "+activeNode.id+" / "+view.speakerName.text+" / "+view.dialogueText.text);
                    yield return new WaitForSeconds(Mathf.Clamp(view.dialogueText.text.Length/18f,1.4f,4.5f));
                    RecordClick(view.dialogueNext);actions++;continue;
                }
                if(view.resultPanel.activeSelf)
                {
                    if(screen=="puzzle" && puzzle!=null)
                    {
                        if(puzzle.Cleared)recordedStages.Add(stage.id);
                        if(puzzle.Failed){recordedRetries++;ContentValidation.Require(attempt<12,"Excessive retries: "+stage.id);}
                    }
                    Checkpoint("result-"+(activeNode?.id??"none")+"-"+attempt,"Result: "+view.resultTitle.text);
                    yield return new WaitForSeconds(Mathf.Clamp(view.resultBody.text.Length/22f,2,5));
                    RecordClick(view.resultPrimary);actions++;continue;
                }
                if(screen=="inspection")
                {
                    Checkpoint("inspection-"+activeNode.id+"-"+Presentation.discoveryCard.activeSelf,"Inspect: "+Presentation.inspectionTitle.text);
                    yield return new WaitForSeconds(3);
                    RecordClick(Presentation.inspectionContinue);actions++;continue;
                }
                if(screen=="explore")
                {
                    var next=story.Next;ContentValidation.Require(next!=null,"No next investigation before ending");
                    Checkpoint("explore-"+next.id,"Investigate: "+next.title);
                    yield return new WaitForSeconds(1.1f);
                    if(story.Progress.location!=next.location)
                    {
                        int room=Array.FindIndex(storage.Pack.locations,l=>l.id==next.location);
                        RecordClick(view.locationButtons[room]);
                    }
                    else RecordClick(view.hotspot);
                    actions++;continue;
                }
                if(screen=="puzzle")
                {
                    if(!recordedScreens.Contains("puzzle-"+stage.id))
                    {
                        Checkpoint("puzzle-"+stage.id,"Block puzzle: "+stage.title);
                        yield return new WaitForSeconds(2);
                    }
                    var move=BestSmokeMove();ContentValidation.Require(move.HasValue,"No legal move: "+stage.id);
                    int oldMoves=puzzle.Session.RemainingMoves;
                    yield return new WaitForSeconds(.42f);
                    RecordClick(view.cells[(stage.height-1-move.Value.Y)*stage.width+move.Value.X].button);
                    ContentValidation.Require(puzzle.Session.RemainingMoves==oldMoves-1,"Click did not consume exactly one legal move");
                    recordedMoves++;actions++;continue;
                }
                if(screen=="riddle")
                {
                    Checkpoint("riddle-"+riddle.id,"Riddle: "+riddle.title);
                    yield return new WaitForSeconds(Mathf.Clamp(view.question.text.Length/20f,4,8));
                    int count=story.Progress.completed.Count;
                    int[] wrong=(int[])riddle.answer.Clone();
                    if(wrong.Length==1)wrong[0]=(wrong[0]+1)%riddle.options.Length;
                    else{int first=wrong[0];wrong[0]=wrong[1];wrong[1]=first;}
                    foreach(int option in wrong){RecordClick(view.options[option]);yield return new WaitForSeconds(.4f);}
                    RecordClick(view.submitAnswer);
                    ContentValidation.Require(!view.resultPanel.activeSelf && story.Progress.completed.Count==count,"Wrong answer advanced story");
                    Checkpoint("wrong-"+riddle.id,"Wrong answer safely rejected: "+riddle.title);
                    yield return new WaitForSeconds(1.7f);
                    RecordClick(view.riddleHint);yield return new WaitForSeconds(2.8f);
                    RecordClick(view.resetAnswer);
                    foreach(int option in riddle.answer){RecordClick(view.options[option]);yield return new WaitForSeconds(.6f);}
                    RecordClick(view.submitAnswer);
                    ContentValidation.Require(view.resultPanel.activeSelf,"Correct answer did not show result");
                    recordedRiddles.Add(riddle.id);actions++;continue;
                }
                throw new InvalidOperationException("Unexpected screen: "+screen);
            }
            if(firstSlice)
            {
                ContentValidation.Require(story.Progress.completed.Contains("keys"),"First investigation did not complete");
                RecordClick(view.notebookButton); yield return new WaitForSeconds(2);
                ContentValidation.Require(Presentation.notebookImage.gameObject.activeSelf,"Visual observation missing from notebook");
                Checkpoint("first-slice-notebook","White thread recorded without changing saved clue set");
                RecordClick(view.notebookBack); RecordClick(view.exploreTitleButton); RecordClick(view.continueButton);
                ContentValidation.Require(NotebookEntries().Any(c=>c.id=="observation.keys"),"Visual observation lost on resume");
                Checkpoint("first-slice-resumed","First investigation resumed"); yield return new WaitForSeconds(2);
                recordingDone=true; yield break;
            }
            ContentValidation.Require(story.Progress.ending && story.Progress.completed.Count==storage.Pack.nodes.Length,"Campaign did not reach ending");
            ContentValidation.Require(recordedStages.Count==storage.Pack.nodes.Count(n=>n.kind=="stage"),"Not all story stages played");
            ContentValidation.Require(recordedRiddles.Count==storage.Pack.riddles.Length,"Not all riddles played");
            int expectedLines=storage.Pack.nodes.Sum(n=>n.before.Length+n.after.Length);
            ContentValidation.Require(recordedLines.Count==expectedLines,"Dialogue coverage incomplete: "+recordedLines.Count+"/"+expectedLines);
            Checkpoint("90-ending","Ending reached");yield return new WaitForSeconds(7);
            RecordClick(view.endingNotebook);yield return new WaitForSeconds(2);
            foreach(var button in view.clueButtons.Where(b=>b.gameObject.activeInHierarchy))
            {ContentValidation.Require(button.interactable,"Missing clue at ending");RecordClick(button);yield return new WaitForSeconds(2);}
            Checkpoint("91-notebook","All clues readable");RecordClick(view.notebookBack);yield return new WaitForSeconds(1);
            RecordClick(view.endingTitle);yield return new WaitForSeconds(1.5f);RecordClick(view.continueButton);
            ContentValidation.Require(story.Progress.ending && view.endingScreen.activeSelf,"Resume did not restore ending");
            Checkpoint("92-resumed","Saved ending restored");yield return new WaitForSeconds(4);
            recordingDone=true;
        }
        private string linePhase()=>ReferenceEquals(lines,activeNode.before)?"before":"after";

        private void AuditModAndPractice()
        {
            var baseSave=JsonUtility.ToJson(storage.Load(out var warning));
            ContentValidation.Require(warning==null,"Unexpected save recovery warning");
            // Exercise actual practice/exit controls; verify the campaign save remains unchanged.
            PressForSmoke(view.endingTitle);PressForSmoke(view.practiceButton);PressForSmoke(view.catalogRows[0]);
            ContentValidation.Require(screen=="puzzle" && practice,"Practice did not open");
            PressForSmoke(view.puzzleHintButton);PressForSmoke(view.puzzleBackButton);PressForSmoke(view.resultSecondary);
            ContentValidation.Require(screen=="puzzle" && !view.resultPanel.activeSelf,"Cancel exit did not return to board");
            PressForSmoke(view.puzzleBackButton);PressForSmoke(view.resultPrimary);
            ContentValidation.Require(JsonUtility.ToJson(storage.Load(out _))==baseSave,"Practice changed story save");
            storage.InstallExample(Path.Combine(Application.streamingAssetsPath,"Hotel","ExampleMod"));
            ContentValidation.Require(storage.TryActivate("moonlight.example",out var error),error);
            ContentValidation.Require(storage.Image("block.0")!=null && storage.Pack.stages.Any(s=>s.id=="moonlight_bonus"),"MOD assets not available");
            ContentValidation.Require(!storage.HasSave,"MOD inherited base save");
            storage.Save(storage.NewProgress());
            ContentValidation.Require(storage.TryActivate("base",out error),error);
            ContentValidation.Require(JsonUtility.ToJson(storage.Load(out _))==baseSave,"Base save changed after MOD round trip");
        }
    }
}
