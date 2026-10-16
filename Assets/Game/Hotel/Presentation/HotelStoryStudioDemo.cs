using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    // Opt-in demonstration uses the saved scene controls and a temporary user-data directory.
    public sealed partial class HotelGame
    {
        private void BeginStudioDemo()
        {
            var args = Environment.GetCommandLineArgs(); int encoder = Array.IndexOf(args, "-hotel-encoder");
            ContentValidation.Require(encoder >= 0 && encoder + 1 < args.Length, "Video encoder path required");
            recordingFolder = Path.Combine(Application.dataPath, "..", "StoryStudioDemo-20261012");
            Directory.CreateDirectory(Path.Combine(recordingFolder, "Checkpoints"));
            File.WriteAllText(Path.Combine(recordingFolder, "timeline.tsv"), "seconds\tevent\n");
            File.WriteAllText(Path.Combine(recordingFolder, "encoder.log"), "");
            storage.Preferences.sound = false;
            Time.captureFramerate = HotelVideoWriter.Fps; HotelPresentationClock.Recording = true;
            video = new HotelVideoWriter(view, args[encoder + 1], Path.Combine(recordingFolder, "MoonlitHotel-FullPlay.mp4"));
            Application.logMessageReceived += RecordingLog;
            StartCoroutine(CaptureStudioDemo()); recordedPlay = StartCoroutine(StudioDemoSteps());
        }
        private IEnumerator CaptureStudioDemo()
        {
            while (!recordingDone && recordingFailure == null && video.Frames < HotelVideoWriter.Fps * 900)
            {
                yield return new WaitForEndOfFrame();
                try { video.Capture(); } catch (Exception e) { recordingFailure = e.ToString(); }
            }
            if (!recordingDone && recordingFailure == null) recordingFailure = "Demonstration timed out";
            if (recordedPlay != null) StopCoroutine(recordedPlay);
            Application.logMessageReceived -= RecordingLog;
            try { video.Dispose(); } catch (Exception e) { recordingFailure = e.ToString(); }
            Time.captureFramerate = 0; HotelPresentationClock.Recording = false;
            string report = recordingFailure == null ? "PASS\nCreated through authored editor UI; PNG import/crop, dialogue, investigation, puzzle painting and legal creator clear, riddle, export ZIP.\nActual MOD selection, fresh play from first event to ending, clue collection and saved ending reload: PASS\nInvestigations: " + story.Progress.completed.Count + "\nStages completed: " + recordedStages.Count + "\nRiddles completed: " + recordedRiddles.Count + "\nDialogue lines shown: " + recordedLines.Count + "\nLegal board moves: " + recordedMoves + "\nVerified UI clicks: " + recordedClicks + "\nDuration seconds: " + (video.Frames / (double)HotelVideoWriter.Fps).ToString("F3", CultureInfo.InvariantCulture) + "\n24fps, silent, automated UI operation; no forced completion. PNG path entered into actual import field.\n" : "FAIL\n" + recordingFailure;
            File.WriteAllText(Path.Combine(recordingFolder, "verification.txt"), report);
            Debug.Log(recordingFailure == null ? "HOTEL_STUDIO_DEMO_PASSED\n" + report : "HOTEL_STUDIO_DEMO_FAILED\n" + report);
            Application.Quit(recordingFailure == null ? 0 : 2);
        }
        private IEnumerator DemoClick(Button button, float pause = .7f) { RecordClick(button); yield return new WaitForSeconds(pause); }
        private IEnumerator DemoType(InputField input, string value)
        {
            input.text = "";
            for (int i = 0; i < value.Length; i += 2) { input.text = value.Substring(0, Math.Min(value.Length, i + 2)); yield return new WaitForSeconds(.035f); }
            input.onEndEdit.Invoke(input.text); yield return new WaitForSeconds(.4f);
        }
        private IEnumerator DemoChoose(int field, int choice)
        {
            var w = view.storyStudio.View;
            yield return DemoClick(w.fieldPickers[field]); yield return DemoClick(w.pickerRows[choice]);
        }
        private IEnumerator DemoSection(string key, string text, float seconds = 2)
        {
            Checkpoint(key, "Demo: " + text); yield return new WaitForSeconds(seconds);
        }
        private IEnumerator StudioDemoSteps()
        {
            yield return DemoSection("00-title", "제작 시작 · 타이틀에서 이야기 제작실 열기");
            yield return DemoClick(view.storyStudioButton);
            var studio = view.storyStudio; var w = studio.View;
            string art = Path.Combine(Application.streamingAssetsPath, "Hotel/Themes/ClassicIllustrated/Images");
            yield return DemoType(w.fields[0], "비 오는 밤의 편지");
            yield return DemoType(w.fields[2], "늦게 도착한 답장");
            yield return DemoType(w.longText, "편지는 도식의 손에 돌아갔다. 비가 그친 호텔에는 오래된 약속 하나가 남았다.");
            yield return DemoClick(w.save);
            yield return DemoSection("01-project", "프로젝트 제목과 엔딩 문구 만들기");
            yield return DemoClick(w.tabs[1]); yield return DemoType(w.fields[0], "호텔 로비");
            yield return DemoClick(w.special);
            w.fields[1].text = Path.Combine(art, "Lobby-PC.png"); yield return new WaitForSeconds(1);
            yield return DemoClick(w.importImage, 2);
            ContentValidation.Require(studio.Project.images.Length == 1, "Lobby import failed: " + w.status.text);
            yield return DemoSection("02-background", "그림 편집 · 배경 PNG 가져오기");
            yield return DemoClick(w.tabs[1]); yield return DemoClick(w.add);
            yield return DemoType(w.fields[0], "객실 복도"); yield return DemoClick(w.special);
            w.fields[1].text = Path.Combine(art, "Corridor-PC.png"); yield return new WaitForSeconds(.8f); yield return DemoClick(w.importImage, 1.5f);
            yield return DemoClick(w.tabs[2]); yield return DemoClick(w.special);
            w.fields[1].text = Path.Combine(art, "Characters-PC.png"); yield return new WaitForSeconds(.8f); yield return DemoClick(w.importImage);
            w.fields[4].text = "543"; w.fields[5].text = "724"; w.fields[5].onEndEdit.Invoke("724"); yield return DemoClick(w.save);
            yield return DemoSection("03-character", "인물 PNG 가져오기 · 사용할 영역 자르기", 3);
            yield return DemoClick(w.tabs[2]); yield return DemoClick(w.rows[2]); yield return DemoClick(w.special);
            w.fields[1].text = Path.Combine(art, "Characters-PC.png"); yield return DemoClick(w.importImage);
            w.fields[2].text = "1086"; w.fields[4].text = "543"; w.fields[5].text = "724"; w.fields[5].onEndEdit.Invoke("724"); yield return DemoClick(w.save);
            yield return DemoClick(w.tabs[3]); yield return DemoClick(w.add);
            yield return DemoType(w.fields[0], "받는 이 없는 편지");
            yield return DemoType(w.longText, "매일 객실 문을 열지만, 그 방에서 잠들지는 않는 사람에게."); yield return DemoClick(w.save);
            yield return DemoSection("04-clue", "조사 보상으로 사용할 단서 작성");
            yield return DemoClick(w.tabs[4]); yield return DemoType(w.fields[0], "도식의 부탁");
            yield return DemoClick(w.dialogue);
            yield return DemoType(w.longText, "도식 씨, 무슨 일인가요? 복도에서 오래된 편지를 찾으셨다고요?");
            yield return DemoClick(w.add); yield return DemoChoose(0, 2);
            yield return DemoType(w.longText, "편지함이 얼어붙어서 열리지 않네. 안에 누군가 두고 간 편지가 있어."); yield return DemoClick(w.save);
            yield return DemoSection("05-dialogue", "대사 제작 · 화자 선택과 대사 줄 추가", 3);
            yield return DemoClick(w.tabs[4]); yield return DemoClick(w.add);
            yield return DemoType(w.fields[0], "얼어붙은 편지함"); yield return DemoType(w.fields[1], "CHAPTER 1 · 편지함");
            yield return DemoChoose(2, 1); yield return DemoChoose(3, 1); yield return DemoChoose(6, 1);
            w.fields[7].text = "64"; w.fields[8].text = "46"; w.fields[8].onEndEdit.Invoke("46");
            yield return DemoType(w.longText, "복도의 편지함을 열어 편지를 찾아보자.");
            yield return DemoSection("06-investigation", "조사 편집 · 장소, 퍼즐, 단서 보상 연결", 3);
            yield return DemoClick(w.dialogue); yield return DemoClick(w.add);
            yield return DemoType(w.longText, "같은 색 블록을 모으고 얼음을 없애면 편지함을 열 수 있겠어.");
            yield return DemoClick(w.textMode); yield return DemoClick(w.add);
            yield return DemoType(w.longText, "편지를 찾았어! 받는 사람의 이름 대신 수수께끼가 적혀 있네.");
            yield return DemoClick(w.save); yield return DemoClick(w.tabs[4]); yield return DemoClick(w.special);
            var workshop = view.workshop; var pw = workshop.View;
            pw.fields[0].text = "편지함의 얼음"; pw.fields[6].text = "8";
            yield return DemoClick(pw.brushes[3]); yield return DemoClick(pw.cells[12]); pw.fields[8].text = "1";
            yield return DemoClick(pw.save);
            yield return DemoSection("07-puzzle-editor", "퍼즐 제작 · 보드에 얼음 배치와 목표 설정", 3);
            yield return DemoClick(pw.verify, 2);
            for (int i = 0; i < 20 && !workshop.Play.Cleared && !workshop.Play.Failed; i++)
            {
                puzzle = workshop.Play; stage = workshop.Draft.stage; var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No legal creator move");
                yield return DemoClick(pw.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X], .9f);
            }
            ContentValidation.Require(workshop.Draft.clearance != null, "Creator clear failed");
            yield return DemoSection("08-certified", "제작자 클리어 검증 완료 · 합법적인 플레이 기록", 3);
            yield return DemoClick(pw.stop); yield return DemoClick(pw.back);
            yield return DemoClick(w.add); yield return DemoType(w.fields[0], "편지의 주인"); yield return DemoType(w.fields[1], "CHAPTER 2 · 받는 사람");
            yield return DemoChoose(3, 2); yield return DemoClick(w.special);
            yield return DemoType(w.fields[0], "누구에게 온 편지일까?"); w.fields[2].text = "2";
            yield return DemoType(w.fields[3], "호텔에 묵는 손님"); yield return DemoType(w.fields[4], "호텔 관리 직원 도식");
            yield return DemoType(w.longText, "매일 객실 문을 열지만, 그 방에서 잠들지는 않는 사람. 이 편지는 누구에게 온 걸까?");
            yield return DemoClick(w.textMode); yield return DemoType(w.longText, "객실 문을 매일 여는 이유를 생각해 보세요.");
            yield return DemoClick(w.textMode); yield return DemoType(w.longText, "객실을 관리하는 도식은 매일 문을 열지만, 손님처럼 그 방에서 잠들지는 않습니다.");
            yield return DemoClick(w.save); yield return DemoSection("09-riddle-editor", "수수께끼 제작 · 질문, 선택지, 정답, 힌트와 해설", 3);
            yield return DemoClick(w.tabs[4]); yield return DemoClick(w.rows[3]);
            yield return DemoType(w.fields[0], "편지를 전하다"); yield return DemoType(w.fields[1], "CHAPTER 2 · 받는 사람");
            yield return DemoClick(w.dialogue); yield return DemoClick(w.textMode);
            yield return DemoType(w.longText, "도식 씨 앞으로 온 편지였어요. 이제야 주인을 찾았네요.");
            yield return DemoClick(w.add); yield return DemoChoose(0, 2);
            yield return DemoType(w.longText, "오래전 친구가 보낸 답장이구나. 찾아줘서 고맙네. 덕분에 내 기다림도 끝났어.");
            yield return DemoClick(w.tabs[4]); yield return DemoClick(w.chain); yield return DemoClick(w.save);
            HotelStoryProjects.Validate(studio.Project, studio.ProjectFolder, true);
            yield return DemoSection("10-connected", "전체 조사 순서 연결 · 엔딩까지 완성", 3);
            yield return DemoClick(w.export, 2);
            var mod = storage.Mods.Single(m => m.Valid && m.manifest.id.StartsWith("adventure."));
            File.Copy(Directory.GetFiles(Path.Combine(storage.Root, "StoryStudio/Exports"), "*.zip").Single(), Path.Combine(recordingFolder, "Rainy-Letter-Demo-MOD.zip"), true);
            yield return DemoSection("11-export", "완성한 이야기를 독립 MOD ZIP으로 내보내기", 3);
            yield return DemoClick(w.back); yield return DemoClick(view.modsButton, 2);
            yield return DemoSection("12-mod-menu", "실제 MOD 관리 · 만든 이야기 선택 전", 2);
            var row = view.catalogRows.First(b => b.gameObject.activeSelf && b.GetComponentInChildren<Text>().text.Contains(mod.manifest.name));
            Timeline("Demo: MOD 적용 순간 · 비 오는 밤의 편지"); yield return DemoClick(row, 3);
            ContentValidation.Require(storage.PackId == mod.manifest.id && !storage.HasSave && studioOriginalStorage == null, "Must start a real fresh MOD game");
            yield return DemoClick(view.catalogBack, 2);
            yield return DemoSection("13-new-game", "MOD 적용 후 새 게임 · 첫 조사부터 시작", 2);
            yield return DemoClick(view.newButton, 2);
            while (!story.Progress.ending)
            {
                yield return null; if (busy || story.Progress.ending) continue;
                ContentValidation.Require(storage.PackId == mod.manifest.id, "Adventure MOD changed during play");
                if (view.dialoguePanel.activeSelf)
                {
                    while (view.stagePresentation.IsRevealing) yield return null;
                    string key = activeNode.id + "-" + linePhase() + "-" + dialogueIndex; recordedLines.Add(key);
                    Checkpoint("play-dialogue-" + key, "Dialogue: " + view.speakerName.text + " / " + view.dialogueText.text);
                    yield return new WaitForSeconds(Mathf.Clamp(view.dialogueText.text.Length / 15f, 2.5f, 5)); RecordClick(view.dialogueNext);
                }
                else if (view.resultPanel.activeSelf)
                {
                    if (screen == "puzzle") { ContentValidation.Require(puzzle.Cleared, "Player puzzle failed"); recordedStages.Add(stage.id); }
                    yield return new WaitForSeconds(3); RecordClick(view.resultPrimary);
                }
                else if (screen == "explore")
                {
                    var next = story.Next; ContentValidation.Require(next != null, "No next investigation");
                    Checkpoint("play-event-" + next.id, "Investigate: " + next.title); yield return new WaitForSeconds(2);
                    if (story.Progress.location != next.location) RecordClick(view.nextLocation); else RecordClick(view.hotspot);
                }
                else if (screen == "puzzle")
                {
                    Checkpoint("play-puzzle", "Demo: 직접 만든 퍼즐을 일반 플레이에서 풀기");
                    var move = BestSmokeMove(); ContentValidation.Require(move.HasValue, "No legal player move");
                    yield return new WaitForSeconds(.9f); int oldMoves = puzzle.Session.RemainingMoves;
                    RecordClick(view.cells[(stage.height - 1 - move.Value.Y) * stage.width + move.Value.X].button);
                    ContentValidation.Require(puzzle.Session.RemainingMoves == oldMoves - 1, "Illegal puzzle input"); recordedMoves++;
                }
                else if (screen == "riddle")
                {
                    Checkpoint("play-riddle", "Demo: 직접 작성한 수수께끼 플레이"); yield return new WaitForSeconds(6);
                    foreach (int answer in riddle.answer) yield return DemoClick(view.options[answer]);
                    yield return DemoClick(view.submitAnswer); recordedRiddles.Add(riddle.id);
                }
                else throw new InvalidOperationException("Unexpected screen: " + screen);
            }
            ContentValidation.Require(story.Progress.completed.Count == 4 && story.Progress.clues.Count == 1 && recordedStages.Count == 1 && recordedRiddles.Count == 1, "Incomplete demo campaign");
            yield return DemoSection("14-ending", "엔딩 · 제작실에 입력한 문구 실제 적용", 6);
            yield return DemoClick(view.endingNotebook, 2); yield return DemoClick(view.clueButtons[0], 3);
            yield return DemoSection("15-clue", "직접 만든 단서의 수집품 설명 확인", 2);
            yield return DemoClick(view.collection.closeDetail); yield return DemoClick(view.notebookBack); yield return DemoClick(view.endingTitle, 2);
            yield return DemoClick(view.continueButton, 2);
            ContentValidation.Require(story.Progress.ending && storage.PackId == mod.manifest.id, "Saved ending reload failed");
            yield return DemoSection("16-resume", "이어하기 · MOD 엔딩 저장 복원", 4);
            recordingDone = true;
        }
    }
}
