using System;
using System.Threading.Tasks;
using static LongBoardsBot.Helpers.FileExtensions;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Models.Entities
{
    static class Texts
    {
        public static Task<string> GetFinalTextToUserAsync() => ReadAllTextAsync(FinalMessageToUserPath);

        public static Task<string> GetFinalTextToAdminsAsync() => ReadAllTextAsync(FinalMessageToAdminsPath);

        public static Task<string> GetLongBoardInfoText() => ReadAllTextAsync(LongboardInfoPatternPath);

        public static Task<string> GetGreetingTextAsync() => ReadAllTextAsync(GreetingTextPath);
    }
}
