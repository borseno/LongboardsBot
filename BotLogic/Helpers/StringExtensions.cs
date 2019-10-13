namespace LongBoardsBot.Helpers
{
    public static class StringExtensions
    {
        public static string Remove(this string value, string substring)
            => value.Remove(value.IndexOf(substring), substring.Length);
    }
}