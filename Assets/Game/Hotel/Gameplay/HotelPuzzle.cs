using System;
using System.Collections.Generic;
using PuzzleSpire.Domain.Puzzle;

namespace PuzzleSpire.Hotel
{
    // The shipping UI and the existing PuzzleTest scene share the same domain rules.
    public sealed class HotelPuzzle
    {
        public Board Board { get; }
        public PuzzleSession Session { get; }
        public ObjectiveSystem Objectives { get; }
        public bool Cleared => Objectives.AllCompleted;
        public bool Failed => !Cleared && !Session.HasMovesRemaining;
        public int Reshuffles { get; private set; }
        private readonly PuzzleTurnResolver resolver;
        private readonly Random random;
        private readonly int colors;
        private readonly ConnectedBlockFinder finder = new ConnectedBlockFinder();

        public HotelPuzzle(HotelStage stage, int attempt = 0)
        {
            ContentValidation.ValidateStage(stage);
            var holes = new List<GridPosition>();
            for (int row = 0; row < stage.height; row++) for (int x = 0; x < stage.width; x++)
                if (stage.layout[row][x] == 'X') holes.Add(new GridPosition(x, stage.height - 1 - row));
            Board = new Board(stage.width, stage.height, holes);
            foreach (var o in stage.obstacles)
                Board.PlaceObstacle(new GridPosition(o.x, o.y), o.type == "box" ? (ICellObstacle)new BoxObstacle("box", o.durability) : new IceObstacle("ice", o.durability));
            colors = stage.colors;
            int seed = unchecked(stage.seed + attempt * 7919);
            random = new Random(seed);
            var palette = new List<BlockColor>();
            for (int i = 0; i < colors; i++) palette.Add((BlockColor)i);
            var spawn = new BlockSpawnSystem(new RandomBlockFactory(palette, seed));
            var state = new PuzzleStateMachine(PuzzleState.WaitingForInput);
            var effects = new SpecialBlockEffectResolver();
            resolver = new PuzzleTurnResolver(finder, new BlockRemovalSystem(finder, 2), new GravitySystem(), spawn, state,
                new SpecialBlockCreationRule(seed), effects, new SpecialBlockCombinationResolver(),
                new SpecialBlockCombinationEffectResolver(effects), new ColorBombSpecialGroupEffectResolver(effects, seed), new ObstacleImpactSystem());
            Session = new PuzzleSession(stage.moves);
            var goals = new List<IStageObjective>();
            if (stage.targetAmount > 0) goals.Add(new RemoveColorObjective("color", (BlockColor)stage.targetColor, stage.targetAmount));
            if (stage.boxes > 0) goals.Add(new RemoveBoxObjective("box", stage.boxes));
            if (stage.ice > 0) goals.Add(new RemoveIceObjective("ice", stage.ice));
            Objectives = new ObjectiveSystem(goals);
            spawn.FillEmptyCells(Board);
            EnsurePlayable();
        }
        public PuzzleTurnResult Play(GridPosition position)
        {
            if (Cleared || Failed) return PuzzleTurnResult.CreateNoChange();
            var result = resolver.Resolve(Board, position);
            Objectives.Apply(result);
            Session.TryConsumeMove(result);
            if (!Cleared && !Failed) EnsurePlayable();
            return result;
        }
        public GridPosition? FindMove()
        {
            for (int y = 0; y < Board.Height; y++) for (int x = 0; x < Board.Width; x++)
            {
                var p = new GridPosition(x, y);
                if (!Board.IsPlayable(p)) continue;
                var b = Board.GetBlock(p);
                if (b != null && (b.Type != BlockType.Normal || finder.Find(Board, p).Count >= 2)) return p;
            }
            return null;
        }
        public bool EnsurePlayable()
        {
            if (FindMove().HasValue) return false;
            var cells = new List<GridPosition>();
            for (int y = 0; y < Board.Height; y++) for (int x = 0; x < Board.Width; x++)
            {
                var p = new GridPosition(x, y);
                if (Board.IsPlayable(p) && Board.GetBlock(p) != null) cells.Add(p);
            }
            for (int pass = 0; pass < 20; pass++)
            {
                foreach (var p in cells)
                {
                    Board.RemoveBlock(p);
                    Board.PlaceBlock(p, new Block((BlockColor)random.Next(colors)));
                }
                if (FindMove().HasValue) { Reshuffles++; return true; }
            }
            // A special block is always actionable, even in disconnected one-cell pockets.
            if (cells.Count > 0)
            {
                var p = cells[0]; var color = Board.GetBlock(p).Color;
                Board.RemoveBlock(p); Board.PlaceBlock(p, new Block(color, BlockType.ColorBomb));
                Reshuffles++; return true;
            }
            throw new InvalidOperationException("플레이할 블록이 없는 보드입니다.");
        }
    }
}
