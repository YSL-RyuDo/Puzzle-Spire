using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelStoryStudio
    {
        private void Render()
        {
            filling = true; fields.Clear(); longSet = null;
            var pack = Project.pack; string[] rows = Rows();
            selected = Mathf.Clamp(selected, 0, Math.Max(0, rows.Length - 1));
            int pages = Math.Max(1, (rows.Length + view.rows.Length - 1) / view.rows.Length);
            page = Mathf.Clamp(page, 0, pages - 1);
            view.heading.text = "이야기 제작실 · " + pack.title;
            view.context.text = tab == 5 ? (Node?.title ?? "조사를 먼저 만드세요") + (dialogueAfter ? " / 조사 후 대사" : " / 조사 전 대사") : tab == 7 ? "그림 가져오기 · 자르기 · 방향 조정" : "프로젝트를 편집하고 실제 게임으로 시험하세요";
            view.page.text = (page + 1) + " / " + pages;
            view.previous.interactable = page > 0; view.next.interactable = page < pages - 1;
            for (int i = 0; i < view.tabs.Length; i++) view.tabs[i].GetComponent<UnityEngine.UI.Image>().color = i == tab ? new Color(.5f,.4f,.24f) : new Color(.19f,.36f,.38f);
            for (int i = 0; i < view.rows.Length; i++)
            {
                int at = page * view.rows.Length + i; view.rows[i].gameObject.SetActive(at < rows.Length);
                if (at < rows.Length) HotelView.Label(view.rows[i], (at == selected ? "● " : "") + rows[at]);
            }
            view.longText.gameObject.SetActive(false); view.longLabel.gameObject.SetActive(false);
            if (tab == 0)
            {
                FieldValue("이야기 제목", () => pack.title, x => pack.title = x);
                FieldValue("제작자", () => Project.author, x => Project.author = x);
                FieldValue("엔딩 제목", () => pack.endingTitle, x => pack.endingTitle = x);
                FieldValue("프로젝트 식별자", () => Project.id, _ => { });
                LongValue("엔딩에 표시할 글", pack.endingText, x => pack.endingText = x);
            }
            else if (rows.Length > 0)
            {
                if (tab == 1)
                {
                    var l = pack.locations[selected];
                    FieldValue("장소 이름", () => l.name, x => l.name = x);
                    FieldValue("장소가 열리는 조사", () => l.unlockAfter ?? "", x => l.unlockAfter = x, new[] { new Choice("", "처음부터 열림") }.Concat(pack.nodes.Select(n => new Choice(n.id, n.title))).ToArray());
                    FieldValue("배경 그림 슬롯", () => l.image ?? "background." + l.id, x => l.image = x);
                    LongValue("장소 설명", l.subtitle, x => l.subtitle = x);
                }
                if (tab == 2)
                {
                    var c = pack.characters[selected];
                    FieldValue("인물 이름", () => c.name, x => c.name = x);
                    FieldValue("역할 / 소개", () => c.role, x => c.role = x);
                    FieldValue("인물 식별자", () => c.id, _ => { });
                    LongValue("그림과 자세", "오른쪽 ‘그림 편집’에서 기본 그림과 자세별 PNG를 넣을 수 있어요.\n대사 탭에서 이 인물과 자세를 선택하세요.", null);
                }
                if (tab == 3)
                {
                    var c = pack.clues[selected]; FieldValue("단서 제목", () => c.title, x => c.title = x);
                    LongValue("수집품에서 보여줄 설명", c.description, x => c.description = x);
                }
                if (tab == 4 && Node != null) InvestigationForm();
                if (tab == 5 && Lines.Length > 0)
                {
                    var line = Lines[selected];
                    FieldValue("말하는 인물", () => line.speaker, x => { line.speaker = x; line.pose = ""; }, People());
                    string prefix = "character." + line.speaker + ".";
                    var poses = new[] { "", "curious", "found", "worried", "remember", "think", "explain", "warm" }.Concat(Project.images.Where(i => i.key.StartsWith(prefix)).Select(i => i.key.Substring(prefix.Length))).Distinct().Select(p => new Choice(p, string.IsNullOrEmpty(p) ? "기본 자세" : p)).ToArray();
                    FieldValue("그림 / 자세", () => line.pose ?? "", x => line.pose = x, poses);
                    LongValue("대사 내용 · 줄바꿈 가능", line.text, x => line.text = x);
                }
                if (tab == 6) RiddleForm(pack.riddles[selected]);
                if (tab == 7) { selectedKey = ImageKeys()[selected]; ImageForm(); }
            }
            for (int i = 0; i < view.fields.Length; i++)
            {
                bool active = i < fields.Count; view.labels[i].gameObject.SetActive(active); view.fields[i].gameObject.SetActive(active); view.fieldPickers[i].gameObject.SetActive(active && fields[i].choices != null);
                if (!active) continue;
                var f = fields[i]; view.labels[i].text = f.label;
                string value = f.get() ?? "";
                view.fields[i].SetTextWithoutNotify(f.choices == null ? value : f.choices.FirstOrDefault(c => c.value == value)?.label ?? value);
                view.fields[i].readOnly = f.choices != null;
            }
            if (longSet != null || tab == 2) { view.longText.gameObject.SetActive(true); view.longLabel.gameObject.SetActive(true); view.longText.readOnly = longSet == null; }
            view.add.interactable = tab != 5 || Node != null;
            view.remove.interactable = tab != 0 && rows.Length > 0 && !(tab == 2 && pack.characters[selected].id == "you");
            view.up.interactable = view.down.interactable = (tab == 4 || tab == 5) && rows.Length > 1;
            view.chain.interactable = tab == 4 && rows.Length > 0;
            view.copy.interactable = tab == 0;
            view.copy.gameObject.SetActive(tab == 0); view.chain.gameObject.SetActive(tab == 4);
            view.dialogue.interactable = Node != null;
            view.special.interactable = (tab >= 1 && tab <= 4 && rows.Length > 0);
            HotelView.Label(view.special, tab == 4 ? (Node?.kind == "stage" ? "퍼즐 편집 · 검증" : Node?.kind == "riddle" ? "수수께끼 편집" : "이 조사 대사 편집") : "그림 편집");
            view.selectedTrial.interactable = Node != null;
            view.textMode.gameObject.SetActive(tab == 5 || tab == 6);
            HotelView.Label(view.textMode, tab == 5 ? (dialogueAfter ? "조사 전 대사로" : "조사 후 대사로") : "질문 / 힌트 / 해설");
            view.importImage.gameObject.SetActive(tab == 7); view.clearImage.gameObject.SetActive(tab == 7);
            view.clearImage.interactable = Project.images.Any(i => i.key == selectedKey);
            view.status.text = "자동 저장 · " + pack.locations.Length + "곳 / " + pack.nodes.Length + "개 조사 / 사용 중인 퍼즐 검증 " + ValidProofCount() + "/" + HotelStoryProjects.PublishedPack(Project).stages.Length + " · 퍼즐의 풀이 순서는 자유입니다.";
            filling = false; DrawPreview();
        }
        private int ValidProofCount() => HotelStoryProjects.PublishedPack(Project).stages.Count(s => Project.proofs.Any(p => p.stageId == s.id && p.fingerprint == HotelPuzzleClearance.Fingerprint(s)));
        private void InvestigationForm()
        {
            var n = Node; var pack = Project.pack;
            FieldValue("조사 이름", () => n.title, x => n.title = x);
            FieldValue("챕터 이름 · 같은 이름끼리 한 챕터", () => n.chapter, x => n.chapter = x);
            FieldValue("장소", () => n.location, x => n.location = x, Rooms());
            FieldValue("조사 종류", () => n.kind, x => { n.kind = x; EnsureContent(n); }, new[] { new Choice("dialogue", "대화 / 물건 조사"), new Choice("stage", "블록 퍼즐"), new Choice("riddle", "수수께끼"), new Choice("ending", "엔딩") });
            FieldValue("연결할 퍼즐 / 수수께끼", () => n.contentId ?? "", x => n.contentId = x, n.kind == "stage" ? pack.stages.Select(s => new Choice(s.id, s.title)).ToArray() : n.kind == "riddle" ? pack.riddles.Select(r => new Choice(r.id, r.title)).ToArray() : new[] { new Choice("", "연결 없음") });
            FieldValue("이 조사 전에 끝낼 조사", () => n.requires.FirstOrDefault() ?? "", x => n.requires = string.IsNullOrEmpty(x) ? Array.Empty<string>() : new[] { x }, Nodes(true));
            FieldValue("완료하면 얻을 단서", () => n.rewards.FirstOrDefault() ?? "", x => n.rewards = string.IsNullOrEmpty(x) ? Array.Empty<string>() : new[] { x }, new[] { new Choice("", "단서 없음") }.Concat(pack.clues.Select(c => new Choice(c.id, c.title))).ToArray());
            FieldValue("조사 위치 가로 % · 미리보기를 클릭해도 돼요", () => (n.x * 100).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), x => n.x = Fraction(x));
            FieldValue("조사 위치 세로 %", () => (n.y * 100).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), x => n.y = Fraction(x));
            LongValue("조사 목표 / 안내", n.purpose, x => n.purpose = x);
        }
        private void RiddleForm(RiddleData r)
        {
            FieldValue("문제 이름", () => r.title, x => r.title = x);
            FieldValue("문제 유형", () => r.kind, x => r.kind = x, new[] { new Choice("choice", "정답 하나 선택"), new Choice("order", "순서 맞히기") });
            FieldValue("정답 번호 · 순서는 2,1,3처럼 입력", () => string.Join(",", r.answer.Select(a => a + 1)), x => r.answer = x.Split(',').Select(v => Number(v.Trim()) - 1).ToArray());
            for (int i = 0; i < 6; i++)
            {
                int at = i;
                FieldValue("선택지 " + (i + 1) + " · 마지막 빈칸은 제외", () => at < r.options.Length ? r.options[at] : "", x => {
                    var options = Enumerable.Range(0, 6).Select(j => j < r.options.Length ? r.options[j] : "").ToArray(); options[at] = x;
                    r.options = options.Take(Array.FindLastIndex(options, v => !string.IsNullOrWhiteSpace(v)) + 1).ToArray();
                });
            }
            if (riddleText == 0) LongValue("문제 내용", r.question, x => r.question = x);
            else if (riddleText == 1) LongValue("힌트 · 한 줄에 하나", string.Join("\n", r.hints), x => r.hints = x.Split('\n').Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
            else LongValue("정답 해설", r.explanation, x => r.explanation = x);
        }
        private ImageOverride SelectedImage => Project.images.FirstOrDefault(i => i.key == selectedKey);
        private void ImageForm()
        {
            var art = SelectedImage;
            FieldValue("그림 슬롯", () => selectedKey, _ => { });
            FieldValue("PNG 경로 · 비워 두면 파일 선택 창", () => importPath, x => importPath = x);
            FieldValue("자르기 X · 왼쪽 기준 픽셀", () => (art?.x ?? 0).ToString(), x => { if (art != null) art.x = Number(x); });
            FieldValue("자르기 Y · 아래 기준 픽셀", () => (art?.y ?? 0).ToString(), x => { if (art != null) art.y = Number(x); });
            FieldValue("자르기 너비 · 0이면 원본 전체", () => (art?.width ?? 0).ToString(), x => { if (art != null) art.width = Number(x); });
            FieldValue("자르기 높이", () => (art?.height ?? 0).ToString(), x => { if (art != null) art.height = Number(x); });
            FieldValue("대화에서 좌우 뒤집기", () => art?.flipInDialogue == true ? "yes" : "no", x => { if (art != null) art.flipInDialogue = x == "yes"; }, new[] { new Choice("no", "원본 방향"), new Choice("yes", "좌우 반전") });
            LongValue("그림 편집 안내", "PNG를 불러온 뒤 사용할 영역을 자르고 방향을 정하세요. 원본 파일은 보존됩니다.\n그림 그리기 자체는 외부 그림 도구에서 하고, 여기서는 게임에 적용할 슬롯을 편집합니다.\n배경·인물·자세·단서·블록을 바꾸고 ‘현재 조사 시험’으로 실제 화면을 확인할 수 있어요.", null);
            view.longText.gameObject.SetActive(true); view.longLabel.gameObject.SetActive(true); view.longText.readOnly = true;
        }
        public void ImportImage(string path)
        {
            HotelStoryProjects.ImportPng(Project, ProjectFolder, selectedKey, path); importPath = ""; Save(); Render();
        }
        private void DrawPreview()
        {
            if (previewSprite != null) Destroy(previewSprite); if (previewTexture != null) Destroy(previewTexture);
            previewSprite = null; previewTexture = null;
            string key = tab == 7 ? selectedKey : tab == 1 && Project.pack.locations.Length > 0 ? Project.pack.locations[selected].image : tab == 2 && Project.pack.characters.Length > 0 ? "character." + Project.pack.characters[selected].id : tab == 3 && Project.pack.clues.Length > 0 ? "clue." + Project.pack.clues[selected].id : Project.pack.locations.FirstOrDefault(l => l.id == Node?.location)?.image;
            if (tab == 5 && Lines.Length > 0)
            {
                var line = Lines[selected]; key = "character." + line.speaker + (string.IsNullOrEmpty(line.pose) ? "" : "." + line.pose);
                if (!Project.images.Any(i => i.key == key) && Project.images.Any(i => i.key == "character." + line.speaker)) key = "character." + line.speaker;
            }
            Sprite sprite = defaultImage(key);
            var art = Project.images.FirstOrDefault(i => i.key == key);
            try
            {
                if (art != null)
                {
                    string path = HotelStorage.ResolveFile(ProjectFolder, art.file); HotelStorage.ValidatePng(path, art);
                    previewTexture = new Texture2D(2, 2); ContentValidation.Require(previewTexture.LoadImage(File.ReadAllBytes(path)), "그림을 읽지 못했습니다.");
                    var region = art.width > 0 ? new Rect(art.x, art.y, art.width, art.height) : new Rect(0, 0, previewTexture.width, previewTexture.height);
                    previewSprite = Sprite.Create(previewTexture, region, Vector2.one * .5f, 100); sprite = previewSprite;
                }
            }
            catch (Exception e) { view.status.text = "그림 확인: " + e.Message; }
            view.preview.sprite = sprite; view.preview.enabled = sprite != null;
            view.preview.rectTransform.localScale = new Vector3(tab == 7 && art?.flipInDialogue == true ? -1 : 1, 1, 1);
            view.previewCaption.text = tab == 4 ? "배경을 클릭하면 조사 위치가 바뀝니다.\n대화·퍼즐은 아래 버튼으로 연결하세요." : art == null ? "기본 그림 미리보기 · PNG를 넣어 교체할 수 있습니다." : "프로젝트에 복사된 그림 · 원본 파일은 유지됩니다.";
            view.marker.gameObject.SetActive(tab == 4 && Node != null);
            if (tab == 4 && Node != null) { view.marker.rectTransform.anchorMin = view.marker.rectTransform.anchorMax = new Vector2(Node.x, 1 - Node.y); view.marker.rectTransform.anchoredPosition = Vector2.zero; }
        }
        private void OnDestroy() { if (previewSprite != null) Destroy(previewSprite); if (previewTexture != null) Destroy(previewTexture); }
    }
}
