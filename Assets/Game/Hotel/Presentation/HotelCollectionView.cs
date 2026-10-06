using System;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed class HotelCollectionView : MonoBehaviour
    {
        [Header("Authored cards · editable in Hierarchy")]
        public Image[] cardImages, cardFaces;
        public Text[] cardTitles, cardStates, cardNumbers;
        public Image progressFill;
        public Text invitation, pageLabel, origin, detailNumber;
        public Button previousPage, nextPage, closeDetail;
        public GameObject detailPanel;
        [Header("Default illustrations · MOD overrides take priority")]
        public string[] illustrationIds;
        public Sprite[] illustrations;
        public Sprite fallback;
        public Sprite Illustration(string id)
        {
            int index=Array.IndexOf(illustrationIds,id);
            return index>=0 && index<illustrations.Length?illustrations[index]:fallback;
        }
    }
}
