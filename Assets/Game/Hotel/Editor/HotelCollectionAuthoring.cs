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
        [MenuItem("Puzzle Spire/수집품 보관함 적용 (최초 1회)")]
        public static void UpgradeCollection()
        {
            EditorSceneManager.OpenScene(ScenePath);v=UnityEngine.Object.FindFirstObjectByType<HotelView>();
            if(v.collection!=null){Debug.Log("Collection already authored; preserving manual changes.");return;}
            Directory.CreateDirectory("Builds/Backups");
            File.Copy(ScenePath,"Builds/Backups/Hotel-before-collection-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity");
            root=v.titleScreen.transform.parent;
            font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            rounded=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/RoundedPanel.png");
            var sprites=ImportAtlas("CollectionEvidence",4,2);
            int sibling=v.notebookScreen.transform.GetSiblingIndex();
            UnityEngine.Object.DestroyImmediate(v.notebookScreen);
            var ui=v.gameObject.AddComponent<HotelCollectionView>();v.collection=ui;
            ui.illustrationIds=new[]{"handover","moon_tag","phone_log","voice_note","old_plan","photo","letter"};
            ui.illustrations=new Sprite[7];Array.Copy(sprites,ui.illustrations,7);ui.fallback=sprites[7];
            v.notebookScreen=Box(root,"05 Collection · evidence cabinet",0,0,1600,900,Hex("1A292C")).gameObject;
            v.notebookScreen.transform.SetSiblingIndex(sibling);var p=v.notebookScreen.transform;
            var background=Box(p,"Hotel desk atmosphere",0,0,1600,900,new Color(.38f,.43f,.43f));background.sprite=v.locationImages[2].sprite;
            Box(p,"Reading shade",0,0,1600,900,new Color(.025f,.055f,.06f,.69f));
            Text(p,"Eyebrow","SEOYUN'S FIELD NOTES",74,34,800,25,16,Gold);
            Text(p,"Collection title","수집한 단서",70,68,1000,61,43,Paper,FontStyle.Bold);
            ui.invitation=Text(p,"Collection invitation","작은 흔적 하나에도, 누군가의 이야기가 남아 있다.",74,140,1120,28,19,Muted);
            v.clueCount=Text(p,"Collection count","00  /  09  기록",1240,82,280,43,25,Paper,FontStyle.Normal,TextAnchor.MiddleRight);
            Box(p,"Progress track",1270,139,252,3,Hex("425151"));
            ui.progressFill=Box(p,"Progress · collected records",1270,139,252,3,Gold);ui.progressFill.sprite=rounded;
            ui.progressFill.type=Image.Type.Filled;ui.progressFill.fillMethod=Image.FillMethod.Horizontal;ui.progressFill.fillAmount=0;
            v.clueButtons=new Button[9];ui.cardImages=new Image[9];ui.cardFaces=new Image[9];ui.cardTitles=new Text[9];ui.cardStates=new Text[9];ui.cardNumbers=new Text[9];
            for(int i=0;i<9;i++)
            {
                float x=72+i%3*492,y=192+i/3*212;
                var button=Button(p,"Evidence slot "+i,"",x,y,468,194,Hex("E8DBC0"));v.clueButtons[i]=button;
                UnityEngine.Object.DestroyImmediate(button.transform.GetChild(0).gameObject);
                ui.cardFaces[i]=button.GetComponent<Image>();
                var colors=button.colors;colors.disabledColor=Color.white;colors.highlightedColor=new Color(1.08f,1.06f,.99f);colors.selectedColor=Color.white;button.colors=colors;
                ui.cardNumbers[i]=Text(button.transform,"Record number",(i+1).ToString("D2"),18,14,65,28,17,Hex("A08960"));
                Box(button.transform,"Fine top rule",388,24,60,1,Hex("BAA580"));
                ui.cardImages[i]=Box(button.transform,"Evidence artwork",134,12,200,126,Color.white);ui.cardImages[i].preserveAspect=true;
                ui.cardTitles[i]=Text(button.transform,"Evidence title","아직 발견하지 못한 기록",18,143,432,31,22,Ink,FontStyle.Bold,TextAnchor.MiddleCenter);
                ui.cardStates[i]=Text(button.transform,"Evidence state","조사를 이어가면 채워집니다",18,175,432,19,13,Hex("827456"),FontStyle.Normal,TextAnchor.MiddleCenter);
            }
            Text(p,"Footer inscription","잔월 호텔  ·  발견한 것들을 잊지 않도록",74,844,840,28,16,Muted);
            ui.previousPage=Button(p,"Previous records","←",900,837,60,42,null,22);
            ui.pageLabel=Text(p,"Page","1 / 1",972,844,96,26,18,Gold,FontStyle.Normal,TextAnchor.MiddleCenter);
            ui.nextPage=Button(p,"Next records","→",1080,837,60,42,null,22);
            v.notebookBack=Button(p,"Close collection","조사로 돌아가기",1256,834,268,48,Hex("6D5A40"),21);
            ui.detailPanel=Box(p,"Evidence detail · full overlay",0,0,1600,900,new Color(.02f,.035f,.04f,.92f)).gameObject;ui.detailPanel.GetComponent<Image>().raycastTarget=true;
            var paper=Box(ui.detailPanel.transform,"Evidence folio · editable",314,74,972,752,Hex("EEE4CD"),true).transform;
            ui.detailNumber=Text(paper,"Record index","RECORD  01",70,26,832,25,15,Hex("88734F"));
            Box(paper,"Header rule",70,57,832,1,Hex("BCAA85"));
            v.clueTitle=Text(paper,"Object title","흰 실 한 가닥",70,75,832,55,35,Ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            ui.origin=Text(paper,"Discovery location","호텔 로비에서 발견 · 관찰 기록",70,137,832,30,18,Hex("807053"),FontStyle.Normal,TextAnchor.MiddleCenter);
            var mat=Box(paper,"Object display mat",136,187,700,299,Hex("E4D7BA"),true);
            v.investigation.notebookImage=Box(mat.transform,"Evidence image · MOD aware",145,12,410,275,Color.white);v.investigation.notebookImage.preserveAspect=true;
            v.clueDescription=Text(paper,"Object description","",84,512,804,132,26,Ink);v.clueDescription.lineSpacing=1.16f;
            Text(paper,"Folio signature","서윤의 조사 수첩",70,684,500,28,17,Hex("897650"));
            ui.closeDetail=Button(paper,"Back to collection","보관함으로",706,672,196,49,Hex("435B58"),20);
            ui.detailPanel.SetActive(false);v.Show(v.titleScreen);
            EditorUtility.SetDirty(v);EditorUtility.SetDirty(ui);EditorUtility.SetDirty(v.investigation);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_COLLECTION_AUTHORED");
        }
        public static void UpgradeCollectionAndBuild(){UpgradeCollection();CreateAndBuild();}
    }
}
