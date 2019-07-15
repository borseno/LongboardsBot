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
        public const string ApiToken = @"821836757:AAHFbFgSrbrvpGVpzCYWZAwG2Jzo7Cbl1m8";
        public const long AdminGroupChatId = -1001181244049;
        public const string CancelText = "Отменить";
        public const string AddText = "Добавить";
        public const string YesText = "Да";
        public const string NoText = "Нет";
        public const string FinishText = "Закончить";

        public static readonly ReplyKeyboardMarkup AllLBkboard; // keyboard for all longboards

        static Constants()
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles();
            var buttons = new KeyboardButton[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                buttons[i] = new KeyboardButton(files[i].NameWithoutExt());
            }

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true);

            AllLBkboard = myReplyMarkup;
        }
    }
}
