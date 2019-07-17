using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using System.IO;
using static LongBoardsBot.Models.Constants;
using System.Drawing;
using System.Drawing.Imaging;
using Telegram.Bot.Types.ReplyMarkups;
using LongBoardsBot.Helpers;
using Microsoft.EntityFrameworkCore;
using LongBoardsBot.Models.Entities;

namespace LongBoardsBot.Models
{
    internal static class Functions
    {
        /// <summary>
        /// <para>1. Sends photos and text about board</para>
        /// <para>2. Adds photos and text history to the user</para>
        /// </summary>
        /// <param name="board">dfdf</param>
        /// <param name="user"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task SendInfoAbout(LongBoard board, BotUser user, TelegramBotClient client)
        {
            var chatId = user.ChatId;
            var info = GetInfo(board.Style, BoardsDirectory);
            var textFile = info.First(i => i.Name == TextFileName);
            var photos = info.Where(i => i.Extension == ImageExtension);
            
            var textTask = FileExtensions.ReadAllLinesAsync(textFile.FullName);
            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, "Идет отправка фотографий...");
            var photosMsgTask = SendPhotos(chatId, client, photos);

            await Task.WhenAll(waitForPhotosMsgTask, photosMsgTask, textTask);

            var toSend = $"Цена: {board.Price}" + Environment.NewLine + textTask.Result;
            var textMsg = await client.SendTextMessageAsync(chatId, toSend);

            user.History.AddRange(photosMsgTask.Result.Select(i => new ChatMessage(i.MessageId, false)));
            user.History.Add(new ChatMessage(textMsg.MessageId, false));
            user.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));
        }

        /// <summary>
        /// <para>1. Sends neededFiles to chatId from client</para>
        /// <para>2. Returns messages that were sent</para>
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="client"></param>
        /// <param name="neededFiles"></param>
        /// <returns></returns>
        public static async Task<Message[]> SendPhotos(long chatId, TelegramBotClient client, IEnumerable<FileInfo> neededFiles)
        {
            var count = neededFiles.Count();
            var photos = new List<InputMediaPhoto>(count); // what if more than 10 photos? TODO!
            var streams = new List<MemoryStream>(count);

            try
            {
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
                var messages = await client.SendMediaGroupAsync(chatId, photos);

                return messages;
            }
            finally
            {
                foreach (var i in streams)
                {
                    i?.Dispose();
                }
            }
        }

        /// <summary>
        /// returns list of files in a given subdirectory of a startDirectory
        /// </summary>
        /// <param name="subDirectory"></param>
        /// <param name="startDirectory"></param>
        /// <returns></returns>
        public static FileInfo[] GetInfo(string subDirectory, DirectoryInfo startDirectory)
            => startDirectory.GetDirectories().First(i => i.Name == subDirectory).GetFiles();    

        /// <summary>
        /// <para>1. deletes all not marked as "deleteIgnore" messages that are in the History property of a given instance of BotUser class</para>
        /// <para>2. removes all the deleted messages from the History property of a given instance</para>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static Task ClearHistory(BotUser instance, TelegramBotClient client)
        {
            var chatId = instance.ChatId;
            var list = instance.History;
            var deleteTasks = new List<Task>(list.Count);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var elem = list.ElementAt(i);
                if (!elem.IgnoreDelete)
                {
                    deleteTasks.Add(client.DeleteMessageAsync(chatId, elem.MessageId));
                    list.RemoveAt(i);
                }
            }

            return Task.WhenAll(deleteTasks);
        }

        /// <summary>
        /// <para>1. Clears history of a given instance</para>
        /// <para>2. Sets entity's stage to ProcessingLongboardsKeyboardInput</para>
        /// <para>3. Sends longboards keyboardand asks to choose longboard</para>
        /// <para>4. clears all longboards from Longboards property</para>
        /// <para>5. adds sent messages to History property</para>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task RestartDialog(BotUser instance, TelegramBotClient client)
        {
            await ClearHistory(instance, client);
            await StartNewDialog(instance, client);
        }
        /// <summary>
        /// <para>1. Sets entity's stage to ProcessingLongboardsKeyboardInput</para>
        /// <para>2. Sends longboards keyboardand asks to choose longboard</para>
        /// <para>3. clears all longboards from Longboards property</para>
        /// <para>4. adds sent messages to History property</para>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task StartNewDialog(BotUser instance, TelegramBotClient client)
        {
            var chatId = instance.ChatId;

            var msg1 = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: AllLBkboard);

            instance.History.Add(new ChatMessage(msg1.MessageId, false));
            instance.BotUserLongBoards.Clear();
            instance.Pending = null;

            instance.Stage = Stage.ProcessingLongboardsKeyboardInput;
        }

        public static Task<Message> SendShouldContinueAddingToBasket(TelegramBotClient client, BotUser instance)
        {
            var text = $"Вы хотите продолжить покупки?";
       
            var btnYes = new KeyboardButton(YesText);
            var btnCancel = new KeyboardButton(CancelText);
            var btnFinish = new KeyboardButton(FinishText);

            var keyboard = new ReplyKeyboardMarkup(new[] { btnYes, btnCancel, btnFinish }, true, true);

            return client.SendTextMessageAsync(instance.ChatId, text, replyMarkup: keyboard);
        }

        public static async Task<bool> SendLongBoards(TelegramBotClient client, long chatId, IList<ChatMessage> msgStorage,
            IEnumerable<LongBoard> boardsToIgnore = null)
        {
            if (boardsToIgnore == null)
                boardsToIgnore = Enumerable.Empty<LongBoard>();

            var allFiles = AllLBImages;
            var neededfiles = allFiles.Where(i => !boardsToIgnore.Any(j => j.Style == i.NameWithoutExt()) ).ToArray();
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
                using (var bitmap = new Bitmap(i.FullName))
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
            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, resizeKeyboard: true, oneTimeKeyboard: true);

            // send photos
            var msgs = await client.SendMediaGroupAsync(chatId, photos);

            // send msg + keyboard
            var msg = await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: myReplyMarkup);

            // now that photos are sent we don't need to keep them in memory anymore
            streams.ForEach(i => i.Dispose());

            // now process the result. Add messages to history.
            msgStorage.Add(new ChatMessage(msg.MessageId, false));

            // if a message contains all longboards
            if (neededfiles.Length == AllLBImages.Length)
            {
                // then add it to history of messages but don't delete it and use in the future
                msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, ignoreDelete: true)));
            }
            else // otherwise, don't use in the future
            {
                msgStorage.AddRange(msgs.Select(i => new ChatMessage(i.MessageId, ignoreDelete: false)));
            }

            return success;
        }

        public static bool ExistsLongBoard(string text)
        {
            var directory = BoardsDirectory;
            var files = directory.GetFiles();
            var exists = files.Any(i => i.Name.Contains(text));

            return exists;
        }

        public static Task<Message> SendInfoAboutBasket(TelegramBotClient client, BotUser user)
            => client.SendTextMessageAsync(user.ChatId, "У вас сейчас в корзине: " + 
                string.Join(", ", user
                    .BotUserLongBoards
                    .Select(i => i.Longboard)
                    .Select(i => i.Style)));

        public static Task<Message> SendShouldAddToBasketKeyboard(TelegramBotClient client, long chatId, string chosen)
        {
            var text = $"Вы хотите добавить {chosen} лонг борд в корзину?";
            var btnCancel = new KeyboardButton(CancelText);
            var btnAdd = new KeyboardButton(AddText);

            var keyboard = new ReplyKeyboardMarkup(new[] { btnCancel, btnAdd }, resizeKeyboard: true, oneTimeKeyboard: true);

            return client.SendTextMessageAsync(chatId, text, replyMarkup: keyboard);
        }

        /// <summary>
        /// <para>1. Changes user's username and phone</para>
        /// <para>2. Sends text message about this action</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static async Task<Message> UpdateUsersPhoneAndUsername(TelegramBotClient client, Message message, DbSet<BotUser> storage)
        {
            var id = message.Chat.Id;
            var phone = message.Text;
            var username = message.Chat.Username;
            var entry = await storage.FirstAsync(i => i.ChatId == id);

            entry.UserName = username;
            entry.Phone = phone;

            return await client.SendTextMessageAsync(id, $"Вы успешно установили свой номер телефона для обратной связи на {phone}");
        }

        /// <summary>
        /// <para>1. Changes user's username and name</para>
        /// <para>2. Sends text message about this action</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static async Task<Message> UpdateUsersNameAndUsername(TelegramBotClient client, Message message, DbSet<BotUser> storage)
        {
            var id = message.Chat.Id;
            var name = message.Text;
            string phone = null;
            var username = message.Chat.Username;

            var entry = await storage.FirstAsync(i => i.ChatId == id);

            entry.Phone = phone;
            entry.UserName = username;
            entry.Name = name;

            return await client.SendTextMessageAsync(id, $"Здравствуйте, {name}");
        }
    }
}
