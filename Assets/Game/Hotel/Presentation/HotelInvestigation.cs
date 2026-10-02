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
            Presentation.inspectionImage.sprite = Presentation.cabinet;
            Presentation.inspectionTitle.text = discovery ? "작은 흔적을 발견했다" : "303호 · 비어 있는 열쇠 자리";
            Presentation.inspectionBody.text = discovery ? node.discoveryText : "고리는 비어 있다. 수건을 놓았던 주변부터 살펴보자.";
            Presentation.discoveryCard.SetActive(discovery);
            Presentation.discoveryImage.sprite = Presentation.Evidence(node.discovery);
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
            view.clueTitle.text = clue.title; view.clueDescription.text = clue.description;
            var sprite = !string.IsNullOrEmpty(clue.image) ? storage.Image("clue." + clue.image) ?? Presentation.Evidence(clue.image) : null;
            Presentation.notebookImage.gameObject.SetActive(sprite != null);
            Presentation.notebookImage.sprite = sprite;
            var rect = (RectTransform)view.clueDescription.transform;
            rect.anchoredPosition = new Vector2(sprite == null ? 48 : 410, -182);
            rect.sizeDelta = new Vector2(sprite == null ? 870 : 500, 320);
        }
    }
}
