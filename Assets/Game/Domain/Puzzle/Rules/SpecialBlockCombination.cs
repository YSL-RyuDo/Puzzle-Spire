using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class SpecialBlockCombination
    {
        private readonly SpecialBlockMember[] _members;

        public SpecialBlockCombinationType Type { get; }

        public GridPosition PrimaryPosition { get; }

        public GridPosition SecondaryPosition =>
            _members[1].Position;

        public BlockType PrimaryBlockType =>
            _members[0].Type;

        public BlockType SecondaryBlockType =>
            _members[1].Type;

        public BlockColor PrimaryColor =>
            _members[0].Color;

        public BlockColor SecondaryColor =>
            _members[1].Color;

        public IReadOnlyList<SpecialBlockMember> Members =>
            _members;

        public int MemberCount =>
            _members.Length;

        public BlockColor TargetColor
        {
            get
            {
                for (int i = 0; i < _members.Length; i++)
                {
                    if (_members[i].Type == BlockType.ColorBomb)
                    {
                        return _members[i].Color;
                    }
                }

                return PrimaryColor;
            }
        }

        public BlockType RocketType
        {
            get
            {
                for (int i = 0; i < _members.Length; i++)
                {
                    if (_members[i].Type.IsRocket())
                    {
                        return _members[i].Type;
                    }
                }

                return BlockType.Normal;
            }
        }

        public SpecialBlockCombination(
            SpecialBlockCombinationType type,
            GridPosition primaryPosition,
            Block primaryBlock,
            GridPosition secondaryPosition,
            Block secondaryBlock)
            : this(
                type,
                primaryPosition,
                new[]
                {
                    new SpecialBlockMember(
                        primaryPosition,
                        primaryBlock),
                    new SpecialBlockMember(
                        secondaryPosition,
                        secondaryBlock)
                })
        {
            int distance =
                Math.Abs(primaryPosition.X - secondaryPosition.X)
                + Math.Abs(primaryPosition.Y - secondaryPosition.Y);

            if (distance != 1)
            {
                throw new ArgumentException(
                    "Special blocks must be orthogonally adjacent.");
            }
        }

        public SpecialBlockCombination(
            SpecialBlockCombinationType type,
            GridPosition primaryPosition,
            IReadOnlyList<SpecialBlockMember> members)
        {
            if (!Enum.IsDefined(
                    typeof(SpecialBlockCombinationType),
                    type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown combination type.");
            }

            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            if (members.Count < 2)
            {
                throw new ArgumentException(
                    "A combination requires at least two special blocks.",
                    nameof(members));
            }

            _members =
                CopyMembersWithPrimaryFirst(
                    primaryPosition,
                    members);

            Type = type;
            PrimaryPosition = primaryPosition;
        }

        public bool ContainsPosition(
            GridPosition position)
        {
            for (int i = 0; i < _members.Length; i++)
            {
                if (_members[i].Position == position)
                {
                    return true;
                }
            }

            return false;
        }

        private static SpecialBlockMember[]
            CopyMembersWithPrimaryFirst(
                GridPosition primaryPosition,
                IReadOnlyList<SpecialBlockMember> source)
        {
            SpecialBlockMember[] result =
                new SpecialBlockMember[source.Count];

            HashSet<GridPosition> positions =
                new HashSet<GridPosition>();

            int primaryIndex = -1;

            for (int i = 0; i < source.Count; i++)
            {
                SpecialBlockMember member = source[i];

                if (!positions.Add(member.Position))
                {
                    throw new ArgumentException(
                        "Combination members contain duplicate positions.",
                        nameof(source));
                }

                if (member.Position == primaryPosition)
                {
                    primaryIndex = i;
                }
            }

            if (primaryIndex < 0)
            {
                throw new ArgumentException(
                    "The primary position is not part of the combination.",
                    nameof(source));
            }

            result[0] = source[primaryIndex];

            int destinationIndex = 1;

            for (int i = 0; i < source.Count; i++)
            {
                if (i == primaryIndex)
                {
                    continue;
                }

                result[destinationIndex] = source[i];
                destinationIndex++;
            }

            return result;
        }
    }
}
