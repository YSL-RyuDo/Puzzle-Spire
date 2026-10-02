using System;
using System.Collections;
using System.IO;
using System.Linq;
using PuzzleSpire.Domain.Puzzle;
using UnityEngine;

namespace PuzzleSpire.Hotel
{
    public sealed partial class HotelGame
    {
        // Explicit QA fixtures, separate from campaign play and real saves.
        private IEnumerator ReviewPresentation()
        {
            string output = Path.Combine(Application.dataPath, "..", "PresentationReview");
            Directory.CreateDirectory(output);
            Application.logMessageReceived += ReviewError;
            HotelCapture.PrepareHiddenPlayer(view);
            HotelPresentationClock.Recording = true; Time.captureFramerate = 24;
            string[] names = { "horizontal", "vertical", "bomb", "color", "color-transform", "special-merge", "creation", "void-board" };
            for (int scenario = 0; scenario < names.Length; scenario++)
            {
                var fixture = new HotelStage
                {
                    id = "presentation_fixture", title = "연출 검증 · " + names[scenario], width = 7, height = 7, moves = 20, colors = 4,
                    targetColor = 0, targetAmount = 90, boxes = 2, ice = 1,
                    layout = Enumerable.Repeat("OOOOOOO", 7).ToArray(),
                    obstacles = new[] { new ObstacleData { x = 2, y = 3, type = "box", durability = 2 }, new ObstacleData { x = 4, y = 3, type = "box", durability = 1 }, new ObstacleData { x = 3, y = 2, type = "ice", durability = 2 } }
                };
                if (scenario == 7) fixture.layout = new[] { "XOOOOOX", "OOOOOOO", "OOXOXOO", "OOOOOOO", "OOOOOOO", "XOOOOOX", "XXOOOXX" };
                StartPuzzle(fixture, true, false);
                for (int y = 0; y < 7; y++) for (int x = 0; x < 7; x++)
                {
                    var at = new GridPosition(x, y);
                    if (!puzzle.Board.IsPlayable(at) || puzzle.Board.GetObstacle(at, ObstacleLayer.Blocking) != null) continue;
                    puzzle.Board.RemoveBlock(at); puzzle.Board.PlaceBlock(at, new Block((BlockColor)((x + y) % 4)));
                }
                var origin = new GridPosition(3, 3);
                var types = new[] { BlockType.HorizontalRocket, BlockType.VerticalRocket, BlockType.Bomb, BlockType.ColorBomb, BlockType.ColorBomb, BlockType.HorizontalRocket, BlockType.Normal, BlockType.Bomb };
                SetFixtureBlock(origin, types[scenario]);
                if (scenario == 4 || scenario == 5) SetFixtureBlock(new GridPosition(3, 4), scenario == 4 ? BlockType.HorizontalRocket : BlockType.Bomb);
                if (scenario == 6)
                {
                    for (int x = 0; x < 7; x++) SetFixtureBlock(new GridPosition(x, 0), BlockType.Normal);
                    origin = new GridPosition(3, 0);
                }
                RenderBoard(); yield return null;
                PressForSmoke(Presentation.helpButton);
                ContentValidation.Require(Presentation.helpPanel.activeSelf, "Help failed to open");
                PressForSmoke(Presentation.helpButton);
                PressForSmoke(view.puzzleHintButton);
                ContentValidation.Require(view.cells.Any(c => c.highlight.enabled), "Hint did not highlight a group");
                HotelCapture.Save(view, Path.Combine(output, names[scenario] + "-before.png"));
                int moves = puzzle.Session.RemainingMoves;
                PressForSmoke(Cell(origin).button);
                ContentValidation.Require(busy && puzzle.Session.RemainingMoves == moves - 1, "Fixture click did not start one turn");
                Cell(origin).button.onClick.Invoke(); view.retryButton.onClick.Invoke();
                ContentValidation.Require(puzzle.Session.RemainingMoves == moves - 1 && !view.resultPanel.activeSelf, "Busy input was not blocked");
                int frame = 0;
                while (busy && frame < 120)
                {
                    yield return null; frame++;
                    if (frame == 3 || frame == 6 || frame == 10 || frame == 15 || frame == 21)
                        HotelCapture.Save(view, Path.Combine(output, names[scenario] + "-frame-" + frame.ToString("00") + ".png"));
                }
                ContentValidation.Require(!busy, "Animation did not finish: " + names[scenario]);
                for (int y = 0; y < 7; y++) for (int x = 0; x < 7; x++)
                {
                    var at = new GridPosition(x, y); var cell = Cell(at);
                    ContentValidation.Require(cell.blockRoot.anchoredPosition.sqrMagnitude < .01f && cell.blockRoot.localScale == Vector3.one, "Unsettled block pose");
                    if (!puzzle.Board.IsPlayable(at)) continue;
                    var block = puzzle.Board.GetBlock(at);
                    ContentValidation.Require(cell.customImage.gameObject.activeSelf == (block != null), "Visual presence differs from domain");
                    if (block != null) ContentValidation.Require(cell.customImage.sprite == BlockSprite(block), "Visual block differs from domain");
                }
                HotelCapture.Save(view, Path.Combine(output, names[scenario] + "-after.png"));
                File.AppendAllText(Path.Combine(output, "verification.txt"), names[scenario] + ": PASS; legal UI click, busy guard, settled sprites match domain\n");
            }
            HotelPresentationClock.Recording = false; Time.captureFramerate = 0;
            Application.logMessageReceived -= ReviewError;
            Debug.Log("HOTEL_PRESENTATION_REVIEW_PASSED"); Application.Quit(0);
        }
        private void SetFixtureBlock(GridPosition at, BlockType type)
        {
            puzzle.Board.RemoveBlock(at); puzzle.Board.PlaceBlock(at, new Block(BlockColor.Blue, type));
        }
        private void ReviewError(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            File.AppendAllText(Path.Combine(Application.dataPath, "..", "PresentationReview", "verification.txt"), "FAIL: " + message + "\n" + stack);
            Application.Quit(2);
        }
    }
}
