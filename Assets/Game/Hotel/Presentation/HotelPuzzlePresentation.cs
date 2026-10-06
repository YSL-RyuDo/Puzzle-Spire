using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        private bool turnReshuffled;
        private readonly HashSet<BlockType> taughtSpecials = new HashSet<BlockType>();
        private HotelInvestigationView Presentation => view.investigation;
        private int CellIndex(GridPosition p) => (stage.height - 1 - p.Y) * stage.width + p.X;
        private HotelCellView Cell(GridPosition p) => view.cells[CellIndex(p)];
        private float CellPitch => view.boardLayout.cellSize.y + view.boardLayout.spacing.y;

        private Sprite ThemePiece(int index) => storage.Image(HotelThemeKeys.Pieces[index]) ?? Presentation.Piece(index);
        private Sprite ThemeBackground(string id)
        {
            int index = Array.FindIndex(storage.Pack.locations, l => l.id == id);
            if (index < 0) index = 0;
            var location = storage.Pack.locations[index];
            return storage.Image(location.image ?? "background." + location.id) ?? defaultBackgrounds[index];
        }
        private Sprite ThemeCabinet => storage.Image("inspection.cabinet") ?? Presentation.cabinet;
        private Sprite BlockSprite(Block block)
        {
            if (block.Type == BlockType.Normal)
                return storage.Image("block." + (int)block.Color) ?? defaultBlockSprites[(int)block.Color];
            int index = block.Type == BlockType.HorizontalRocket ? 5 : block.Type == BlockType.VerticalRocket ? 6 : block.Type == BlockType.Bomb ? 7 : 8;
            return ThemePiece(index);
        }
        private void DrawBlock(HotelCellView cell, Block block)
        {
            cell.customImage.gameObject.SetActive(block != null);
            cell.symbol.text = "";
            if (block == null) return;
            cell.customImage.sprite = BlockSprite(block);
            cell.customImage.color = Color.white;
            if (cell.customImage.sprite == null)
            {
                cell.customImage.color = blockColors[(int)block.Color];
                cell.symbol.text = new[] { "◆", "●", "▲", "■", "✦" }[(int)block.Color];
            }
        }
        private void DrawCell(int index)
        {
            var cell = view.cells[index];
            cell.ResetPose();
            var p = new GridPosition(index % stage.width, stage.height - 1 - index / stage.width);
            bool playable = puzzle.Board.IsPlayable(p);
            var block = playable ? puzzle.Board.GetBlock(p) : null;
            cell.face.color = playable ? new Color(.07f, .13f, .16f, .82f) : Color.clear;
            DrawBlock(cell, block);
            var box = playable ? puzzle.Board.GetObstacle(p, ObstacleLayer.Blocking) as IDamageableObstacle : null;
            var ice = playable ? puzzle.Board.GetObstacle(p, ObstacleLayer.Overlay) as IDamageableObstacle : null;
            var obstacle = box ?? ice;
            cell.obstacleBorder.gameObject.SetActive(obstacle != null);
            cell.obstacleText.text = "";
            if (obstacle != null)
            {
                var authored = stage.obstacles.FirstOrDefault(o => o.x == p.X && o.y == p.Y && o.type == (box != null ? "box" : "ice"));
                bool damaged = authored != null && obstacle.CurrentDurability < authored.durability;
                cell.obstacleBorder.sprite = ThemePiece(box != null ? (damaged ? 10 : 9) : (damaged ? 12 : 11));
                cell.obstacleBorder.color = Color.white;
                if (obstacle.CurrentDurability > 1) cell.obstacleText.text = obstacle.CurrentDurability.ToString();
            }
            cell.button.interactable = !busy && !puzzle.Cleared && !puzzle.Failed && block != null;
        }
        private void UpdatePuzzleHud()
        {
            view.movesText.text = puzzle.Session.RemainingMoves.ToString();
            view.movesText.color = puzzle.Session.RemainingMoves <= 5 ? new Color(1, .58f, .42f) : new Color(.96f, .9f, .76f);
            for (int i = 0; i < Presentation.goalIcons.Length; i++)
            {
                bool active = i < puzzle.Objectives.Objectives.Count;
                Presentation.goalIcons[i].transform.parent.gameObject.SetActive(active);
                if (!active) continue;
                var goal = puzzle.Objectives.Objectives[i];
                var color = goal as RemoveColorObjective;
                Presentation.goalIcons[i].sprite = color != null
                    ? storage.Image("block." + (int)color.TargetColor) ?? defaultBlockSprites[(int)color.TargetColor]
                    : ThemePiece(goal is RemoveBoxObjective ? 9 : 11);
                Presentation.goalNames[i].text = color != null ? ColorName(color.TargetColor) : goal is RemoveBoxObjective ? "상자" : "얼음";
                Presentation.goalAmounts[i].text = goal.IsCompleted ? "완료" : Math.Max(0, goal.TargetAmount - goal.CurrentAmount).ToString();
                Presentation.goalAmounts[i].fontSize = goal.IsCompleted ? 23 : 36;
                Presentation.goalChecks[i].gameObject.SetActive(goal.IsCompleted);
                Presentation.goalIcons[i].color = goal.IsCompleted ? new Color(.66f, .85f, .72f) : Color.white;
            }
            view.comboText.text = toast;
        }
        private void SetupPuzzlePresentation()
        {
            Presentation.ClearEffects(); taughtSpecials.Clear();
            Presentation.puzzleScene.sprite = !practice && activeNode?.inspection == "cabinet" ? ThemeCabinet : ThemeBackground(activeNode?.location ?? "lobby");
            Presentation.puzzlePortrait.sprite = Portrait("you", "curious");
            Presentation.contextTitle.text = practice ? "자유롭게 연습하기" : activeNode?.title ?? stage.title;
            Presentation.contextBody.text = !practice && activeNode?.inspection == "cabinet" ? "비어 있는 303호 자리.\n주변을 정리하면 작은 흔적이 보일지도." : "목표를 채우고\n다음 흔적을 확인해 보자.";
            view.puzzleHelp.text = "같은 색 2개 이상을 클릭\n\n5개  →  로켓\n7개  →  폭탄\n9개  →  색 제거\n\n상자는 옆 블록을 제거해요.\n얼음은 같은 칸을 제거해요.\n특수 블록은 하나만 눌러도 발동해요.";
            toast = stage.boxes > 0 ? "상자 옆 묶음을 눌러 보세요" : stage.ice > 0 ? "얼음 속 블록을 정리해 보세요" : "같은 색 두 개 이상을 눌러 보세요";
        }
        private void HoverCell(int index, bool entered)
        {
            if (busy || screen != "puzzle" || view.resultPanel.activeSelf || Presentation.helpPanel.activeSelf) return;
            ClearHighlights();
            if (!entered) { view.comboText.text = toast; return; }
            var p = new GridPosition(index % stage.width, stage.height - 1 - index / stage.width);
            if (!puzzle.Board.IsPlayable(p)) return;
            var block = puzzle.Board.GetBlock(p);
            if (block == null) return;
            if (block.Type != BlockType.Normal)
            {
                Cell(p).highlight.enabled = true;
                view.comboText.text = SpecialLabel(block.Type) + " · 클릭해서 발동";
                return;
            }
            var group = new ConnectedBlockFinder().Find(puzzle.Board, p);
            if (group.Count < 2) return;
            foreach (var at in group) Cell(at).highlight.enabled = true;
            view.comboText.text = group.Count >= 9 ? "색 제거 블록을 만들 수 있어요" : group.Count >= 7 ? "폭탄을 만들 수 있어요" : group.Count >= 5 ? "로켓을 만들 수 있어요" : group.Count + "개 함께 정리";
        }
        private void ClearHighlights() { foreach (var cell in view.cells) cell.highlight.enabled = false; }
        private static string SpecialLabel(BlockType type) => type == BlockType.Bomb ? "폭탄" : type == BlockType.ColorBomb ? "색 제거" : type == BlockType.HorizontalRocket ? "가로 로켓" : "세로 로켓";

        private IEnumerator TweenPhase(float duration, Action<float> update)
        {
            if (smoke) { update(1); yield break; }
            for (float t = 0; t < duration; t += HotelPresentationClock.Delta)
            { update(Mathf.Clamp01(t / duration)); yield return null; }
            update(1);
        }
        private Vector2 BoardPoint(GridPosition p)
        {
            var rect = (RectTransform)Cell(p).transform;
            return Presentation.effectsRoot.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
        }
        private Image Effect(int index, Vector2 center, Vector2 size, Color color, float angle = 0, bool disk = false)
        {
            if (index >= Presentation.effects.Length) return null;
            var image = Presentation.effects[index]; image.gameObject.SetActive(true);
            image.sprite = disk ? Presentation.effectDisk : null; image.type = Image.Type.Simple;
            image.rectTransform.anchoredPosition = center; image.rectTransform.sizeDelta = size;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            image.color = color; return image;
        }
        private void DrawSpecialEffects(PuzzleTurnResult result, float t)
        {
            Presentation.ClearEffects(); int index = 0;
            foreach (var activation in result.ActivatedSpecialBlocks)
            {
                var center = BoardPoint(activation.Position);
                float alpha = Mathf.Sin(t * Mathf.PI) * .65f;
                if (activation.Type == BlockType.HorizontalRocket || activation.Type == BlockType.VerticalRocket)
                {
                    bool horizontal = activation.Type == BlockType.HorizontalRocket;
                    var first = BoardPoint(new GridPosition(horizontal ? 0 : activation.Position.X, horizontal ? activation.Position.Y : 0));
                    var last = BoardPoint(new GridPosition(horizontal ? stage.width - 1 : activation.Position.X, horizontal ? activation.Position.Y : stage.height - 1));
                    var middle = (first + last) * .5f;
                    var size = horizontal ? new Vector2((Vector2.Distance(first, last) + CellPitch) * Mathf.Min(1, t * 2), 12) : new Vector2(12, (Vector2.Distance(first, last) + CellPitch) * Mathf.Min(1, t * 2));
                    Effect(index++, middle, size, new Color(1, .83f, .42f, alpha));
                }
                else if (activation.Type == BlockType.Bomb)
                    Effect(index++, center, Vector2.one * CellPitch * Mathf.Lerp(.3f, 3.4f, t), new Color(1, .67f, .25f, alpha * .65f), 0, true);
                else
                    foreach (var at in result.RemovedPositions)
                    {
                        Vector2 end = Vector2.Lerp(center, BoardPoint(at), Mathf.Min(1, t * 2));
                        Vector2 direction = end - center;
                        Effect(index++, (center + end) / 2, new Vector2(direction.magnitude, 4), new Color(.64f, .87f, 1, alpha), Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    }
            }
        }
        private IEnumerator PresentTurn(PuzzleTurnResult result)
        {
            ClearHighlights();
            foreach (var cell in view.cells) cell.button.interactable = false;
            view.movesText.text = puzzle.Session.RemainingMoves.ToString();
            if (result.MergedSpecialBlocks.Count > 0)
                yield return TweenPhase(.14f, t =>
                {
                    foreach (var merge in result.MergedSpecialBlocks)
                        Cell(merge.SourcePosition).blockRoot.anchoredPosition = (BoardPoint(merge.TargetPosition) - BoardPoint(merge.SourcePosition)) * t;
                });
            foreach (var change in result.TransformedSpecialBlocks)
                DrawBlock(Cell(change.Position), new Block(change.Color, change.TransformedType));
            if (result.TransformedSpecialBlocks.Count > 0)
                yield return TweenPhase(.16f, t =>
                {
                    foreach (var change in result.TransformedSpecialBlocks)
                        Cell(change.Position).blockRoot.localScale = Vector3.one * (1 + .18f * Mathf.Sin(t * Mathf.PI));
                });
            if (result.ActivatedSpecialBlocks.Count > 0)
            {
                toast = string.Join(" · ", result.ActivatedSpecialBlocks.Select(a => SpecialLabel(a.Type)).Distinct());
                view.comboText.text = toast;
                yield return TweenPhase(.3f, t => DrawSpecialEffects(result, t));
                Presentation.ClearEffects();
            }
            yield return TweenPhase(turnAnimationSeconds, t =>
            {
                DrawRemovalFragments(result, t);
                foreach (var at in result.RemovedPositions)
                    Cell(at).blockRoot.localScale = Vector3.one * Mathf.Lerp(1.08f, 0, t * t);
                foreach (var change in result.ObstacleChanges)
                {
                    var rect = Cell(change.Position).obstacleBorder.rectTransform;
                    rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 6) * 7 * (1 - t));
                    rect.localScale = Vector3.one * (change.WasDestroyed ? 1 - t : 1 + .08f * Mathf.Sin(t * Mathf.PI));
                }
            });
            Presentation.ClearEffects();
            foreach (var at in result.RemovedPositions) Cell(at).customImage.gameObject.SetActive(false);
            if (result.CreatedSpecialBlock != null)
            {
                var created = result.CreatedSpecialBlock; var cell = Cell(created.Position);
                DrawBlock(cell, created.Block);
                yield return TweenPhase(.16f, t => cell.blockRoot.localScale = Vector3.one * Mathf.Lerp(.2f, 1, t));
                if (taughtSpecials.Add(created.Type)) toast = SpecialLabel(created.Type) + " 생성! 하나만 눌러도 발동해요";
            }
            // Render final identities at their destination, then offset only their block children.
            // Obstacles and click targets stay at fixed grid positions throughout the fall.
            RenderBoard(false);
            var starts = new Dictionary<int, Vector2>();
            if (!turnReshuffled)
            {
                foreach (var move in result.GravityMoves)
                    starts[CellIndex(move.To)] = new Vector2((move.From.X - move.To.X) * CellPitch, (move.From.Y - move.To.Y) * CellPitch);
                foreach (var spawn in result.SpawnedBlocks)
                {
                    int top = spawn.Position.Y;
                    while (top + 1 < stage.height && puzzle.Board.IsPlayable(new GridPosition(spawn.Position.X, top + 1)) && puzzle.Board.GetObstacle(new GridPosition(spawn.Position.X, top + 1), ObstacleLayer.Blocking) == null) top++;
                    starts[CellIndex(spawn.Position)] = Vector2.up * CellPitch * (top - spawn.Position.Y + 1);
                }
            }
            else
            {
                toast = "움직일 곳이 없어 다시 정리했어요 · 횟수 추가 소모 없음";
                foreach (var cell in view.cells.Where(c => c.gameObject.activeSelf)) cell.blockRoot.localScale = Vector3.zero;
            }
            yield return TweenPhase(.29f, t =>
            {
                float eased = t < .85f ? Mathf.Pow(t / .85f, 2) : 1 + Mathf.Sin((t - .85f) / .15f * Mathf.PI) * .025f;
                foreach (var pair in starts) view.cells[pair.Key].blockRoot.anchoredPosition = pair.Value * (1 - eased);
                if (turnReshuffled) foreach (var cell in view.cells) cell.blockRoot.localScale = Vector3.one * Mathf.Min(1, t * 1.3f);
            });
            foreach (var cell in view.cells) cell.ResetPose();
            UpdatePuzzleHud();
            if (puzzle.Cleared)
            {
                Presentation.contextTitle.text = "흔적을 확인할 시간";
                Presentation.contextBody.text = "주변이 정리됐어요.\n조사하던 곳을 다시 살펴봐요.";
                Presentation.puzzlePortrait.sprite = Portrait("you", "found");
                yield return TweenPhase(.3f, t =>
                {
                    foreach (var icon in Presentation.goalIcons) icon.rectTransform.localScale = Vector3.one * (1 + .12f * Mathf.Sin(t * Mathf.PI));
                });
            }
        }
        private void DrawRemovalFragments(PuzzleTurnResult result, float t)
        {
            Presentation.ClearEffects(); int index = 0;
            foreach (var removed in result.RemovedBlocks)
            {
                Color color = Color.Lerp(blockColors[(int)removed.Color], Color.white, .35f); color.a = 1 - t;
                var center = BoardPoint(removed.Position);
                for (int j = 0; j < 2; j++)
                {
                    float angle = (removed.Position.X * 53 + removed.Position.Y * 97 + j * 163) * Mathf.Deg2Rad;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Effect(index++, center + direction * CellPitch * .46f * t + Vector2.down * 22 * t * t,
                        new Vector2(7, 11) * (1 - .6f * t), color, angle * Mathf.Rad2Deg + t * 90);
                }
            }
            foreach (var change in result.ObstacleChanges)
            {
                Color color = change.Type == CellObstacleType.Box ? new Color(.83f, .59f, .3f, 1 - t) : new Color(.7f, .92f, 1, 1 - t);
                for (int j = 0; j < 3; j++)
                {
                    float angle = j * 2.1f;
                    var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * CellPitch * .55f * t;
                    Effect(index++, BoardPoint(change.Position) + offset, Vector2.one * 10 * (1 - .65f * t), color, j * 60 + t * 100);
                }
            }
        }
    }
}
