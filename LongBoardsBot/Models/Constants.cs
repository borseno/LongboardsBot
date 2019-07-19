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
        public const string GreetingText = @"Здравствуйте, вас приветствует компания Лонгбордист! 
Купить лонгборд для любого стиля катания, можно используя этот бот.
Цена указывается после выбора желаемого стиля (подробнее о стилях можно почитать на сайте longboardist.kharkiv.com)";
        public const string ChooseLongBoardText = @"Выберите лонгборд в зависимости от желаемого стиля катания:";
        public const string DeliverOrNotText = @"Вам доставить или вы сами придете?";
        public const string DeliverBtnText = "Доставить";
        public const string TakeMySelfBtnText = "Заберу сам";
        public const string WriteHomeAdressText = @"Напишите свой адрес для доставки";
        public const string PlaceToTakeLongBoardText = @"Адрес для получения лонгборда: ст. метро защитников украины, рэббит кофе";
        public const string EnterCorrectNameText = @"Введите, пожалуйста, настоящее имя для дальнейшего общения!";
        public const string EnterCorrectPhoneText = @"Вы ввели некорректный номер. Ввведите номер, начинающийся на +380...";
        public const string RestartCommand = @"/restart";
        public const string PhotosAreBeingSentText = @"Идет отправка фотографий...";
        public const string AskingToEnterAmountOfLBText = 
            @"Вы собираетесь добавить лонгборды {0} стиля катания в корзину. Укажите количество"; // 0 -> style of lboard
        public const string AddedToBasketNotificationText = 
            @"Вы успешно добавили {0} лонгбордов {1} стиля катания в корзину!"; // 0 -> amount; 1 -> style of lboard

        public const string NameRegexp = @"^[а-яА-Яa-zA-Z][а-яa-z]*$";
        public const string PhoneRegexp = @"^\+?3?8?(0[5-9][0-9]\d{7})$";

        public static readonly ReplyKeyboardMarkup AllLBkboard; // keyboard for all longboards
        public static readonly FileInfo[] AllLBImages;
        public static readonly DirectoryInfo BoardsDirectory;
        public static readonly ReplyKeyboardMarkup RestartKBoard;
        public static readonly ReplyKeyboardMarkup DeliverOrNotKBoard;

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
            RestartKBoard = new ReplyKeyboardMarkup(new[] { new KeyboardButton(RestartText) }, oneTimeKeyboard: true, resizeKeyboard: true);

            DeliverOrNotKBoard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton(DeliverBtnText),
                new KeyboardButton(TakeMySelfBtnText)
            }, true, true);
        }
    }
}
