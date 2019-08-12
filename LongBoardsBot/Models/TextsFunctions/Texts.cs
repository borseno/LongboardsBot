using System;
using System.Threading.Tasks;
using static LongBoardsBot.Models.Constants;
using static System.IO.File;

namespace LongBoardsBot.Models.TextsFunctions
{
    static class Texts
    {
        public static Task<string> GetFinalTextToUserAsync() => ReadAllTextAsync(FinalMessageToUserPath);

        public static Task<string> GetFinalTextToAdminsAsync() => ReadAllTextAsync(FinalMessageToAdminsPath);

        public static Task<string> GetLongBoardInfoText() => ReadAllTextAsync(LongboardInfoPatternPath);

        public static Task<string> GetGreetingTextAsync() => ReadAllTextAsync(GreetingTextPath);

        public static Task<string> GetDeliveryNotification() => ReadAllTextAsync(DeliveryNotificationPath);

        public static Task<string> GetCancelledOrderingNotificationText() => ReadAllTextAsync(CancelledOrderingNotificationPath);

        public static Task<string> GetFinalTestingTextToAdminsAsync() => ReadAllTextAsync(FinalTestingTextToAdminsPath);

        public static Task<string> GetFinalTestingTextToUserAsync() => ReadAllTextAsync(FinalTestingTextToUserPath);
    }
}
