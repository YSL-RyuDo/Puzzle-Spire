using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel.Editor
{
    /// <summary>One-time authoring tool. It saves real editable scene objects and never runs in a player.</summary>
    public static partial class HotelSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/Hotel.unity";
        private static readonly Color Ink = Hex("172D37"), Panel = Hex("233E47"), Teal = Hex("315C60"), Gold = Hex("D6B67A"), Paper = Hex("F1E8D3"), Muted = Hex("A9BDC0");
        private static Font font;
        private static Sprite rounded, circle;
        private static HotelView v;
        private static Transform root;

        [MenuItem("Puzzle Spire/호텔 프로토타입 열기")]
        public static void Open()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            else EditorSceneManager.OpenScene(ScenePath);
        }
        // Batch entry point. Existing authored scenes are ALWAYS preserved.
        public static void CreateAndBuild()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            else EditorSceneManager.OpenScene(ScenePath);
            ApplyBundledFont();
            FixInitialShapeImports();
            var pack = HotelStorage.ReadJson<HotelPack>("Assets/StreamingAssets/Hotel/base.json");
            ContentValidation.Validate(pack);
            Directory.CreateDirectory("Builds/Hotel");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Hotel/MoonlitHotel.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Hotel build failed: " + report.summary.result);
            Directory.CreateDirectory("Builds/Hotel/ThirdPartyNotices");
            File.Copy("Assets/Game/Hotel/Art/Fonts/OFL.txt", "Builds/Hotel/ThirdPartyNotices/NanumGothic-OFL.txt", true);
            Debug.Log("HOTEL_BUILD_PASSED: " + report.summary.outputPath);
        }
        public static void CreateScene()
        {
            if (File.Exists(ScenePath)) throw new InvalidOperationException("기존 Hotel 씬을 덮어쓰지 않습니다. Hierarchy에서 직접 편집하세요.");
            Directory.CreateDirectory("Assets/Game/Hotel/Art");
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            if (font == null) throw new InvalidOperationException("번들 한글 폰트를 찾을 수 없습니다.");
            rounded = Shape("RoundedPanel", false); circle = Shape("Circle", true);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Ink; camera.orthographic = true; cameraObject.tag = "MainCamera";
            var canvasObject = new GameObject("Hotel Canvas · EDIT UI HERE", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600,900); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            root = Box(canvasObject.transform, "Safe Area · 1600x900", 0,0,1600,900,Ink).transform;
            var safeRect = (RectTransform)root; safeRect.anchorMin = safeRect.anchorMax = new Vector2(.5f,.5f); safeRect.pivot = new Vector2(.5f,.5f); safeRect.anchoredPosition = Vector2.zero;
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var systems = new GameObject("Hotel Systems · references", typeof(HotelView), typeof(HotelGame), typeof(AudioSource));
            v = systems.GetComponent<HotelView>();
            var audio = systems.GetComponent<AudioSource>(); audio.playOnAwake = false;
            var serialized = new SerializedObject(systems.GetComponent<HotelGame>());
            serialized.FindProperty("view").objectReferenceValue = v;
            serialized.FindProperty("audioSource").objectReferenceValue = audio; serialized.ApplyModifiedPropertiesWithoutUndo();
            Title(); Explore(); Puzzle(); Riddle(); Notebook(); Catalog(); Settings(); Ending(); Dialogue(); Result();
            v.Show(v.titleScreen);
            EditorUtility.SetDirty(v);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene("Assets/Game/Scenes/PuzzleTest.unity", false), new EditorBuildSettingsScene("Assets/Game/Scenes/SampleScene.unity", false) };
            PlayerSettings.productName = "Moonlit Hotel";
            PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_SCENE_CREATED: all UI is editable in the scene hierarchy.");
        }
        private static void ApplyBundledFont()
        {
            var bundled = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            bool changed = false;
            foreach (var sceneRoot in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var label in sceneRoot.GetComponentsInChildren<Text>(true))
            {
                // Migrate only the initial placeholder font. Preserve any designer-assigned replacement.
                if (label.font != null && AssetDatabase.GetAssetPath(label.font) == "Assets/Game/Hotel/Art/HotelKorean.fontsettings")
                { label.font = bundled; EditorUtility.SetDirty(label); changed = true; }
            }
            if (changed) EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        private static void FixInitialShapeImports()
        {
            foreach (string name in new[] { "RoundedPanel", "Circle" })
            {
                string path = "Assets/Game/Hotel/Art/" + name + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null || importer.spriteImportMode == SpriteImportMode.Single) continue;
                var targets = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Image>(true))
                    .Where(i => i.sprite != null && AssetDatabase.GetAssetPath(i.sprite) == path).ToArray();
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                foreach (var target in targets) { target.sprite = sprite; EditorUtility.SetDirty(target); }
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
        }
        private static Sprite Shape(string name, bool disk)
        {
            const int size = 64; var texture = new Texture2D(size,size,TextureFormat.RGBA32,false);
            for (int y=0;y<size;y++) for (int x=0;x<size;x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x-31.5f)-20,0), dy=Mathf.Max(Mathf.Abs(y-31.5f)-20,0);
                float distance = disk ? Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))-30 : Mathf.Sqrt(dx*dx+dy*dy)-11;
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(.5f-distance)));
            }
            texture.Apply(); string path="Assets/Game/Hotel/Art/"+name+".png"; File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.spriteBorder=new Vector4(16,16,16,16); importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static Color Hex(string s) { ColorUtility.TryParseHtmlString("#"+s,out var c);return c; }
        private static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rt=go.GetComponent<RectTransform>(); rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        private static Image Box(Transform parent,string name,float x,float y,float w,float h,Color color,bool soft=false)
        {
            var image=Rect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;
            if(soft){image.sprite=rounded;image.type=Image.Type.Sliced;}return image;
        }
        private static Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size=24,Color? color=null,FontStyle style=FontStyle.Normal,TextAnchor align=TextAnchor.UpperLeft)
        {
            var t=Rect(parent,name,x,y,w,h).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color??Paper;t.fontStyle=style;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.supportRichText=false;return t;
        }
        private static Button Button(Transform parent,string name,string label,float x,float y,float w,float h,Color? color=null,int fontSize=23)
        {
            var image=Box(parent,name,x,y,w,h,color??Teal,true);image.raycastTarget=true;
            var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var colors=b.colors;colors.highlightedColor=new Color(1.18f,1.18f,1.18f,1);colors.pressedColor=new Color(.75f,.85f,.83f,1);colors.disabledColor=new Color(.58f,.62f,.64f,.6f);b.colors=colors;
            var text=Text(b.transform,"Label · editable",label,16,4,w-32,h-8,fontSize,Paper,FontStyle.Normal,TextAnchor.MiddleCenter);
            var tr=(RectTransform)text.transform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=new Vector2(12,4);tr.offsetMax=new Vector2(-12,-4);
            return b;
        }
        private static GameObject Screen(string name)
        {
            var go=Box(root,name,0,0,1600,900,Ink).gameObject;
            Box(go.transform,"Top gold rule",48,40,1504,2,Gold);Text(go.transform,"Brand","잔월 호텔  /  MOONLIT HOTEL",48,56,900,32,19,Gold);
            return go;
        }
        private static void Title()
        {
            v.titleScreen=Screen("01 Title");var p=v.titleScreen.transform;
            var art=Box(p,"Hotel illustration · editable shapes",640,108,908,700,Hex("233E47"),true); LobbyArt(art.transform,908,700);
            Text(p,"Edition","A SMALL MYSTERY, AFTER THE RAIN",68,160,560,34,18,Gold);
            Text(p,"Game title","잔월 호텔",62,211,600,90,76,Paper,FontStyle.Bold);
            Text(p,"Subtitle","사람을 기다리는 방",70,316,550,42,30,Gold);
            Text(p,"Invitation","사라진 열쇠 하나.\n빈 객실에서 울리는 전화.\n오래된 호텔에 남겨진 약속을 찾아보세요.",72,390,540,118,25,Muted);
            v.continueButton=Button(p,"Continue","이어하기",72,550,250,62);
            v.newButton=Button(p,"New game","새 조사 시작",340,550,250,62,Hex("766247"));
            v.practiceButton=Button(p,"Practice","퍼즐 연습",72,628,250,52);
            v.modsButton=Button(p,"MOD manager","MOD 관리",340,628,250,52);
            v.settingsButton=Button(p,"Settings","설정",72,696,250,48);
            v.quitButton=Button(p,"Quit","종료",340,696,250,48);
            v.titleStatus=Text(p,"Save and pack status","기본 이야기",72,778,1350,76,18,Muted);
        }
        private static void Explore()
        {
            v.exploreScreen=Screen("02 Investigation");var p=v.exploreScreen.transform;
            v.chapter=Text(p,"Chapter","01 · 사라진 303호 열쇠",48,108,1130,46,32,Paper,FontStyle.Bold);
            v.exploreTitleButton=Button(p,"Return to title","타이틀",1412,100,140,48);
            var art=Box(p,"Location canvas",48,183,1090,557,Panel,true);
            v.locationArt=new GameObject[3];v.locationImages=new Image[3];
            for(int i=0;i<3;i++)
            {
                var layer=Box(art.transform,new[]{"Lobby · editable illustration","Corridor · editable illustration","Archive · editable illustration"}[i],0,0,1090,557,Panel);v.locationArt[i]=layer.gameObject;
                if(i==0)LobbyArt(layer.transform,1090,557);else if(i==1)CorridorArt(layer.transform);else ArchiveArt(layer.transform);
                v.locationImages[i]=Box(art.transform,"MOD background "+i,0,0,1090,557,Color.white);v.locationImages[i].preserveAspect=false;v.locationImages[i].gameObject.SetActive(false);
            }
            v.hotspot=Button(art.transform,"Investigation hotspot · position from content","＋ 프런트",370,300,330,66,Hex("355C59"),23);
            var hr=(RectTransform)v.hotspot.transform;hr.pivot=new Vector2(.5f,.5f);
            var sidebar=Box(p,"Investigation card",1160,183,392,557,Panel,true);
            Text(sidebar.transform,"Eyebrow","현재의 질문",28,30,330,30,19,Gold);
            v.objective=Text(sidebar.transform,"Objective","하린에게 체크인을 요청하자.",28,77,330,132,29,Paper,FontStyle.Bold);
            Box(sidebar.transform,"Divider",28,229,336,2,Teal);
            v.hotspotTitle=Text(sidebar.transform,"Action title","프런트의 하린",28,258,330,80,25,Gold);
            v.hotspotDescription=Text(sidebar.transform,"Action description","반짝이는 조사 지점을 눌러보세요.",28,338,330,118,21,Muted);
            v.nextLocation=Button(sidebar.transform,"Go to next location","다음 장소 →",28,473,336,56);
            v.locationTitle=Text(p,"Location title","호텔 로비",48,757,450,40,29,Paper,FontStyle.Bold);
            v.locationDescription=Text(p,"Location subtitle","비가 그친 뒤",48,803,640,52,18,Muted);
            v.locationButtons=new Button[3];
            for(int i=0;i<3;i++)v.locationButtons[i]=Button(p,"Location "+i,new[]{"로비","복도","보관실"}[i],700+i*188,787,176,54,null,20);
            v.notebookButton=Button(p,"Notebook","조사 수첩",1280,787,272,54,Hex("766247"));
            v.progressText=Text(p,"Progress","조사 0 / 20",1160,750,392,28,18,Muted);
        }
        private static void Puzzle()
        {
            v.puzzleScreen=Screen("03 Block puzzle");var p=v.puzzleScreen.transform;
            v.puzzleTitle=Text(p,"Stage title","열쇠 보관함",48,110,1000,45,32,Paper,FontStyle.Bold);
            v.puzzlePurpose=Text(p,"Narrative purpose","조사할 곳을 정리해 단서를 찾아보자.",48,162,1400,38,21,Muted);
            var left=Box(p,"Objectives card",48,227,280,551,Panel,true);
            Text(left.transform,"Moves label","남은 조사 횟수",26,28,228,32,21,Gold);
            v.movesText=Text(left.transform,"Moves","24",26,70,230,100,72,Paper,FontStyle.Bold);
            Box(left.transform,"Rule",26,184,228,2,Teal);
            Text(left.transform,"Goals label","조사 목표",26,210,228,32,21,Gold);
            v.goalsText=Text(left.transform,"Goals","산호 블록\n0 / 12",26,262,228,265,27);
            var board=Box(p,"Board · 81 authored cells",356,216,640,640,Hex("10232B"),true);
            v.boardLayout=board.gameObject.AddComponent<GridLayoutGroup>();v.boardLayout.padding=new RectOffset(8,8,8,8);v.boardLayout.spacing=new Vector2(6,6);v.boardLayout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;v.boardLayout.constraintCount=7;v.boardLayout.cellSize=new Vector2(83,83);v.boardLayout.childAlignment=TextAnchor.MiddleCenter;
            v.cells=new HotelCellView[81];
            for(int i=0;i<81;i++)
            {
                var button=Button(board.transform,"Cell "+i.ToString("00"),"",0,0,80,80);
                UnityEngine.Object.DestroyImmediate(button.GetComponentInChildren<Text>().gameObject);
                var cell=button.gameObject.AddComponent<HotelCellView>();cell.button=button;cell.face=button.image;
                cell.customImage=Box(button.transform,"MOD block image",4,4,72,72,Color.white,true);Stretch(cell.customImage.rectTransform,4);
                cell.symbol=Text(button.transform,"Block symbol","◆",0,0,80,80,34,Paper,FontStyle.Bold,TextAnchor.MiddleCenter);Stretch((RectTransform)cell.symbol.transform,0);
                cell.obstacleBorder=Box(button.transform,"Obstacle overlay",3,3,74,74,Teal,true);Stretch(cell.obstacleBorder.rectTransform,3);
                cell.obstacleText=Text(button.transform,"Obstacle label","",0,0,80,80,18,Paper,FontStyle.Bold,TextAnchor.MiddleCenter);Stretch((RectTransform)cell.obstacleText.transform,0);
                v.cells[i]=cell;
            }
            var right=Box(p,"How to investigate",1024,227,528,388,Panel,true);
            Text(right.transform,"Help label","플레이 안내",28,27,460,35,24,Gold);
            v.puzzleHelp=Text(right.transform,"Rules","같은 색을 클릭하세요.",28,83,465,280,23,Muted);
            v.comboText=Text(p,"Turn feedback","목표를 확인하세요.",1024,638,528,100,23,Gold);
            v.puzzleHintButton=Button(p,"Find a move","힌트",1024,751,160,54);
            v.retryButton=Button(p,"Retry stage","재도전",1200,751,160,54);
            v.puzzleBackButton=Button(p,"Exit stage","돌아가기",1376,751,176,54,Hex("766247"));
        }
        private static void Riddle()
        {
            v.riddleScreen=Screen("04 Riddle");var p=v.riddleScreen.transform;
            v.riddleTitle=Text(p,"Riddle title","세 번의 이동",48,108,1200,48,34,Paper,FontStyle.Bold);
            var paper=Box(p,"Question paper",48,187,664,484,Paper,true);
            Text(paper.transform,"Question eyebrow","조사 기록에서 찾는 답",32,31,600,35,22,Hex("806B44"));
            v.question=Text(paper.transform,"Question","문제를 읽어보세요.",32,99,600,337,29,Ink);
            v.options=new Button[6];
            for(int i=0;i<6;i++)v.options[i]=Button(p,"Answer option "+i,"선택지",752,187+i*78,800,67,Panel,23);
            v.selectionText=Text(p,"Selection instruction","순서대로 선택하세요.",752,674,800,92,22,Gold);
            v.feedbackText=Text(p,"Hint and feedback","힌트를 펼쳐 보세요.",48,692,664,159,23,Muted);
            v.riddleHint=Button(p,"Hint","힌트",752,791,170,58);
            v.resetAnswer=Button(p,"Reset selection","다시 선택",938,791,170,58);
            v.riddleBack=Button(p,"Return to investigation","돌아가기",1124,791,188,58);
            v.submitAnswer=Button(p,"Submit answer","정답 확인",1328,791,224,58,Hex("766247"));
        }
        private static void Notebook()
        {
            v.notebookScreen=Screen("05 Notebook");var p=v.notebookScreen.transform;
            Text(p,"Title","조사 수첩",48,108,900,45,34,Paper,FontStyle.Bold);
            v.clueCount=Text(p,"Clue count","0 / 8개의 단서",48,159,900,32,20,Gold);
            v.clueButtons=new Button[12];
            for(int i=0;i<12;i++)v.clueButtons[i]=Button(p,"Clue slot "+i,"발견하지 못한 단서",48,216+i*49,490,43,Panel,20);
            var paper=Box(p,"Clue detail paper",586,216,966,562,Paper,true);
            v.clueTitle=Text(paper.transform,"Clue title","조사 수첩",48,47,864,86,36,Ink,FontStyle.Bold);
            Box(paper.transform,"Rule",48,148,870,2,Gold);
            v.clueDescription=Text(paper.transform,"Clue description","단서를 선택해 주세요.",48,182,870,300,29,Ink);
            v.notebookBack=Button(p,"Close notebook","돌아가기",1280,797,272,54);
        }
        private static void Catalog()
        {
            v.catalogScreen=Screen("06 MOD and practice catalog");var p=v.catalogScreen.transform;
            v.catalogTitle=Text(p,"Catalog title","MOD 관리",48,108,1100,45,34,Paper,FontStyle.Bold);
            v.catalogDescription=Text(p,"Catalog description","MOD별로 저장됩니다.",48,170,1450,102,20,Muted);
            v.catalogRows=new Button[6];
            for(int i=0;i<6;i++)v.catalogRows[i]=Button(p,"Catalog row "+i,"기본 이야기",48,292+i*70,1504,62,Panel,21);
            v.catalogFolder=Button(p,"Open MOD folder","MOD 폴더 열기",48,733,255,50);
            v.catalogRefresh=Button(p,"Refresh MODs","목록 새로고침",321,733,255,50);
            v.catalogSample=Button(p,"Install example MOD","샘플 MOD 설치",594,733,255,50,Hex("766247"));
            v.catalogPrevious=Button(p,"Previous page","←",1010,733,72,50);
            v.catalogPage=Text(p,"Page","1 / 1",1092,744,146,35,22,Gold,FontStyle.Normal,TextAnchor.MiddleCenter);
            v.catalogNext=Button(p,"Next page","→",1250,733,72,50);
            v.catalogBack=Button(p,"Back to title","타이틀",1340,733,212,50);
            Text(p,"Pack instructions","MOD를 선택하면 바로 적용됩니다. 새 파일을 넣은 뒤에는 목록을 새로고침하세요.",48,815,1480,40,19,Muted);
        }
        private static void Settings()
        {
            v.settingsScreen=Screen("07 Settings");var p=v.settingsScreen.transform;
            Text(p,"Title","설정과 조작 안내",48,108,1100,48,34,Paper,FontStyle.Bold);
            v.settingsInfo=Text(p,"Controls and save location","조작 안내",72,211,980,570,26,Muted);
            v.soundButton=Button(p,"Sound toggle","효과음: 켜짐",1120,230,400,64);
            v.fullscreenButton=Button(p,"Fullscreen toggle","전체 화면 / 창 모드 전환",1120,316,400,64);
            v.settingsBack=Button(p,"Back","타이틀",1120,753,400,64,Hex("766247"));
        }
        private static void Ending()
        {
            v.endingScreen=Screen("08 Ending");var p=v.endingScreen.transform;
            Text(p,"Ending eyebrow","EPILOGUE",96,192,1300,42,23,Gold);
            Text(p,"Ending title","사람을 기다리는 방",90,263,1400,88,62,Paper,FontStyle.Bold);
            v.endingSummary=Text(p,"Ending text","서재가 다시 열렸습니다.",96,404,1340,290,28,Muted);
            v.endingNotebook=Button(p,"Review clues","단서 다시 보기",96,746,330,66);
            v.endingTitle=Button(p,"Return to title","타이틀로",448,746,330,66,Hex("766247"));
        }
        private static void Dialogue()
        {
            v.dialoguePanel=Box(root,"09 Dialogue overlay",0,0,1600,900,new Color(0,0,0,.36f)).gameObject;v.dialoguePanel.GetComponent<Image>().raycastTarget=true;
            var p=Box(v.dialoguePanel.transform,"Dialogue card",64,548,1472,292,Paper,true).transform;
            v.portraitColor=Box(p,"Portrait card · editable placeholder",24,24,188,244,Teal,true);
            v.portraitInitial=Text(v.portraitColor.transform,"Portrait initial","하",0,30,188,152,92,Paper,FontStyle.Bold,TextAnchor.MiddleCenter);
            Text(v.portraitColor.transform,"Portrait note","HOTEL GUEST",8,206,172,24,13,Paper,FontStyle.Normal,TextAnchor.MiddleCenter);
            v.portraitImage=Box(v.portraitColor.transform,"MOD portrait",0,0,188,244,Color.white);v.portraitImage.preserveAspect=true;
            v.speakerName=Text(p,"Speaker","하린",244,24,260,42,30,Ink,FontStyle.Bold);
            v.speakerRole=Text(p,"Role","프런트 직원",520,30,680,36,19,Hex("776645"));
            v.dialogueText=Text(p,"Dialogue text","잔월 호텔에 오신 것을 환영해요.",244,89,1168,115,29,Ink);
            v.dialogueCounter=Text(p,"Line count","1 / 3",244,229,250,35,18,Hex("776645"));
            v.dialogueNext=Button(p,"Next dialogue","다음 →",1192,220,244,52);
        }
        private static void Result()
        {
            v.resultPanel=Box(root,"10 Result and confirmation overlay",0,0,1600,900,new Color(.02f,.07f,.09f,.85f)).gameObject;v.resultPanel.GetComponent<Image>().raycastTarget=true;
            var p=Box(v.resultPanel.transform,"Result card",300,173,1000,566,Paper,true).transform;
            v.resultEyebrow=Text(p,"Eyebrow","조사 완료",44,38,910,32,20,Hex("806B44"));
            v.resultTitle=Text(p,"Title","기록이 이어졌습니다",44,91,910,82,38,Ink,FontStyle.Bold);
            v.resultBody=Text(p,"Body","새로운 단서를 찾았습니다.",44,190,910,233,26,Ink);
            v.resultPrimary=Button(p,"Primary action","계속",44,460,442,62);
            v.resultSecondary=Button(p,"Secondary action","취소",508,460,448,62,Hex("766247"));
        }
        private static void Stretch(RectTransform rt,float inset)
        { rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(inset,inset);rt.offsetMax=new Vector2(-inset,-inset); }

        // Original geometric scenery: each shape remains editable in the Hierarchy.
        private static void LobbyArt(Transform p,float w,float h)
        {
            Box(p,"Wall",0,0,w,h,Hex("2D4E53"));Box(p,"Floor",0,h*.72f,w,h*.28f,Hex("35424A"));
            for(int i=0;i<7;i++)Box(p,"Wall panel "+i,i*w/6,0,3,h*.72f,Hex("476566"));
            Box(p,"Cornice",0,h*.13f,w,6,Gold);Box(p,"Skirting",0,h*.70f,w,9,Hex("A38A63"));
            var window=Box(p,"Arched window",w*.69f,h*.20f,w*.23f,h*.43f,Hex("9B8867"),true);
            Box(window.transform,"Night glass",9,9,w*.23f-18,h*.43f-18,Hex("183747"),true);
            var moon=Box(window.transform,"Moon",w*.11f,h*.04f,54,54,Gold);moon.sprite=circle;
            Box(window.transform,"Window mullion",w*.115f,8,6,h*.43f-16,Hex("9B8867"));Box(window.transform,"Crossbar",8,h*.23f,w*.23f-16,6,Hex("9B8867"));
            var keyrack=Box(p,"Key rack",w*.40f,h*.27f,w*.19f,h*.26f,Hex("725D43"),true);
            for(int y=0;y<3;y++)for(int x=0;x<4;x++)
            { var key=Box(keyrack.transform,"Key hook "+x+"-"+y,18+x*w*.037f,18+y*h*.069f,12,12,Gold);key.sprite=circle;Box(keyrack.transform,"Key stem "+x+"-"+y,22+x*w*.037f,26+y*h*.069f,4,16,Gold); }
            Box(p,"Front desk",w*.17f,h*.57f,w*.48f,h*.24f,Hex("785C46"),true);
            Box(p,"Desk top",w*.15f,h*.55f,w*.52f,17,Hex("CFB281"),true);
            Box(p,"Desk inset",w*.20f,h*.63f,w*.42f,h*.11f,Hex("604B3D"),true);
            Text(p,"Hotel plaque","잔 월",w*.33f,h*.64f,w*.17f,h*.10f,28,Gold,FontStyle.Bold,TextAnchor.MiddleCenter);
            Box(p,"Bell base",w*.26f,h*.525f,43,14,Gold,true);var bell=Box(p,"Bell",w*.266f,h*.488f,32,32,Gold);bell.sprite=circle;
            Box(p,"Ledger",w*.51f,h*.51f,66,22,Paper,true);
            Box(p,"Rug",w*.23f,h*.87f,w*.53f,h*.06f,Hex("806353"),true);
            Text(p,"Location inscription","AFTER THE RAIN",28,28,w*.64f,28,17,Gold);
            // A framed clock with a deliberately simple face.
            var clock=Box(p,"Clock",w*.12f,h*.24f,84,84,Gold);clock.sprite=circle;
            var dial=Box(clock.transform,"Clock face",7,7,70,70,Paper);dial.sprite=circle;
            Box(clock.transform,"Clock hand vertical",40,17,4,28,Ink);Box(clock.transform,"Clock hand horizontal",40,40,23,4,Ink);
        }
        private static void CorridorArt(Transform p)
        {
            Box(p,"Wall",0,0,1090,557,Hex("344D59"));Box(p,"Floor",0,410,1090,147,Hex("46464A"));Box(p,"Gold rail",0,340,1090,6,Gold);
            for(int i=0;i<3;i++)
            {
                float x=90+i*306;Box(p,"Door frame "+i,x,88,184,322,Hex("947B58"),true);Box(p,"Door "+i,x+10,98,164,312,Hex("3C383A"),true);
                Text(p,"Room number "+i,(301+i).ToString(),x+34,123,116,50,28,Gold,FontStyle.Bold,TextAnchor.MiddleCenter);
                var knob=Box(p,"Handle "+i,x+141,262,13,13,Gold);knob.sprite=circle;
            }
            Box(p,"Runner",30,451,1030,61,Hex("826653"),true);
            Box(p,"Blue basket",590,385,128,88,Hex("4B8191"),true);Box(p,"Folded linen",600,375,108,26,Paper,true);
            Box(p,"Phone table",890,330,136,103,Hex("73573D"),true);Box(p,"Telephone base",910,308,98,27,Ink,true);Box(p,"Telephone receiver",904,294,110,17,Gold,true);
            Text(p,"Inscription","3F  /  객실 복도",30,28,500,30,20,Gold);
        }
        private static void ArchiveArt(Transform p)
        {
            Box(p,"Wall",0,0,1090,557,Hex("4B514D"));Box(p,"Floor",0,425,1090,132,Hex("45413C"));
            for(int k=0;k<2;k++)
            {
                int start=86+k*565;Box(p,"Shelf back "+k,start,86,340,329,Hex("35413C"),true);
                for(int y=0;y<3;y++)
                {
                    Box(p,"Shelf "+k+"-"+y,start-8,162+y*99,356,12,Hex("A3875C"));
                    for(int x=0;x<7;x++)Box(p,"Book "+k+"-"+y+"-"+x,start+18+x*43,104+y*99,30,58,(x+y)%2==0?Hex("748679"):Hex("AC8B68"),true);
                }
            }
            Box(p,"Writing table",348,348,399,136,Hex("806449"),true);Box(p,"Table surface",328,333,439,20,Gold,true);
            var paper=Box(p,"Old plan",456,300,172,35,Paper,true);Text(paper.transform,"Plan marks","—  ◇  —",0,0,172,35,20,Ink,FontStyle.Normal,TextAnchor.MiddleCenter);
            for(int i=0;i<3;i++)Box(p,"Storage box "+i,140+i*94,442-i%2*28,85,70,Hex("A38660"),true);
            Text(p,"Inscription","ARCHIVE  /  남겨진 기록",30,28,750,30,20,Gold);
        }
    }
}
