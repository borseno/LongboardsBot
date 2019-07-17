using LongBoardsBot.Helpers;
using System.IO;
using Telegram.Bot.Types.ReplyMarkups;

namespace LongBoardsBot.Models
{
    static class Constants
    {
        public const string Url = "https://longboardistbot.azurewebsites.net:443/";
        public const string NickName = "@LongBoard_Dev_Bot"; // nickname (the one that starts with @)
        public const string ApiKey = @"821836757:AAHFbFgSrbrvpGVpzCYWZAwG2Jzo7Cbl1m8";

        public const string LBDirectory = @"Models\LongBoardsPhotos";
        public const long AdminGroupChatId = -1001181244049;
        public const long BugReportChatId = 777785046;
        public const string CancelText = "Отменить";
        public const string AddText = "Добавить";
        public const string YesText = "Да";
        public const string NoText = "Нет";
        public const string FinishText = "Закончить";
        public const string TextFileName = "Text.txt";
        public const string ImageExtension = ".jpg";
        public const string FinalMessagePath = @"Models\Texts\FinalMessage.txt";
        public const string RestartText = "Начать покупки заново";

        public static readonly ReplyKeyboardMarkup AllLBkboard; // keyboard for all longboards
        public static readonly FileInfo[] AllLBImages;
        public static readonly DirectoryInfo BoardsDirectory;
        public static readonly ReplyKeyboardMarkup RestartKBoard;

        static Constants()
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles();
            var buttons = new KeyboardButton[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                buttons[i] = new KeyboardButton(files[i].NameWithoutExt());
            }

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true, true);

            AllLBkboard = myReplyMarkup;
            AllLBImages = files;
            BoardsDirectory = directory;
            RestartKBoard = new ReplyKeyboardMarkup(new[] { new KeyboardButton(RestartText) }, oneTimeKeyboard: true);
        }
    }
}
