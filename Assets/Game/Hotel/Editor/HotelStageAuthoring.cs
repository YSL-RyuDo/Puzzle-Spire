using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel.Editor
{
    public static partial class HotelSceneBuilder
    {
        // Explicit one-time migration requested for the PC staging redesign.
        // Normal build/open never recreates these objects or overwrites artist edits.
        [MenuItem("Puzzle Spire/PC 단일 무대 적용 (최초 1회)")]
        public static void UpgradePcStage()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = UnityEngine.Object.FindFirstObjectByType<HotelView>();
            if (v.stagePresentation != null) { Debug.Log("PC stage already authored; keeping Inspector edits."); return; }
            Directory.CreateDirectory("Builds/Backups");
            File.Copy(ScenePath, "Builds/Backups/Hotel-before-PC-stage.unity", false);
            root = v.titleScreen.transform.parent;
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/RoundedPanel.png");
            circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/Circle.png");
            int exploreIndex=v.exploreScreen.transform.GetSiblingIndex(), dialogueIndex=v.dialoguePanel.transform.GetSiblingIndex();
            UnityEngine.Object.DestroyImmediate(v.exploreScreen);
            UnityEngine.Object.DestroyImmediate(v.dialoguePanel);
            PcExplore(); v.exploreScreen.transform.SetSiblingIndex(exploreIndex);
            PcDialogue(); v.dialoguePanel.transform.SetSiblingIndex(dialogueIndex);
            v.resultPanel.transform.SetAsLastSibling();
            var curtainImage=Box(root,"11 Room transition · editable curtain",0,0,1600,900,Hex("171D24"));
            curtainImage.raycastTarget=true;
            v.stagePresentation.curtain=curtainImage.gameObject.AddComponent<CanvasGroup>();
            v.stagePresentation.curtain.alpha=0;v.stagePresentation.curtain.blocksRaycasts=false;
            var game=new SerializedObject(v.GetComponent<HotelGame>());
            var portraits=game.FindProperty("defaultPortraits"); portraits.arraySize=4;
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Hotel/Art/Characters-PC.png");
            if(atlas==null) throw new InvalidOperationException("Characters-PC.png missing");
            string[] ids={"you","front","keeper","guest"};
            for(int i=0;i<4;i++)
            {
                var sprite=Sprite.Create(atlas,new Rect(i*atlas.width/4f,0,atlas.width/4f,atlas.height),new Vector2(.5f,0),100);
                sprite.name=ids[i];AssetDatabase.CreateAsset(sprite,"Assets/Game/Hotel/Art/Portrait-"+ids[i]+".asset");
                portraits.GetArrayElementAtIndex(i).objectReferenceValue=sprite;
            }
            game.ApplyModifiedPropertiesWithoutUndo();
            v.stagePresentation.leftActor.sprite=(Sprite)portraits.GetArrayElementAtIndex(0).objectReferenceValue;
            v.stagePresentation.rightActor.sprite=(Sprite)portraits.GetArrayElementAtIndex(1).objectReferenceValue;
            v.Show(v.titleScreen);
            EditorUtility.SetDirty(v);EditorUtility.SetDirty(v.stagePresentation);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_PC_STAGE_AUTHORED");
        }
        public static void UpgradePcStageAndBuild() { UpgradePcStage(); CreateAndBuild(); }
        [MenuItem("Puzzle Spire/대화 인물 높이·하단 기준 적용")]
        public static void UpgradeDialogueFraming()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = UnityEngine.Object.FindFirstObjectByType<HotelView>();
            var presentation = v.stagePresentation;
            foreach (var actor in new[] { presentation.leftActor, presentation.rightActor })
            {
                var rect = actor.rectTransform;
                var position = rect.anchoredPosition;
                position += new Vector2((.5f - rect.pivot.x) * rect.rect.width, -rect.pivot.y * rect.rect.height);
                rect.pivot = new Vector2(.5f, 0); rect.anchoredPosition = position;
                float aspect = actor.sprite.rect.width / actor.sprite.rect.height;
                float width = Mathf.Min(presentation.maximumPortraitWidth, presentation.portraitHeight * aspect);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width / aspect);
                EditorUtility.SetDirty(rect);
            }
            EditorUtility.SetDirty(presentation);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
        }
        public static void UpgradeDialogueFramingAndBuild() { UpgradeDialogueFraming(); CreateAndBuild(); }
        private static Sprite ImportBackground(string name)
        {
            string path="Assets/Game/Hotel/Art/"+name+"-PC.png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null) return null;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void PcExplore()
        {
            v.exploreScreen=Box(root,"02 Investigation · SINGLE FULL-SCREEN ROOM",0,0,1600,900,Ink).gameObject;
            var p=v.exploreScreen.transform;
            var stage=Rect(p,"Room stage · entire PC viewport",0,0,1600,900);
            v.locationArt=new GameObject[3];v.locationImages=new Image[3];
            for(int i=0;i<3;i++)
            {
                var layer=Box(stage,new[]{"Lobby fallback","Corridor fallback","Archive fallback"}[i],0,0,1090,557,Panel);
                layer.rectTransform.localScale=new Vector3(1600f/1090,900f/557,1);
                v.locationArt[i]=layer.gameObject;
                if(i==0)LobbyArt(layer.transform,1090,557);else if(i==1)CorridorArt(layer.transform);else ArchiveArt(layer.transform);
                v.locationImages[i]=Box(stage,"Room background · "+i,0,0,1600,900,Color.white);
                v.locationImages[i].sprite=ImportBackground(new[]{"Lobby","Corridor","Archive"}[i]);
                v.locationImages[i].gameObject.SetActive(v.locationImages[i].sprite!=null);
            }
            var hud=Rect(p,"Investigation HUD · hidden during conversations",0,0,1600,900);
            v.explorationHud=hud.gameObject.AddComponent<CanvasGroup>();
            var header=Box(hud,"Location plaque",36,28,480,96,new Color(.07f,.10f,.12f,.88f),true);
            v.locationTitle=Text(header.transform,"Room name","호텔 로비",24,12,430,40,29,Paper,FontStyle.Bold);
            v.chapter=Text(header.transform,"Chapter","사라진 303호 열쇠",24,59,430,26,17,Gold);
            v.exploreTitleButton=Button(hud,"Title","메뉴",1420,32,140,48);
            v.notebookButton=Button(hud,"Notebook","조사 수첩",1220,32,184,48,Hex("766247"));
            v.hotspot=Button(hud,"Investigate · content positions","프런트의 하린",840,455,220,58,Hex("68543B"),22);
            var hr=(RectTransform)v.hotspot.transform;hr.pivot=new Vector2(.5f,.5f);
            var footer=Box(hud,"Small investigation footer",36,770,1528,104,new Color(.07f,.10f,.12f,.88f),true);
            v.objective=Text(footer.transform,"Current thought","하린에게 체크인을 요청하자.",24,15,760,36,23,Paper);
            v.locationDescription=Text(footer.transform,"Room atmosphere","비가 그친 뒤",24,62,760,26,16,Gold);
            v.locationButtons=new Button[3];
            for(int i=0;i<3;i++)v.locationButtons[i]=Button(footer.transform,"Room "+i,new[]{"로비","복도","보관실"}[i],828+i*222,22,206,58,null,20);
            v.nextLocation=Button(hud,"Suggested exit","다음 장소 →",1260,683,304,55);
            // Kept as authored optional detail labels; no sidebar or second viewport.
            v.hotspotTitle=Text(hud,"Optional action detail","",36,705,700,30,20,Gold);v.hotspotTitle.gameObject.SetActive(false);
            v.hotspotDescription=Text(hud,"Optional action description","",36,665,700,32,20);v.hotspotDescription.gameObject.SetActive(false);
            v.progressText=Text(hud,"Optional counters","",36,740,700,24,16);v.progressText.gameObject.SetActive(false);
        }
        private static void PcDialogue()
        {
            v.dialoguePanel=Box(root,"09 Dialogue · actors on SAME room background",0,0,1600,900,new Color(.025f,.045f,.065f,.14f)).gameObject;
            var p=v.dialoguePanel.transform;
            var stage=v.gameObject.AddComponent<HotelStagePresentation>();v.stagePresentation=stage;
            // One transparent interaction surface; no panels behind either character.
            var clickArea=Box(p,"Click anywhere to continue",0,0,1600,900,Color.clear);clickArea.raycastTarget=true;
            v.dialogueNext=clickArea.gameObject.AddComponent<Button>();v.dialogueNext.targetGraphic=clickArea;
            v.dialogueNext.transition=Selectable.Transition.None;
            stage.leftActor=Box(p,"Seoyun · editable portrait slot",155,50,500,690,Color.white);stage.leftActor.preserveAspect=true;
            stage.rightActor=Box(p,"Conversation partner · editable portrait slot",905,50,500,690,Color.white);stage.rightActor.preserveAspect=true;
            stage.leftGroup=stage.leftActor.gameObject.AddComponent<CanvasGroup>();stage.rightGroup=stage.rightActor.gameObject.AddComponent<CanvasGroup>();
            var card=Box(p,"Dialogue box · editable anchored layout",96,655,1408,209,Hex("EDE3CB"),true);
            stage.dialogueCard=card.rectTransform;stage.dialogueGroup=card.gameObject.AddComponent<CanvasGroup>();
            Box(card.transform,"Inset rule",28,18,1352,2,Hex("B99B65"));
            var namePlate=Box(card.transform,"Speaker nameplate",24,-48,280,58,Hex("31474B"),true);
            v.speakerName=Text(namePlate.transform,"Speaker name","하린",20,8,240,39,28,Paper,FontStyle.Bold);
            v.speakerRole=Text(card.transform,"Role","프런트 직원",330,-32,700,28,18,Paper);
            v.dialogueText=Text(card.transform,"Dialogue · stable typewriter mesh","잔월 호텔에 오신 것을 환영해요.",40,38,1300,106,30,Ink);
            v.dialogueText.lineSpacing=1.15f;
            stage.dialogueText=v.dialogueText;stage.textReveal=v.dialogueText.gameObject.AddComponent<HotelTextReveal>();
            v.dialogueCounter=Text(card.transform,"Keyboard hint","클릭 / Space · 계속",40,170,1150,24,15,Hex("766A55"));
            stage.continueMark=(RectTransform)Text(card.transform,"Continue indicator","▼",1316,163,50,35,27,Hex("68543B"),FontStyle.Bold,TextAnchor.MiddleCenter).transform;
            // Old portrait fields are obsolete and intentionally unassigned.
            v.portraitColor=null;v.portraitImage=null;v.portraitInitial=null;
        }
    }
}
