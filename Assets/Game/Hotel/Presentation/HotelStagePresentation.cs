using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    /// <summary>Animates existing scene references. Creates no UI objects or layouts.</summary>
    public sealed class HotelStagePresentation : MonoBehaviour
    {
        [Header("Authored stage · no split screen")]
        public Image leftActor, rightActor;
        [Tooltip("Right-facing default portraits to mirror when used by the right speaker. External MOD art keeps its supplied orientation.")]
        public Sprite[] mirrorOnRight = Array.Empty<Sprite>();
        public CanvasGroup leftGroup, rightGroup, dialogueGroup, curtain;
        public RectTransform dialogueCard, continueMark;
        public HotelTextReveal textReveal;
        public Text dialogueText;
        [Header("Portrait framing · shared bottom baseline")]
        [Range(400, 760)] public float portraitHeight = 690;
        [Range(600, 1000)] public float maximumPortraitWidth = 900;
        // MOD-specific facing; default art remains editable through mirrorOnRight.
        public Func<Sprite, bool?> modPortraitMirror;
        [Header("Timing · editable in Inspector")]
        [Range(10,90)] public float lettersPerSecond = 32;
        [Range(.1f,1)] public float entranceSeconds = .3f;
        [Range(.1f,1)] public float exitSeconds = .2f;
        [Range(.1f,1)] public float roomFadeSeconds = .25f;
        [Range(0,120)] public float entranceDistance = 60;
        [Range(0,8)] public float breathPixels = 1.8f;
        public bool IsRevealing { get; private set; }
        public bool IsClosing { get; private set; }
        private Vector2 leftHome, rightHome, cardHome, markHome;
        private Color leftTint = Color.white, rightTint = Color.white;
        private float enteredAt, revealCount;
        private bool initialized, active, leftSpeaking;
        private Coroutine transition;
        // Keep the authored centre and bottom while changing sprite aspect ratio.
        private static void AnchorPortrait(Image actor)
        {
            var rect = actor.rectTransform;
            var position = rect.anchoredPosition;
            position += new Vector2((.5f - rect.pivot.x) * rect.rect.width, -rect.pivot.y * rect.rect.height);
            rect.pivot = new Vector2(.5f, 0); rect.anchoredPosition = position;
        }
        private void FitPortrait(Image actor)
        {
            if (actor.sprite == null) return;
            float aspect = actor.sprite.rect.width / actor.sprite.rect.height;
            float width = Mathf.Min(maximumPortraitWidth, portraitHeight * aspect);
            actor.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            actor.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width / aspect);
        }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            AnchorPortrait(leftActor); AnchorPortrait(rightActor);
            leftHome = leftActor.rectTransform.anchoredPosition;
            rightHome = rightActor.rectTransform.anchoredPosition;
            cardHome = dialogueCard.anchoredPosition;
            markHome = continueMark.anchoredPosition;
            curtain.alpha = 0; curtain.blocksRaycasts = false;
        }
        public void Open(Sprite protagonist, Sprite other)
        {
            Initialize(); active = true; IsClosing = false; enteredAt = HotelPresentationClock.Now;
            leftActor.sprite = protagonist; rightActor.sprite = other;
            FitPortrait(leftActor); FitPortrait(rightActor);
            leftActor.rectTransform.localRotation = rightActor.rectTransform.localRotation = Quaternion.identity;
            OrientRightActor();
            leftActor.enabled = protagonist != null; rightActor.enabled = other != null;
            leftGroup.alpha = rightGroup.alpha = dialogueGroup.alpha = 0;
        }
        public void Speak(bool protagonist, Sprite portrait, string text, bool immediate)
        {
            leftSpeaking = protagonist;
            var actor = protagonist ? leftActor : rightActor;
            // Changing the conversation partner briefly introduces that sprite again.
            if (portrait != actor.sprite) { actor.sprite = portrait; actor.enabled = portrait != null; FitPortrait(actor); }
            OrientRightActor();
            leftTint = protagonist ? Color.white : new Color(.68f,.70f,.74f);
            rightTint = protagonist ? new Color(.68f,.70f,.74f) : Color.white;
            dialogueText.text = text; revealCount = 0; IsRevealing = !immediate;
            textReveal.Reveal(immediate ? 100000 : 0);
            continueMark.gameObject.SetActive(immediate);
        }
        public void CompleteText()
        {
            IsRevealing = false; textReveal.Reveal(100000); continueMark.gameObject.SetActive(true);
        }
        private void OrientRightActor()
        {
            var scale = rightActor.rectTransform.localScale;
            bool flipRight = modPortraitMirror?.Invoke(rightActor.sprite) ?? (Array.IndexOf(mirrorOnRight, rightActor.sprite) >= 0);
            scale.x = Mathf.Abs(scale.x) * (flipRight ? -1 : 1);
            rightActor.rectTransform.localScale = scale;
            var leftScale = leftActor.rectTransform.localScale;
            leftScale.x = Mathf.Abs(leftScale.x) * ((modPortraitMirror?.Invoke(leftActor.sprite) ?? false) ? -1 : 1);
            leftActor.rectTransform.localScale = leftScale;
        }
        public void Close(Action done, bool immediate)
        {
            if (IsClosing) return;
            if (immediate) { CancelDialogue(); done(); return; }
            IsClosing = true; StartCoroutine(CloseRoutine(done));
        }
        private IEnumerator CloseRoutine(Action done)
        {
            float started = HotelPresentationClock.Now;
            while (HotelPresentationClock.Now - started < exitSeconds)
            {
                float t = Mathf.Clamp01((HotelPresentationClock.Now - started) / exitSeconds);
                leftGroup.alpha = rightGroup.alpha = dialogueGroup.alpha = 1-t;
                leftActor.rectTransform.anchoredPosition = leftHome - Vector2.right * entranceDistance * t;
                rightActor.rectTransform.anchoredPosition = rightHome + Vector2.right * entranceDistance * t;
                yield return null;
            }
            CancelDialogue(); done();
        }
        public void CancelDialogue()
        {
            active = false; IsClosing = IsRevealing = false;
            if (!initialized) return;
            leftActor.rectTransform.anchoredPosition = leftHome;
            rightActor.rectTransform.anchoredPosition = rightHome;
            leftActor.rectTransform.localRotation = rightActor.rectTransform.localRotation = Quaternion.identity;
            dialogueCard.anchoredPosition = cardHome;
        }
        public void ChangeRoom(Action change, Action finished, bool immediate)
        {
            Initialize();
            if (immediate) { change(); finished(); return; }
            if (transition != null) return;
            transition = StartCoroutine(RoomRoutine(change, finished));
        }
        private IEnumerator RoomRoutine(Action change, Action finished)
        {
            curtain.blocksRaycasts = true;
            for (float t=0;t<roomFadeSeconds;t+=HotelPresentationClock.Delta)
            { curtain.alpha = Mathf.Clamp01(t/roomFadeSeconds); yield return null; }
            curtain.alpha = 1;
            change();
            for (float t=0;t<roomFadeSeconds;t+=HotelPresentationClock.Delta)
            { curtain.alpha = 1-Mathf.Clamp01(t/roomFadeSeconds); yield return null; }
            curtain.alpha = 0; curtain.blocksRaycasts = false; transition = null; finished();
        }
        private void Update()
        {
            if (!active || IsClosing) return;
            float t = Mathf.Clamp01((HotelPresentationClock.Now-enteredAt)/entranceSeconds);
            float eased = 1-Mathf.Pow(1-t,3);
            leftGroup.alpha = rightGroup.alpha = dialogueGroup.alpha = eased;
            float breath = Mathf.Sin(HotelPresentationClock.Now*1.9f)*breathPixels;
            leftActor.rectTransform.anchoredPosition = leftHome - Vector2.right*entranceDistance*(1-eased) + Vector2.up*(leftSpeaking ? breath : 0);
            rightActor.rectTransform.anchoredPosition = rightHome + Vector2.right*entranceDistance*(1-eased) + Vector2.up*(!leftSpeaking ? breath : 0);
            dialogueCard.anchoredPosition = cardHome - Vector2.up*20*(1-eased);
            leftActor.color = Color.Lerp(leftActor.color,leftTint,HotelPresentationClock.Delta*14);
            rightActor.color = Color.Lerp(rightActor.color,rightTint,HotelPresentationClock.Delta*14);
            continueMark.anchoredPosition = markHome + Vector2.up*Mathf.Sin(HotelPresentationClock.Now*4)*3;
            if (!IsRevealing) return;
            revealCount += HotelPresentationClock.Delta*lettersPerSecond;
            textReveal.Reveal((int)revealCount);
            if (textReveal.QuadCount > 0 && revealCount >= textReveal.QuadCount) CompleteText();
        }
    }
}
