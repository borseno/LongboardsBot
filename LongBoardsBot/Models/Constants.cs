﻿using LongBoardsBot.Helpers;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace LongBoardsBot.Models
{
    static partial class Constants
    {
        public const string Url = "https://longboardistbot.azurewebsites.net:443/";
        public const string NickName = "@LongBoard_Dev_Bot"; // nickname (the one that starts with @)
        public const string ApiKey = @"821836757:AAHFbFgSrbrvpGVpzCYWZAwG2Jzo7Cbl1m8";
        public const long AdminGroupChatId = -1001181244049;
        public const long BugReportChatId = 777785046;
    }

    static partial class Constants
    {
        public const string GreetingStickerId = @"CAADAgADKwcAAmMr4gmfxHm1DmV88gI";

        public const string LBDirectory = @"Models\LongBoardsPhotos";
        public const string TextsDirectory = @"Models\Texts";

        public const string GreetingTextPath = TextsDirectory + @"\Greeting.txt";
        public const string FinalMessageToUserPath = TextsDirectory + @"\FinalMessageToUser.txt";

        // 0 -> username; 1 -> list of lboards + their amounts; 2 -> total cost of lboards; 
        // 3 -> Name; 4 -> Phone; 5 -> Info about adress (adress or "Заберет сам")
        public const string FinalMessageToAdminsPath = TextsDirectory + @"\FinalMessageToAdmins.txt";

        // 0 -> Price (in UAH, it is stored in DB in UAH)
        // 1 -> info about this particular longboard (Text.txt file in this longboard's folder)
        public const string LongboardInfoPatternPath = TextsDirectory + @"\LongboardInfoPattern.txt";

        public const string PhotosForLBFileName = "PhotoHashes" + TextExtension;
        public const string LBInfoFileName = "Info" + TextExtension;
        public const string ImageExtension = ".jpg";
        public const string TextExtension = ".txt";
    }

    static partial class Constants
    {
        public const string NameRegexp = @"^[а-яА-Яa-zA-Z][а-яa-z]*$";
        public const string PhoneRegexp = @"^\+?3?8?(0[5-9][0-9]\d{7})$";
    }

    static partial class Constants
    {
        public const string RestartCommand = @"/restart";
        public const string ElementsSeparator = @", ";
        public const string CancelText = "Отменить";
        public const string AddText = "Добавить";
        public const string YesText = "Да";
        public const string NoText = "Нет";
        public const string RestartText = "Начать покупки заново";
        public const string FinishText = "Закончить";
        public const string ShouldRestartText = RestartText + "?";
        public const string ChooseLongBoardText = @"Выберите лонгборд в зависимости от желаемого стиля катания:";
        public const string DeliverOrNotText = @"Вам доставить или вы сами придете?";
        public const string DeliverBtnText = "Доставить";
        public const string TakeMySelfBtnText = "Заберу сам";
        public const string WriteHomeAdressText = @"Напишите свой адрес для доставки";
        public const string PlaceToTakeLongBoardText = @"Адрес для получения лонгборда: ст. метро защитников украины, рэббит кофе";
        public const string EnterCorrectNameText = @"Введите, пожалуйста, настоящее имя для дальнейшего общения!";
        public const string EnterCorrectPhoneText = @"Вы ввели некорректный номер. Ввведите номер, начинающийся на +380...";
        public const string NoMoreStylesText = @"Больше стилей катания на лонгбордах нет! 😌";
        public const string NoDeliveryInfo = @"Заберет сам";
        public const string WantToContinuePurchasingText = @"Вы хотите продолжить покупки?";
        public const string EnterYourNameText = @"Введите ваше имя:";
        public const string EnterYourPhoneText = @"Введите контактный номер телефона:";
        public const string PhotosAreBeingSentText = @"Идет отправка фотографий...";
        public const string AskingToEnterAmountOfLBText =
            @"Вы собираетесь добавить лонгборды {0} стиля катания в корзину. Укажите количество"; // 0 -> style of lboard
        public const string AddedToBasketNotificationText =
            @"Вы успешно добавили {0} лонгбордов {1} стиля катания в корзину!"; // 0 -> amount; 1 -> style of lboard
        public const string UserPurchaseInfoText = @"Вы купили {0}. Стоимость = {1}"; // 0 -> lboards + their amounts
        public const string AfterNameTypedText = @"Здравствуйте, {0}"; // 0 -> name
        public const string AfterPhoneTypedText = @"Вы успешно установили свой номер телефона для обратной связи на {0}"; // 0 -> phone
        public const string ConfirmAddingLBText = @"Вы хотите добавить {0} лонг борд в корзину?"; // 0 -> longboard
        public const string InfoAboutBasket = @"У вас сейчас в корзине: {0} Итого: {1}"; // 0 -> longboards + their amounts, 1 -> total cost
        public const string LBInBasketInfo = @"{0} ({1})"; // 0 - longboard's style, 1 - amount
    }

    static partial class Constants
    { 
        public static readonly ReplyKeyboardMarkup AllLBkboard; // keyboard for all longboards
        public static readonly FileInfo[] AllLBs;
        public static readonly DirectoryInfo BoardsDirectory;
        public static readonly ReplyKeyboardMarkup RestartKBoard;
        public static readonly ReplyKeyboardMarkup DeliverOrNotKBoard;
        public static readonly ReplyKeyboardMarkup AddToBasketOrNotKBoard;
        public static readonly ReplyKeyboardMarkup ContinuePurchasingOrNotKBoard;

        static Constants()
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));

            BoardsDirectory = directory;
            AllLBs = directory.GetFiles();

            AllLBkboard = new ReplyKeyboardMarkup(directory.GetFiles().Select(i => new KeyboardButton(i.NameWithoutExt())), true, true);
            RestartKBoard = new ReplyKeyboardMarkup(new[] { new KeyboardButton(RestartText) }, true, true);

            DeliverOrNotKBoard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton(DeliverBtnText),
                new KeyboardButton(TakeMySelfBtnText)
            }, true, true);

            AddToBasketOrNotKBoard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton(CancelText),
                new KeyboardButton(AddText)
            }, true, true);

            ContinuePurchasingOrNotKBoard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton(YesText),
                new KeyboardButton(CancelText),
                new KeyboardButton(FinishText)
            }, true, true);
        }
    }
}
