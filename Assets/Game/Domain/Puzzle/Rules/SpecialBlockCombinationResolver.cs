using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombinationResolver
    {
        private static readonly GridPosition[] Directions =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1)
        };

        public bool TryResolve(
            Board board,
            GridPosition selectedPosition,
            out SpecialBlockCombination combination)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            combination = null;

            if (!board.IsPlayable(selectedPosition))
            {
                return false;
            }

            Block selectedBlock =
                board.GetBlock(selectedPosition);

            if (selectedBlock == null
                || !selectedBlock.Type.IsSpecial())
            {
                return false;
            }

            List<SpecialBlockMember> members =
                FindConnectedSpecialMembers(
                    board,
                    selectedPosition);

            if (members.Count < 2)
            {
                return false;
            }

            SpecialBlockCombinationType combinationType =
                GetCombinationType(members);

            combination =
                new SpecialBlockCombination(
                    combinationType,
                    selectedPosition,
                    members);

            return true;
        }

        private static List<SpecialBlockMember>
            FindConnectedSpecialMembers(
                Board board,
                GridPosition startPosition)
        {
            Queue<GridPosition> queue =
                new Queue<GridPosition>();

            HashSet<GridPosition> visited =
                new HashSet<GridPosition>();

            List<SpecialBlockMember> members =
                new List<SpecialBlockMember>();

            queue.Enqueue(startPosition);
            visited.Add(startPosition);

            while (queue.Count > 0)
            {
                GridPosition currentPosition =
                    queue.Dequeue();

                Block currentBlock =
                    board.GetBlock(currentPosition);

                if (currentBlock == null
                    || !currentBlock.Type.IsSpecial())
                {
                    continue;
                }

                members.Add(
                    new SpecialBlockMember(
                        currentPosition,
                        currentBlock));

                for (int i = 0; i < Directions.Length; i++)
                {
                    GridPosition direction = Directions[i];

                    GridPosition nextPosition =
                        new GridPosition(
                            currentPosition.X + direction.X,
                            currentPosition.Y + direction.Y);

                    if (!board.IsPlayable(nextPosition)
                        || !visited.Add(nextPosition))
                    {
                        continue;
                    }

                    Block nextBlock =
                        board.GetBlock(nextPosition);

                    if (nextBlock == null
                        || !nextBlock.Type.IsSpecial())
                    {
                        continue;
                    }

                    queue.Enqueue(nextPosition);
                }
            }

            members.Sort(
                (left, right) =>
                {
                    if (left.Position == startPosition)
                    {
                        return -1;
                    }

                    if (right.Position == startPosition)
                    {
                        return 1;
                    }

                    int leftDistance =
                        GetManhattanDistance(
                            left.Position,
                            startPosition);

                    int rightDistance =
                        GetManhattanDistance(
                            right.Position,
                            startPosition);

                    int distanceComparison =
                        leftDistance.CompareTo(rightDistance);

                    if (distanceComparison != 0)
                    {
                        return distanceComparison;
                    }

                    int yComparison =
                        left.Position.Y.CompareTo(
                            right.Position.Y);

                    if (yComparison != 0)
                    {
                        return yComparison;
                    }

                    return left.Position.X.CompareTo(
                        right.Position.X);
                });

            return members;
        }

        private static SpecialBlockCombinationType
            GetCombinationType(
                IReadOnlyList<SpecialBlockMember> members)
        {
            int colorBombCount = 0;
            bool hasBomb = false;
            bool hasRocket = false;

            for (int i = 0; i < members.Count; i++)
            {
                BlockType type = members[i].Type;

                if (type == BlockType.ColorBomb)
                {
                    colorBombCount++;
                }
                else if (type == BlockType.Bomb)
                {
                    hasBomb = true;
                }
                else if (type.IsRocket())
                {
                    hasRocket = true;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown special block type.");
                }
            }

            if (colorBombCount >= 2)
            {
                return SpecialBlockCombinationType
                    .ColorBombColorBomb;
            }

            if (colorBombCount == 1)
            {
                return hasBomb
                    ? SpecialBlockCombinationType.ColorBombBomb
                    : SpecialBlockCombinationType.ColorBombRocket;
            }

            if (hasBomb && hasRocket)
            {
                return SpecialBlockCombinationType.RocketBomb;
            }

            if (hasBomb)
            {
                return SpecialBlockCombinationType.BombBomb;
            }

            return SpecialBlockCombinationType.RocketRocket;
        }

        private static int GetManhattanDistance(
            GridPosition first,
            GridPosition second)
        {
            return Math.Abs(first.X - second.X)
                + Math.Abs(first.Y - second.Y);
        }
    }
}
