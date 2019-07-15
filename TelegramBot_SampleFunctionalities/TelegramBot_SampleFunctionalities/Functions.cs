﻿using System;
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
}
