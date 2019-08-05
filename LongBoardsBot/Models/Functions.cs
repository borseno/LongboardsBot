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
using LongBoardsBot.Models.Entities;
using Telegram.Bot.Exceptions;
using static System.String;
using static System.IO.File;

namespace LongBoardsBot.Models
{
    internal static class Functions
    {
        private const string IdsNoMatchMessage = "Chat ids dont match";

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
            var lbInfoPatternTask = Texts.GetLongBoardInfoText();

            var chatId = user.ChatId;
            var info = GetInfo(board.Style, BoardsDirectory);
            var textFile = info.First(i => i.Name == LBInfoFileName);

            var textTask = ReadAllTextAsync(textFile.FullName);
            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, PhotosAreBeingSentText);
            var photosMsgTask = SendLBExistingPhotos(chatId, client, board.Style);

            await Task.WhenAll(waitForPhotosMsgTask, photosMsgTask, textTask, lbInfoPatternTask);

            var toSend = Format(lbInfoPatternTask.Result, board.Price, textTask.Result);
            var textMsg = await client.SendTextMessageAsync(chatId, toSend);

            user.History.AddMessages(photosMsgTask.Result, false);
            user.History.AddMessage(textMsg, false);
            user.History.AddMessage(waitForPhotosMsgTask.Result, false);
        }

        public static Task<Message> SendInfoAboutBasket(TelegramBotClient client, BotUser user)
        {
            var text = Format(InfoAboutBasket, Join(ElementsSeparator, user.Basket), user.Basket.GetCost());

            return client.SendTextMessageAsync(user.ChatId, text);
        }

        public static Task<Message> SendShouldAddToBasketKeyboard(TelegramBotClient client, long chatId, string chosen)
        {
            var text = Format(ConfirmAddingLBText, chosen);

            return client.SendTextMessageAsync(chatId, text, replyMarkup: AddToBasketOrNotKBoard);
        }

        /// <summary>
        /// <para>1. Sends neededFiles to chatId from client</para>
        /// <para>2. Returns messages that were sent</para>
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="client"></param>
        /// <param name="neededFiles"></param>
        /// <returns></returns>
        public static Task<Message[]> SendPhotos(long chatId, TelegramBotClient client, IEnumerable<FileInfo> neededFiles)
        {
            return SendAllLBExistingPhotos(chatId, client, neededFiles);
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
        /// Can throw ApiRequestException, which should be ignored (telegram doesnt allow to delete messages older than 2 days)
        /// <para>1. deletes all not marked as "deleteIgnore" messages that are in the History property of a given instance of BotUser class</para>
        /// <para>2. removes all the deleted messages from the History property of a given instance</para>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task ClearHistory(BotUser instance, TelegramBotClient client, bool deleteAll = false)
        {
            var chatId = instance.ChatId;
            var list = instance.History;
            var deleteTasks = new List<Task>(list.Count);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var elem = list.ElementAt(i);
                try
                {
                    if (!elem.IgnoreDelete || deleteAll)
                    {
                        list.RemoveAt(i);
                        deleteTasks.Add(client.DeleteMessageAsync(chatId, elem.MessageId));
                    }
                }
                catch (ApiRequestException)
                {
                    //list.Remove(elem);
                }
            }

            try
            {
                await Task.WhenAll(deleteTasks);
            }
            catch (ApiRequestException)
            {

            }
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
        public static async Task RestartPurchasing(BotUser instance, TelegramBotClient client)
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
            instance.Stage = Stage.WhatLongBoard;

            var chatId = instance.ChatId;

            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, PhotosAreBeingSentText);
            var sendingLongBoardsTask = SendLongBoards(client, chatId, instance.History);

            instance.Basket.Clear();
            instance.Pending = null;

            await Task.WhenAll(waitForPhotosMsgTask, sendingLongBoardsTask);

            instance.History.AddMessage(waitForPhotosMsgTask.Result, false);
        }

        public static Task<Message> SendShouldContinueAddingToBasket(TelegramBotClient client, BotUser instance)
            => client.SendTextMessageAsync(instance.ChatId, WantToContinuePurchasingText, replyMarkup: ContinuePurchasingOrNotKBoard);

        public static async Task<bool> SendLongBoards(TelegramBotClient client, long chatId, IList<ChatMessage> msgStorage,
            IEnumerable<LongBoard> boardsToIgnore = null)
        {
            if (boardsToIgnore == null)
                boardsToIgnore = Enumerable.Empty<LongBoard>();

            var neededFiles = AllLBs.Where(i => !boardsToIgnore.Any(j => j.Style == i.NameWithoutExt())).ToArray();
            var success = neededFiles.Length >= 1;

            if (!success)
            {
                var msg1 = await client.SendTextMessageAsync(chatId, NoMoreStylesText);
                msgStorage.AddMessage(msg1, false);
                return success;
            }

            var msgsTask = SendPhotos(chatId, client, neededFiles);
            var buttons = GetButtons(neededFiles).ToArray();

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, resizeKeyboard: true, oneTimeKeyboard: true);

            await msgsTask;

            var msg = await client.SendTextMessageAsync(chatId, ChooseLongBoardText, replyMarkup: myReplyMarkup);

            // now process the result. Add messages to history.
            msgStorage.AddMessage(msg, false);

            msgStorage.AddMessages(msgsTask.Result, false);

            return success;
        }

        private static IEnumerable<KeyboardButton> GetButtons(FileInfo[] neededFiles)
        {
            IEnumerable<KeyboardButton> GetButtonsTxt() =>
            neededFiles
               .Where(i => i.Extension == TextExtension)
               .Select(i => new KeyboardButton(i.NameWithoutExt()));

            IEnumerable<KeyboardButton> GetButtonsJpg() =>
            neededFiles
                .Where(i => i.Extension == ImageExtension)
                .Select(i => new KeyboardButton(i.NameWithoutExt()));

            return GetButtonsTxt();
        }
        /// <summary>
        /// <para>1. Changes user's username and phone</para>
        /// <para>2. Sends text message about this action</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static Task<Message> UpdateUsersPhoneAndUsername(TelegramBotClient client, Message message, BotUser instance)
        {
            var chatId = instance.ChatId;

            if (message.Chat.Id != chatId)
            {
                throw new ArgumentException(IdsNoMatchMessage);
            }

            var phone = message.Text;
            var task = client.SendTextMessageAsync(chatId, Format(AfterPhoneTypedText, phone));

            instance.UserName = message.Chat.Username;
            instance.Phone = phone;

            return task;
        }

        /// <summary>
        /// <para>1. Changes user's username and name</para>
        /// <para>2. Sends text message about this action</para>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static Task<Message> UpdateUsersNameAndUsername(TelegramBotClient client, Message message, BotUser instance)
        {
            var chatId = instance.ChatId;

            if (message.Chat.Id != chatId)
            {
                throw new ArgumentException(IdsNoMatchMessage);
            }

            var name = message.Text;

            var msgTask = client.SendTextMessageAsync(chatId, Format(AfterNameTypedText, name));

            instance.UserName = message.Chat.Username;
            instance.Name = name;

            return msgTask;
        }

        public static bool ExistsLongBoard(string text) => AllLBs.Any(i => i.Name.Contains(text));

        /// <summary>
        /// sends photos from this server to telegram server and then to the user
        /// </summary>
        /// <param name="chatId"></param>
        /// <param name="client"></param>
        /// <param name="neededFiles"></param>
        /// <returns></returns>
        private static async Task<Message[]> LoadAndSendPhotos(long chatId, TelegramBotClient client, IEnumerable<FileInfo> neededFiles)
        {
            var neededFilesArr = neededFiles.Where(i => i.Extension == ImageExtension).ToArray();
            var count = neededFilesArr.Length;
            var photos = new List<InputMediaPhoto>(count); // what if more than 10 photos? TODO!
            var streams = new List<MemoryStream>(count);

            try
            {
                foreach (var i in neededFilesArr)
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

                return await client.SendMediaGroupAsync(chatId, photos); // has to be awaited here! so that streams dont get disposed before photos are sent
            }
            finally
            {
                foreach (var i in streams)
                {
                    i?.Dispose();
                }
            }
        }

        private static async Task<Message[]> SendAllLBExistingPhotos(long chatId, TelegramBotClient client, IEnumerable<FileInfo> neededFiles)
        {
            var neededFilesArr = neededFiles.Where(i => i.Extension == TextExtension).ToArray();
            var photos = new List<InputMediaPhoto>(neededFilesArr.Length);

            foreach (var i in neededFilesArr)
            {
                var token = await ReadAllTextAsync(i.FullName);
                photos.Add(new InputMediaPhoto(token));
            }

            return await client.SendMediaGroupAsync(chatId, photos);
        }

        private static async Task<Message[]> SendLBExistingPhotos(long chatId, TelegramBotClient client, string style)
        {
            var thisLBDirectory = Path.Combine(Directory.GetCurrentDirectory(), LBDirectory, style);

            var target = Path.Combine(thisLBDirectory, PhotosForLBFileName);

            var tokens = await System.IO.File.ReadAllLinesAsync(target);

            var photos = tokens.Select(i => new InputMediaPhoto(i)).ToArray();

            return await client.SendMediaGroupAsync(chatId, photos);
        }
    }
}
