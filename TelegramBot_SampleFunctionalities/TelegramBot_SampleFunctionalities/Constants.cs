using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_SampleFunctionalities
{
    static class Constants
    {
        public const string LBDirectory = @"LongBoardsPhotos";
        public const string ApiToken = @"834221048:AAEZ6xzH3DYzz-VOAea_2Fc0-y6GByFLEhE";
        public const long AdminGroupChatId = -1001181244049;
        public const string CancelText = "Отменить";
        public const string AddText = "Добавить";
        public const string YesText = "Да";
        public const string NoText = "Нет";
        public const string FinishText = "Закончить";
        public const string TextFileName = "Text.txt";
        public const string ImageExtension = ".jpg";
        public const string FinalMessagePath = @"Texts\FinalMessage.txt";
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
