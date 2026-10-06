using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel.Editor
{
    public static partial class HotelSceneBuilder
    {
        [MenuItem("Puzzle Spire/조사와 퍼즐 연출 적용 (최초 1회)")]
        public static void UpgradeInvestigation()
        {
            EditorSceneManager.OpenScene(ScenePath);
            v = UnityEngine.Object.FindFirstObjectByType<HotelView>();
            if (v.investigation != null) { Debug.Log("Investigation presentation already authored; preserving scene edits."); return; }
            Directory.CreateDirectory("Builds/Backups");
            File.Copy(ScenePath, "Builds/Backups/Hotel-before-investigation-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
            root = v.titleScreen.transform.parent;
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Game/Hotel/Art/Fonts/NanumGothic-Regular.ttf");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/RoundedPanel.png");
            circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Hotel/Art/Circle.png");
            var ui = v.gameObject.AddComponent<HotelInvestigationView>(); v.investigation = ui;
            ui.pieces = ImportAtlas("InvestigationPieces", 4, 4);
            ui.poses = ImportAtlas("InvestigationPoses", 2, 2);
            ui.cabinet = ImportBackground("KeyCabinet");
            ui.effectDisk = circle;
            var game = new SerializedObject(v.GetComponent<HotelGame>());
            var blocks = game.FindProperty("defaultBlockSprites"); blocks.arraySize = 5;
            for (int i = 0; i < 5; i++) blocks.GetArrayElementAtIndex(i).objectReferenceValue = ui.pieces[i];
            game.FindProperty("turnAnimationSeconds").floatValue = .16f;
            game.ApplyModifiedPropertiesWithoutUndo();
            AuthorPuzzle(ui); AuthorInspection(ui); AuthorProps(ui);
            var paper = v.clueDescription.transform.parent;
            ui.notebookImage = Box(paper, "Evidence illustration", 50, 185, 325, 310, Color.white);
            ui.notebookImage.preserveAspect = true;
            v.clueDescription.fontSize = 25;
            v.Show(v.titleScreen);
            EditorUtility.SetDirty(v); EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("HOTEL_INVESTIGATION_AUTHORED");
        }
        public static void UpgradeInvestigationAndBuild() { UpgradeInvestigation(); CreateAndBuild(); }

        private static Sprite[] ImportAtlas(string name, int columns, int rows)
        {
            string path = "Assets/Game/Hotel/Art/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.isReadable = true; importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            var sprites = new Sprite[columns * rows];
            int cw = texture.width / columns, ch = texture.height / rows;
            for (int i = 0; i < sprites.Length; i++)
            {
                string asset = "Assets/Game/Hotel/Art/" + name + "-" + i.ToString("00") + ".asset";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(asset);
                bool storybookCutout = (name.StartsWith("Storybook") && name != "StorybookBackgrounds") || name.StartsWith("Mystery");
                if (sprites[i] != null && !storybookCutout) continue;
                int x0 = i % columns * cw, y0 = (rows - 1 - i / columns) * ch;
                int cellWidth=cw,cellHeight=ch;
                if (storybookCutout)
                {
                    // Find each row's actual transparent gutter; a sleeve can extend
                    // past the mathematical grid line without touching its neighbour.
                    int Edge(int column)
                    {
                        if (column == 0) return 0;
                        if (column == columns) return texture.width;
                        int ideal = column * cw, bestX = ideal, bestScore = int.MaxValue;
                        for (int x = ideal - cw / 4; x <= ideal + cw / 4; x++)
                        {
                            int count = 0;
                            for (int y = y0; y < y0 + ch; y++) if (pixels[y * texture.width + x].a > 64) count++;
                            int score = count * texture.width + Math.Abs(x - ideal);
                            if (score < bestScore) { bestScore = score; bestX = x; }
                        }
                        return bestX;
                    }
                    int col = i % columns; x0 = Edge(col); cellWidth = Edge(col + 1) - x0;
                }
                if(name=="CollectionEvidence")
                {
                    // The generated cutouts have uneven gutters. Slice along the transparent
                    // gap in each column instead of cutting through the tag or envelope.
                    float[] edges={0,.27f,.5f,.75f,1};int col=i%columns;
                    x0=Mathf.RoundToInt(edges[col]*texture.width);cellWidth=Mathf.RoundToInt(edges[col+1]*texture.width)-x0;
                    int split=ch,best=int.MaxValue;
                    for(int y=Mathf.RoundToInt(texture.height*.40f);y<Mathf.RoundToInt(texture.height*.60f);y++)
                    {
                        int opaque=0;for(int x=x0;x<x0+cellWidth;x++)if(pixels[y*texture.width+x].a>64)opaque++;
                        int score=opaque*texture.height+Mathf.Abs(y-ch);
                        if(score<best){best=score;split=y;}
                    }
                    y0=i<columns?split:0;cellHeight=i<columns?texture.height-split:split;
                }
                int minX = x0 + cellWidth, minY = y0 + cellHeight, maxX = x0, maxY = y0;
                for (int y = y0; y < y0 + cellHeight; y++) for (int x = x0; x < x0 + cellWidth; x++)
                    if (pixels[y * texture.width + x].a > 64) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                if (maxX <= minX) throw new InvalidOperationException("Empty atlas cell: " + asset);
                var sprite = Sprite.Create(texture, new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
                sprite.name = name + "-" + i.ToString("00");
                if (sprites[i] != null) { EditorUtility.CopySerialized(sprite, sprites[i]); EditorUtility.SetDirty(sprites[i]); UnityEngine.Object.DestroyImmediate(sprite); }
                else { AssetDatabase.CreateAsset(sprite, asset); sprites[i] = sprite; }
            }
            importer.isReadable = false; importer.SaveAndReimport();
            return sprites;
        }
        private static void Place(RectTransform rect, Transform parent, float x, float y, float width, float height)
        {
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); rect.localScale = Vector3.one;
        }
        private static void AuthorPuzzle(HotelInvestigationView ui)
        {
            var p = v.puzzleScreen.transform;
            var backdrop = Box(p, "Hotel atmosphere", 0, 0, 1600, 900, new Color(.36f, .42f, .45f));
            backdrop.sprite = v.locationImages[0].sprite; backdrop.transform.SetAsFirstSibling();
            var shade = Box(p, "Board contrast wash", 0, 0, 1600, 900, new Color(.025f, .07f, .09f, .76f)); shade.transform.SetSiblingIndex(1);
            Place((RectTransform)v.puzzleTitle.transform, p, 48, 103, 1020, 45); v.puzzleTitle.fontSize = 30;
            v.puzzlePurpose.gameObject.SetActive(false);
            v.movesText.transform.parent.gameObject.SetActive(false);
            var left = Box(p, "Objectives · icon and remaining count", 48, 185, 238, 610, Hex("20363D"), true).transform;
            Text(left, "Moves caption", "남은 조사", 24, 24, 190, 32, 20, Gold);
            Place((RectTransform)v.movesText.transform, left, 24, 59, 190, 100); v.movesText.fontSize = 76;
            Box(left, "Gold divider", 24, 176, 190, 2, Gold);
            Text(left, "Goals caption", "찾아낼 목표", 24, 198, 190, 35, 21, Gold);
            ui.goalIcons = new Image[3]; ui.goalAmounts = new Text[3]; ui.goalNames = new Text[3]; ui.goalChecks = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var item = Box(left, "Goal " + i, 16, 250 + i * 108, 206, 94, Hex("2E4850"), true).transform;
                ui.goalIcons[i] = Box(item, "Target picture", 10, 12, 70, 70, Color.white); ui.goalIcons[i].preserveAspect = true; ui.goalIcons[i].sprite = ui.pieces[i];
                ui.goalAmounts[i] = Text(item, "Remaining", "12", 96, 31, 99, 52, 36, Paper, FontStyle.Bold);
                ui.goalNames[i] = Text(item, "Target name", "산호", 97, 9, 99, 25, 17, Muted);
                ui.goalChecks[i] = Box(item, "Completed glow", 2, 2, 202, 90, new Color(.48f, .8f, .59f, .12f), true); ui.goalChecks[i].gameObject.SetActive(false);
            }
            var frame = Box(p, "Brass board rim", 316, 167, 724, 724, Hex("997D50"), true);
            Place((RectTransform)v.boardLayout.transform, p, 324, 175, 708, 708);
            frame.transform.SetSiblingIndex(v.boardLayout.transform.GetSiblingIndex());
            v.boardLayout.padding = new RectOffset(9, 9, 9, 9); v.boardLayout.spacing = new Vector2(5, 5);
            v.boardLayout.GetComponent<Image>().color = Hex("12262E");
            v.boardLayout.gameObject.AddComponent<RectMask2D>();
            foreach (var cell in v.cells)
            {
                cell.button.transition = Selectable.Transition.None;
                cell.blockRoot = Rect(cell.transform, "Moving block · obstacle stays in slot", 0, 0, 80, 80); Stretch(cell.blockRoot, 0);
                cell.customImage.transform.SetParent(cell.blockRoot, false); Stretch(cell.customImage.rectTransform, 1); cell.customImage.preserveAspect = true; cell.customImage.type = Image.Type.Simple;
                cell.symbol.transform.SetParent(cell.blockRoot, false); Stretch((RectTransform)cell.symbol.transform, 0);
                cell.blockRoot.SetSiblingIndex(0);
                cell.obstacleBorder.type = Image.Type.Simple; cell.obstacleBorder.preserveAspect = true; Stretch(cell.obstacleBorder.rectTransform, 0);
                cell.obstacleText.fontSize = 18; cell.obstacleText.alignment = TextAnchor.LowerRight; cell.obstacleText.color = Ink;
                cell.highlight = Box(cell.transform, "Group preview", 0, 0, 80, 80, new Color(1, .84f, .4f, .3f), true); Stretch(cell.highlight.rectTransform, 0); cell.highlight.enabled = false;
            }
            ui.effectsRoot = Rect(p, "Pooled special effects · no runtime creation", 324, 175, 708, 708);
            ui.effectsRoot.gameObject.AddComponent<RectMask2D>(); ui.effects = new Image[96];
            for (int i = 0; i < ui.effects.Length; i++)
            {
                var image = Box(ui.effectsRoot, "Effect " + i, 0, 0, 1, 1, Color.white, true);
                image.rectTransform.pivot = new Vector2(.5f, .5f); image.gameObject.SetActive(false); ui.effects[i] = image;
            }
            v.puzzleHelp.transform.parent.gameObject.SetActive(false);
            var context = Box(p, "Investigation beside the board", 1072, 185, 480, 550, Hex("20363D"), true).transform;
            Text(context, "Scene caption", "이번에 살펴보는 곳", 24, 20, 420, 30, 19, Gold);
            ui.puzzleScene = Box(context, "Investigation image", 18, 63, 444, 250, Color.white); ui.puzzleScene.sprite = ui.cabinet;
            ui.contextTitle = Text(context, "Investigation title", "열쇠 보관함", 24, 337, 252, 72, 28, Paper, FontStyle.Bold);
            ui.contextBody = Text(context, "Seoyun thought", "작은 흔적을 찾아보자.", 24, 418, 270, 112, 21, Muted);
            ui.puzzlePortrait = Box(context, "Seoyun reacts", 298, 323, 170, 213, Color.white); ui.puzzlePortrait.preserveAspect = true; ui.puzzlePortrait.sprite = ui.poses[0];
            Place((RectTransform)v.comboText.transform, p, 1088, 750, 448, 58); v.comboText.fontSize = 20;
            Place((RectTransform)v.puzzleHintButton.transform, p, 1072, 828, 146, 54);
            Place((RectTransform)v.retryButton.transform, p, 1230, 828, 146, 54);
            Place((RectTransform)v.puzzleBackButton.transform, p, 1388, 828, 164, 54);
            ui.helpButton = Button(p, "Rules on demand", "도움말", 1372, 105, 180, 48, Teal, 20);
            ui.helpPanel = Box(p, "Rules overlay · toggle with help or Escape", 1038, 167, 514, 622, Hex("EDE3CB"), true).gameObject;
            Text(ui.helpPanel.transform, "Help heading", "조사하는 방법", 30, 25, 450, 40, 28, Ink, FontStyle.Bold);
            Place((RectTransform)v.puzzleHelp.transform, ui.helpPanel.transform, 30, 88, 450, 490); v.puzzleHelp.fontSize = 24; v.puzzleHelp.color = Ink;
            ui.helpPanel.SetActive(false);
        }
        private static void AuthorInspection(HotelInvestigationView ui)
        {
            ui.inspectionPanel = Box(root, "12 Object inspection · first key investigation", 0, 0, 1600, 900, Ink).gameObject;
            var p = ui.inspectionPanel.transform;
            ui.inspectionImage = Box(p, "Key cabinet close-up", 0, 0, 1600, 900, Color.white); ui.inspectionImage.sprite = ui.cabinet;
            var title = Box(p, "Inspection plaque", 42, 32, 700, 88, new Color(.06f, .1f, .12f, .92f), true);
            ui.inspectionTitle = Text(title.transform, "Object title", "303호 · 비어 있는 열쇠 자리", 24, 22, 650, 50, 30, Paper, FontStyle.Bold);
            ui.discoveryCard = Box(p, "Found evidence", 1120, 220, 380, 408, new Color(.06f, .13f, .16f, .97f), true).gameObject;
            Text(ui.discoveryCard.transform, "Discovery label", "발견한 흔적", 25, 20, 330, 32, 19, Gold);
            ui.discoveryImage = Box(ui.discoveryCard.transform, "Thread illustration", 50, 67, 280, 250, Color.white); ui.discoveryImage.preserveAspect = true; ui.discoveryImage.sprite = ui.pieces[13];
            ui.discoveryTitle = Text(ui.discoveryCard.transform, "Evidence name", "흰 실", 25, 333, 330, 56, 25, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
            var footer = Box(p, "Observation and action", 36, 741, 1528, 130, new Color(.035f, .08f, .1f, .96f), true).transform;
            ui.inspectionBody = Text(footer, "Observed fact", "", 24, 25, 850, 89, 25, Paper);
            ui.inspectionContinue = Button(footer, "Continue investigation", "주변 조사하기", 932, 38, 360, 58, Hex("766247"), 24);
            ui.inspectionBack = Button(footer, "Return to room", "돌아가기", 1312, 38, 190, 58, Teal, 21);
            ui.inspectionPanel.SetActive(false);
            v.resultPanel.transform.SetAsLastSibling(); v.stagePresentation.curtain.transform.SetAsLastSibling();
        }
        private static void AuthorProps(HotelInvestigationView ui)
        {
            var hud = v.explorationHud.transform;
            ui.props = new[] { Button(hud, "Inspect reception bell", "벨 살펴보기", 1060, 304, 170, 46, Hex("374E51"), 18), Button(hud, "Inspect grandfather clock", "시계 살펴보기", 72, 283, 188, 46, Hex("374E51"), 18) };
            var note = Box(hud, "Short observation", 44, 651, 890, 85, new Color(.035f, .075f, .095f, .92f), true);
            ui.propCaption = Text(note.transform, "Object observation", "", 22, 21, 846, 53, 23, Paper);
            ui.investigatedMark = Box(hud, "Key cabinet investigated", 1100, 224, 242, 36, new Color(.035f, .075f, .095f, .9f), true).gameObject;
            Text(ui.investigatedMark.transform, "Completed observation", "✓ 보관함 조사 완료", 12, 5, 218, 28, 17, Gold);
            ui.investigatedMark.SetActive(false); note.gameObject.SetActive(false);
        }
    }
}
