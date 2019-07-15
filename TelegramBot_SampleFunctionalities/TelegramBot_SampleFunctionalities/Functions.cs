using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using System.IO;
using static TelegramBot_SampleFunctionalities.Constants;
using System.Drawing;
using System.Drawing.Imaging;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_SampleFunctionalities
{
    internal static class Functions
    {
        public static Task<Message> AskName(TelegramBotClient client, long chatId)
            => client.SendTextMessageAsync(chatId, "Здравствуйте! Введите ваше имя");

        public static Task<Message> AskPhone(TelegramBotClient client, long chatId, bool hasUserName)
            => client.SendTextMessageAsync(
                chatId,
                hasUserName ? "Введите контактный номер телефона (по желанию)"
                : "У вас нет username в телеграме. Введите ваш номер телефона, иначе мы не сможем с вами связаться");

        public static async Task SendInfoAbout(string board, BotUser user, TelegramBotClient client)
        {
            var chatId = user.Id;
            var info = GetInfoAsync(board);
            var textFile = info.First(i => i.Name == TextFileName);
            var photos = info.Where(i => i.Extension == ImageExtension);

            string text;
            using (var reader = new StreamReader(textFile.FullName))
            {
                text = await reader.ReadToEndAsync();
            }

            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, "Идет отправка фотографий...");
            var photosMsgTask = SendPhotos(chatId, client, photos);

            await Task.WhenAll(waitForPhotosMsgTask, photosMsgTask);

            var textMsg = await client.SendTextMessageAsync(chatId, text);

            user.History.AppendMsg(false, photosMsgTask.Result);
            user.History.AppendMsg(false, textMsg);
            user.History.AppendMsg(false, waitForPhotosMsgTask.Result);
        }

        public static async Task<string> ReadAllLinesAsync(string path)
        {
            string result;
            using (var reader = new StreamReader(path))
            {
                result = await reader.ReadToEndAsync();
            }
            return result;
        }

        public static Task<Message[]> SendPhotos(long chatId, TelegramBotClient client, IEnumerable<FileInfo> neededFiles)
        {
            var count = neededFiles.Count();
            var photos = new List<InputMediaPhoto>(count); // what if more than 10 photos? TODO!
            var streams = new List<MemoryStream>(count);

            foreach (var i in neededFiles)
            {
                using (var bitmap = new Bitmap(i.FullName)) // critical change here
                {
                    var stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;

                    var nameWithoutExt = i.NameWithoutExt();
                    var photo = new InputMediaPhoto(new InputMedia(stream, nameWithoutExt));

                    photos.Add(photo);
                    streams.Add(stream);
                }
            }

            return client.SendMediaGroupAsync(chatId, photos);
        }

        public static FileInfo[] GetInfoAsync(string board)
        {
            var dir = BoardsDirectory.GetDirectories().First(i => i.Name == board);
            var files = dir.GetFiles();
            return files;
        }

        public static async Task ClearHistory(BotUser entry, TelegramBotClient client)
        {
            var chatId = entry.Id;
            var list = entry.History;
            var deleteTasks = new List<Task>(list.Count);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var elem = list[i];
                if (!elem.IgnoreDelete)
                {
                    deleteTasks.Add(client.DeleteMessageAsync(chatId, elem.MsgId));
                    list.RemoveAt(i);
                }
            }

            await Task.WhenAll(deleteTasks);
        }

        public static async Task RestartDialog(BotUser entry, TelegramBotClient client)
        {
            var chatId = entry.Id;

            await ClearHistory(entry, client);

            var msg1 = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: AllLBkboard);

            entry.History.Add(new ChatMessage(msg1.MessageId, false));
            entry.Longboards.Clear();

            entry.Stage = Stage.ProcessingLongboardsKeyboardInput;
        }

        public static async Task StartNewDialog(BotUser entry, TelegramBotClient client)
        {
            var chatId = entry.Id;

            var msg1 = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: AllLBkboard);

            entry.History.Add(new ChatMessage(msg1.MessageId, false));
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

            var allFiles = AllLBImages;
            var neededfiles = allFiles.Where(i => !filesToIgnore.Contains(i.NameWithoutExt())).ToArray();
            var success = neededfiles.Length >= 1;

            if (!success)
            {
                var msg1 = await client.SendTextMessageAsync(chatId, "Больше лонгбордов нет! 😌");
                msgStorage.Add(new ChatMessage(msg1.MessageId, false));
                return success;
            }

            var photos = new List<InputMediaPhoto>(neededfiles.Length); // what if more than 10 photos? TODO!
            var streams = new List<MemoryStream>(neededfiles.Length);
            var buttons = new List<KeyboardButton>(neededfiles.Length);

            // init photos, streams (for photos), buttons
            foreach (var i in neededfiles)
            {
                using (var bitmap = new Bitmap(i.FullName)) // critical change here
                {
                    var stream = new MemoryStream();
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;

                    var nameWithoutExt = i.NameWithoutExt();
                    var photo = new InputMediaPhoto(new InputMedia(stream, nameWithoutExt));

                    photos.Add(photo);
                    streams.Add(stream);
                    buttons.Add(new KeyboardButton(nameWithoutExt));
                }
            }

            // after buttons and photos are inited, we can init keyboard (cuz buttons are inited)
            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true);

            // send photos
            var msgs = await client.SendMediaGroupAsync(chatId, photos);

            // send msg + keyboard
            var msg = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: myReplyMarkup);


            // now process the result. Add messages to history. Dispose the streams.
  
            msgStorage.Add(new ChatMessage(msg.MessageId, false));

            // now that photos are sent we don't need to keep them in memory anymore
            Parallel.ForEach(streams, i => i.Dispose()); 

            // if a message contains all longboards
            if (neededfiles.Length == AllLBImages.Length)
            {
                // then don't delete it and use in the future ----------------------\/
                msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, true)));
            }
            else // otherwise, don't
            {
                msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, false)));
            }

            return success;
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
            var text = $"Вы хотите добавить {chosen} лонг борд в корзину?";
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
}
