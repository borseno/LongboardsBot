using LongBoardsBot.Helpers;
using System;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace LongBoardsBot.Models
{
    static partial class Constants
    {
        public const string Url = "https://longboardistbot1.azurewebsites.net:443/";
        public const string NickName = "LongboardistBot"; // nickname (the one that starts with @)
        public const string ApiKey = @"678399349:AAF9TQBnP3uMT1Jn_CjewoohpmgOoGMo6Lo";
        public const long AdminGroupChatId = -1001181244049;
        public const long BugReportChatId = 446310692;
    }

    static partial class Constants
    {
        public const string GreetingStickerId = @"CAADAgADKwcAAmMr4gmfxHm1DmV88gI";

        public const string LBDirectory = @"Models\LongBoardsPhotos";
        public const string TextsDirectory = @"Models\Texts";

        public const string GreetingTextPath = TextsDirectory + @"\Greeting.txt";
        public const string FinalMessageToUserPath = TextsDirectory + @"\FinalMessageToUser.txt";
        public const string DeliveryNotificationPath = TextsDirectory + @"\DeliveryNotification.txt";
        public const string CancelledOrderingNotificationPath = TextsDirectory + @"\CancelledOrderingNotification.txt";
        public const string FinalTestingTextToAdminsPath = TextsDirectory + @"\TestingFinalMessageToAdmins.txt";
        public const string FinalTestingTextToUserPath = TextsDirectory + @"\TestingFinalMessageToUser.txt";

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
        public const string DateTimeFormat = @"dd.MM.yyyy HH:mm";

        public const string NextComment = "NEXT";
        public const string PreviousComment = "PREV";
        public const string FinishComment = "FINISH";

        public const string SuccessfullySent = @"Уведомление было успешно отправлено пользователю";
        public const string SoldMessage = @"ПРОДАНО И ДОСТАВЛЕНО";
        public const string WantsAddComment = @"Хорошо";
        public const string NotWantsAddComment = @"Лень)";
        public const string RestartCommand = @"/" + @"restart";
        public const string GetCommentsCommand = @"/" + @"getcomments";

        public const string ElementsSeparator = @", ";

        public const string StartPurchasingText = "Оформить покупку";
        public const string StartTestingText = "Оформить тестинг";
        public const string MenuText = "Выберите дальнейшее действие";

        public static readonly string AskDateOfVisitText = "Мы доступны с 08:00 по 22:00 в любой день." + Environment.NewLine + "Введите пожалуйста, дату визита, которая вам подходит в формате {0}, где ММ - месяц, dd - день, yyyy - год, HH - час, mm - минуты";
        public const string WantsToTypeStatisticsText = "Хотите ввести дополнительные данные для статистики?";
        public const string CleanUpBasketText = "Очистить корзину";
        public const string CancelText = "Отменить";
        public const string AddText = "Добавить";
        public const string YesText = "Да";
        public const string NoText = "Нет";
        public const string RestartText = "Обратно в меню";
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
        public const string EnterYourAgeText = @"Введите ваш возраст:";
        public const string EnterYourPhoneText = @"Введите контактный номер телефона:";
        public const string PhotosAreBeingSentText = @"Идет отправка фотографий...";
        public const string AskingToEnterAmountOfLBText =
            @"Вы собираетесь добавить лонгборды {0} стиля катания в корзину. Укажите количество"; // 0 -> style of lboard
        public const string AddedToBasketNotificationText =
            @"Вы успешно добавили {0} лонгбордов {1} стиля катания в корзину!"; // 0 -> amount; 1 -> style of lboard
        public const string UserPurchaseInfoText = @"Вы купили {0}. Стоимость: {1}. Номер покупки: {2}"; // 0 -> lboards + their amounts
        public const string AfterNameTypedText = @"Здравствуйте, {0}"; // 0 -> name
        public const string AfterPhoneTypedText = @"Вы успешно установили свой номер телефона для обратной связи на {0}"; // 0 -> phone
        public const string ConfirmAddingLBText = @"Вы хотите добавить {0} лонг борд в корзину?"; // 0 -> longboard
        public const string InfoAboutBasket = @"У вас сейчас в корзине: {0} Итого: {1}"; // 0 -> longboards + their amounts, 1 -> total cost
        public const string LBInBasketInfo = @"{0} ({1})"; // 0 - longboard's style, 1 - amount

        public const string DeliveredText = "Доставили";
        public const string CancelDeliveryText = "Отмена";

        public const string TestedText = "Протестировано";
        public const string CancelTestingText = "Отмена";
    }

    // data for development only! pls dont touch
    static partial class Constants
    {
        public const string CancelTestingData = "CancelTesting#";
        public const string TestedData = "Tested#";

        public const string CancelDeliveryData = "CancelDelivery#";
        public const string DeliveredData = "Delivered#";

        public const string LinkFormat = @"[{0}](tg://user?id={1})";
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
                new KeyboardButton(CleanUpBasketText),
                new KeyboardButton(FinishText)
            }, true, true);
        }
    }

    static partial class Constants
    {
        public static ReplyKeyboardMarkup WantsToSendReviewOrNotKboard
            => new ReplyKeyboardMarkup(
                    new[] {
                        new KeyboardButton(WantsAddComment),
                        new KeyboardButton(NotWantsAddComment)
                    }, true, true);

        public static ReplyKeyboardMarkup CancelKeyboard
            => new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton(CancelText)
                    }, true, true);
    }
}
