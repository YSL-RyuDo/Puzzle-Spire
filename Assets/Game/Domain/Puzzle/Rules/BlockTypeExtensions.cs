namespace PuzzleSpire.Domain.Puzzle
{
    public static class BlockTypeExtensions
    {
        public static bool IsSpecial(this BlockType type)
        {
            return type != BlockType.Normal;
        }

        public static bool IsRocket(this BlockType type)
        {
            return type == BlockType.HorizontalRocket
                || type == BlockType.VerticalRocket;
        }
    }
}
