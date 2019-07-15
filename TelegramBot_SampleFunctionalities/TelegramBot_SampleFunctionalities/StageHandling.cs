using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using System.IO;
using static TelegramBot_SampleFunctionalities.Functions;
using static TelegramBot_SampleFunctionalities.Constants;
using System.Drawing;
using System.Drawing.Imaging;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;

namespace TelegramBot_SampleFunctionalities
{
    public enum Stage
    {
        AskingName = 0,
        GettingName = 1,
        GettingPhone = 2,
        ProcessingLongboardsKeyboardInput = 3,
        ProcessingBasketKeyboardInput = 4,
        AskingIfShouldContinueAddingToBasket = 5
    }
    public static class StageHandling
    {
        private static readonly List<BotUser> storage = new List<BotUser>(16);
        private static readonly SemaphoreSlim slim = new SemaphoreSlim(1, 1);
        public static readonly ReplyKeyboardMarkup allLBkboard;
        
        static StageHandling()
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles();
            var buttons = new KeyboardButton[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                buttons[i] = new KeyboardButton(files[i].NameWithoutExt());
            }

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true);

            allLBkboard = myReplyMarkup;
        }

        public static async Task HandleUpdate(TelegramBotClient client, Update update)
        {
            await slim.WaitAsync();
            try
            {
                if (update.Message == null)
                    return;

                var chatId = update.Message.Chat.Id;
                var entry = storage.FirstOrDefault(i => i.Id == chatId);

                if (entry == null)
                {
                    entry = new BotUser
                    {
                        Id = chatId,
                        Stage = 0
                    };

                    storage.Add(entry);
                }

                entry.History.Add(new ChatMessage(update.Message.MessageId, false));

                if (entry.Stage == Stage.AskingName)
                {
                    var msg = await AskName(client, chatId);

                    entry.History.Add(new ChatMessage(msg.MessageId, false));

                    entry.Stage = Stage.GettingName;
                }
                else if (entry.Stage == Stage.GettingName)
                {
                    var msg2 = await UpdateUsersNameAndUsername(client, update.Message, storage);
                    var msg1 = await AskPhone(client, update.Message.Chat.Id, !String.IsNullOrWhiteSpace(update.Message.Chat.Username));

                    entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                    entry.Stage = Stage.GettingPhone;
                }
                else if (entry.Stage == Stage.GettingPhone)
                {
                    var msg = await UpdateUsersPhoneAndUsername(client, update.Message, storage);

                    entry.History.Add(new ChatMessage(msg.MessageId, false));

                    await SendLongBoards(client, update.Message.Chat.Id, entry.History);
                    entry.Stage = Stage.ProcessingLongboardsKeyboardInput;
                }
                else if (entry.Stage == Stage.ProcessingLongboardsKeyboardInput)
                {
                    var lb = update.Message.Text;

                    var isLongBoard = ValidateLongBoard(lb);

                    if (isLongBoard)
                    {
                        entry.Pending = lb;
                        var msg = await SendShouldAddToBasketKeyboard(client, chatId, lb);

                        entry.History.Add(new ChatMessage(msg.MessageId, false));

                        entry.Stage = Stage.ProcessingBasketKeyboardInput;
                    }
                }
                else if (entry.Stage == Stage.ProcessingBasketKeyboardInput)
                {
                    bool ValidateResult(string value) => value == CancelText || value == AddText;

                    var result = update.Message.Text;

                    if (ValidateResult(result))
                    {
                        if (result == AddText)
                        {
                            entry.Longboards.Add(entry.Pending);
                            var msg = await client.SendTextMessageAsync(chatId, $"Вы успешно добавили {entry.Pending} лонгборд в корзину!");

                            entry.History.Add(new ChatMessage(msg.MessageId, false));

                            await Task.Delay(100);
                        }

                        entry.Pending = null;

                        var msg1 = await SendInfoAboutBasket(client, entry);
                        var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                        entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                        entry.Stage = Stage.AskingIfShouldContinueAddingToBasket;
                    }
                }
                else if (entry.Stage == Stage.AskingIfShouldContinueAddingToBasket)
                {
                    var result = update.Message.Text;

                    if (result == YesText)
                    {
                        var successful = await SendLongBoards(client, entry.Id, entry.History, entry.Longboards);

                        if (successful)
                            entry.Stage = Stage.ProcessingLongboardsKeyboardInput;
                        else
                        {
                            var msg1 = await SendInfoAboutBasket(client, entry);
                            var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                            entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                            entry.Stage = Stage.AskingIfShouldContinueAddingToBasket;
                        }
                    }
                    else if (result == CancelText)
                    {
                        await ClearHistory(entry, client);
                    }
                    else if (result == FinishText)
                    {
                        var lbrds = String.Join(", ", entry.Longboards);
                        var phone = entry.Phone;
                        var username = entry.UserName;
                        var name = entry.Name;
                        var userChatId = entry.Id;

                        var msgToAdminGroup = 
                            $"@{username} хочет купить {{{lbrds}}}{Environment.NewLine}" +
                            $"Имя: {name}, Телефон: {phone}";

                        var msgToUser = $"Вы купили {lbrds}. Стоимость = 100 долларов. Деньги отдать наличкой в бро кофе. Метро защитников украины, г. Харьков. Дима встретит";

                        await client.SendTextMessageAsync(AdminGroupChatId, msgToAdminGroup);
                        var msg = await client.SendTextMessageAsync(userChatId, msgToUser);

                        entry.History.Add(new ChatMessage(msg.MessageId, true));

                        await ClearHistory(entry, client);
                    }
                }

            }
            finally
            {
                slim.Release();
            }
        }
    }

    internal static class Functions
    {
        public static Task<Message> AskName(TelegramBotClient client, long chatId) => client.SendTextMessageAsync(chatId, "Здравствуйте! Введите ваше имя");
        public static Task<Message> AskPhone(TelegramBotClient client, long chatId, bool hasUserName)
            => client.SendTextMessageAsync(
                chatId,
                hasUserName ? "Введите контактный номер телефона (по желанию)"
                : "У вас нет username в телеграме. Введите ваш номер телефона, иначе мы не сможем с вами связаться");

        public static async Task ClearHistory(BotUser entry, TelegramBotClient client)
        {
            var chatId = entry.Id;
            var list = entry.History;
            var deleteTasks = new List<Task>(list.Count);

            for (int i = list.Count - 1; i > 0; i--)
            {
                var elem = list[i];
                if (!elem.IgnoreDelete)
                {
                    deleteTasks.Add(client.DeleteMessageAsync(chatId, elem.MsgId));
                    list.RemoveAt(i);
                }
            }

            await Task.WhenAll(deleteTasks);

            var msg = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: StageHandling.allLBkboard);

            entry.History.Add(new ChatMessage(msg.MessageId, false));
            entry.Longboards.Clear();

            entry.Stage = Stage.ProcessingLongboardsKeyboardInput;
        }

        public static Task<Message> SendShouldContinueAddingToBasket(TelegramBotClient client, BotUser entry)
        {
            var text = $"Вы хотите продолжить покупки?";
            var btnYes = new KeyboardButton(YesText);
            var btnCancel = new KeyboardButton(CancelText);
            var btnFinish = new KeyboardButton(FinishText);

            var keyboard = new ReplyKeyboardMarkup(new[] { btnYes, btnCancel, btnFinish }, true);

            return client.SendTextMessageAsync(entry.Id, text, replyMarkup: keyboard);
        }

        public static async Task<bool> SendLongBoards(TelegramBotClient client, long chatId, List<ChatMessage> msgStorage,
            IEnumerable<string> filesToIgnore = null)
        {
            if (filesToIgnore == null)
                filesToIgnore = Enumerable.Empty<string>();

            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles().Where(i => !filesToIgnore.Contains(i.NameWithoutExt())).ToArray();
            var images = new List<KeyValuePair<InputMediaPhoto, MemoryStream>>(files.Length);
            var buttons = new KeyboardButton[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var nameWithoutExt = files[i].NameWithoutExt();
                var bitmap = new Bitmap(files[i].FullName);
                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;

                var photo = new InputMediaPhoto(new InputMedia(stream, nameWithoutExt));

                images.Add(new KeyValuePair<InputMediaPhoto, MemoryStream>(photo, stream));
                buttons[i] = new KeyboardButton(nameWithoutExt);
            }

            var photos = images.Select(i => i.Key).ToArray(); // what if more than 10 photos? TODO!

            if (photos.Length >= 1)
            {
                var msgs = await client.SendMediaGroupAsync(chatId, photos);

                if (files.Length == directory.GetFiles().Length)
                {
                    msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, true)));
                }
                else
                {
                    msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, false)));
                }
            }
            else
            {
                var msg = await client.SendTextMessageAsync(chatId, "Больше лонгбордов нет! 😌");

                msgStorage.Add(new ChatMessage(msg.MessageId, false));
            }

            foreach (var i in images)
            {
                i.Value.Dispose();
            }

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true);

            if (photos.Length >= 1)
            {
                var msg = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: myReplyMarkup);
                msgStorage.Add(new ChatMessage(msg.MessageId, false));
            }

            return photos.Length >= 1;
        }

        public static bool ValidateLongBoard(string text)
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles();
            var exists = files.Any(i => i.Name.Contains(text));

            return exists;
        }

        public static Task<Message> SendInfoAboutBasket(TelegramBotClient client, BotUser user)
            => client.SendTextMessageAsync(user.Id, "У вас сейчас в корзине: " + String.Join(", ", user.Longboards));

        public static Task<Message> SendShouldAddToBasketKeyboard(TelegramBotClient client, long chatId, string chosen)
        {
            //var text = "У вас сейчас в корзине: " + String.Join(", ", basket);
            var text = $"Вы хотите добавить {chosen} лонг борд в корзину";
            var btnCancel = new KeyboardButton(CancelText);
            var btnAdd = new KeyboardButton(AddText);

            var keyboard = new ReplyKeyboardMarkup(new[] { btnCancel, btnAdd }, true);

            return client.SendTextMessageAsync(chatId, text, replyMarkup: keyboard);
        }

        // returns false is longboard is not correct or doesnt exist
        public static Task<Message> SendInfoAboutLongBoard(TelegramBotClient client, long chatId)
        {
            var longboardInfo = "Sample info about longboard " + Environment.NewLine + " It costs 300$";

            return client.SendTextMessageAsync(chatId, longboardInfo);
        }

        public static Task<Message> UpdateUsersPhoneAndUsername(TelegramBotClient client, Message message, List<BotUser> storage)
        {
            var id = message.Chat.Id;
            var phone = message.Text;
            var username = message.Chat.Username;
            var entry = storage.First(i => i.Id == id);

            entry.UserName = username;
            entry.Phone = phone;

            return client.SendTextMessageAsync(id, $"Вы успешно установили свой номер телефона для обратной связи на {phone}");
        }

        public static Task<Message> UpdateUsersNameAndUsername(TelegramBotClient client, Message message, List<BotUser> storage)
        {
            var id = message.Chat.Id;
            var name = message.Text;
            string phone = null;
            var username = message.Chat.Username;

            var entry = storage.First(i => i.Id == id);

            entry.Phone = phone;
            entry.UserName = username;
            entry.Name = name;

            return client.SendTextMessageAsync(id, $"Здравствуйте, {name}");
        }
    }

    public static class FileExtensions
    {
        public static string NameWithoutExt(this FileInfo file)
            => file.Name.Remove(file.Name.Length - file.Extension.Length, file.Extension.Length);
    }
}
