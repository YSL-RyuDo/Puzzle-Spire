using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private void RefreshInvestigation()
        {
            bool lobby = story.Progress.location == "lobby";
            foreach (var prop in Presentation.props) prop.gameObject.SetActive(lobby);
            Presentation.propCaption.transform.parent.gameObject.SetActive(false);
            Presentation.investigatedMark.SetActive(lobby && story.Progress.completed.Contains("keys"));
        }
        private void InspectProp(int index)
        {
            Presentation.propCaption.transform.parent.gameObject.SetActive(true);
            Presentation.propCaption.text = index == 0
                ? "작은 황동 벨. 손때가 묻은 가장자리만 반짝인다."
                : "추 시계가 느리게 흔들린다. 비 오는 로비에도 시간은 흐르고 있다.";
        }
        private void ShowInspection(InvestigationNode node, bool discovery, Action done)
        {
            ShowExplore(); screen = "inspection";
            view.explorationHud.alpha = 0; view.explorationHud.blocksRaycasts = false;
            Presentation.inspectionPanel.SetActive(true);
            Presentation.inspectionImage.sprite = ThemeCabinet;
            Presentation.inspectionTitle.text = discovery ? "작은 흔적을 발견했다" : "303호 · 비어 있는 열쇠 자리";
            Presentation.inspectionBody.text = discovery ? node.discoveryText : "고리는 비어 있다. 수건을 놓았던 주변부터 살펴보자.";
            Presentation.discoveryCard.SetActive(discovery);
            Presentation.discoveryImage.sprite = storage.Image("clue." + node.discovery) ?? Presentation.Evidence(node.discovery);
            Presentation.discoveryTitle.text = node.discoveryTitle;
            Presentation.inspectionBack.gameObject.SetActive(!discovery);
            HotelView.Label(Presentation.inspectionContinue, discovery ? "하린에게 보여 주기" : "주변 조사하기");
            Bind(Presentation.inspectionContinue, () => { Presentation.inspectionPanel.SetActive(false); done(); });
            Bind(Presentation.inspectionBack, ShowExplore);
        }
        private List<ClueData> NotebookEntries()
        {
            // These observations are derived, not added to the serialized save's clue set.
            var entries = storage.Pack.nodes.Where(n => story.Progress.completed.Contains(n.id) && !string.IsNullOrEmpty(n.discovery))
                .Select(n => new ClueData { id = "observation." + n.id, title = n.discoveryTitle, description = n.discoveryText, image = n.discovery }).ToList();
            entries.AddRange(storage.Pack.clues.Where(c => story.Progress.clues.Contains(c.id)));
            return entries;
        }
        private void SelectNotebookEntry(ClueData clue)
        {
            var entries=CollectionEntries();
            if(!NotebookEntries().Any(c=>c.id==clue.id))return;
            view.clueTitle.text = clue.title; view.clueDescription.text = clue.description;
            Presentation.notebookImage.gameObject.SetActive(true);
            Presentation.notebookImage.sprite = CollectionSprite(clue);
            view.collection.detailNumber.text="RECORD  "+(entries.FindIndex(c=>c.id==clue.id)+1).ToString("D2");
            var node=storage.Pack.nodes.FirstOrDefault(n=>"observation."+n.id==clue.id || n.rewards.Contains(clue.id));
            var location=node==null?null:storage.Pack.locations.FirstOrDefault(l=>l.id==node.location);
            view.collection.origin.text=location==null?"조사 중 남긴 기록":location.name+"에서 발견 · "+(clue.id.StartsWith("observation.")?"관찰 기록":"사건 단서");
            view.collection.detailPanel.SetActive(true);
        }
        private int notebookPage;
        private List<ClueData> CollectionEntries()
        {
            var entries=storage.Pack.nodes.Where(n=>!string.IsNullOrEmpty(n.discovery))
                .Select(n=>new ClueData{id="observation."+n.id,title=n.discoveryTitle,description=n.discoveryText,image=n.discovery}).ToList();
            entries.AddRange(storage.Pack.clues);return entries;
        }
        private Sprite CollectionSprite(ClueData clue)
        {
            if(!string.IsNullOrEmpty(clue.image))
            {
                var supplied=storage.Image("clue."+clue.image);
                if(supplied!=null)return supplied;
            }
            return storage.Image("clue."+clue.id)??Presentation.Evidence(clue.image)??(Array.IndexOf(view.collection.illustrationIds,clue.id)>=0?view.collection.Illustration(clue.id):storage.Image("clue.fallback")??view.collection.fallback);
        }
        private void RenderCollection()
        {
            var ui=view.collection;var all=CollectionEntries();var found=NotebookEntries();
            int pages=Math.Max(1,(all.Count+view.clueButtons.Length-1)/view.clueButtons.Length);
            notebookPage=Mathf.Clamp(notebookPage,0,pages-1);
            view.clueCount.text=found.Count.ToString("D2")+"  /  "+all.Count.ToString("D2")+"  기록";
            ui.progressFill.fillAmount=all.Count==0?0:found.Count/(float)all.Count;
            ui.invitation.text=found.Count==0?"조사 중 발견한 물건과 기록이 이곳에 모입니다.":"작은 흔적 하나에도, 누군가의 이야기가 남아 있다.";
            ui.pageLabel.text=(notebookPage+1)+" / "+pages;
            ui.previousPage.gameObject.SetActive(pages>1);ui.nextPage.gameObject.SetActive(pages>1);ui.pageLabel.gameObject.SetActive(pages>1);
            ui.previousPage.interactable=notebookPage>0;ui.nextPage.interactable=notebookPage<pages-1;
            Bind(ui.previousPage,()=>{notebookPage--;RenderCollection();});Bind(ui.nextPage,()=>{notebookPage++;RenderCollection();});
            Bind(ui.closeDetail,()=>ui.detailPanel.SetActive(false));
            for(int i=0;i<view.clueButtons.Length;i++)
            {
                int index=notebookPage*view.clueButtons.Length+i;var button=view.clueButtons[i];
                button.gameObject.SetActive(index<all.Count);if(index>=all.Count)continue;
                var clue=all[index];bool owned=found.Any(c=>c.id==clue.id);button.interactable=owned;
                ui.cardNumbers[i].text=(index+1).ToString("D2");
                ui.cardTitles[i].text=owned?clue.title:"아직 발견하지 못한 기록";
                ui.cardStates[i].text=owned?"자세히 살펴보기  ↗":"조사를 이어가면 채워집니다";
                ui.cardImages[i].gameObject.SetActive(owned);ui.cardImages[i].sprite=owned?CollectionSprite(clue):null;
                ui.cardFaces[i].color=owned?new Color(.91f,.86f,.75f):new Color(.24f,.29f,.28f,.8f);
                ui.cardTitles[i].color=owned?new Color(.19f,.22f,.22f):new Color(.65f,.68f,.63f);
                ui.cardStates[i].color=owned?new Color(.47f,.39f,.27f):new Color(.56f,.60f,.55f);
                Bind(button,()=>SelectNotebookEntry(clue));
            }
        }
    }
}
