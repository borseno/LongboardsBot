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

namespace TelegramBot_SampleFunctionalities
{
    public enum Stage
    {
        GettingName = 0,
        GettingPhone = 1,
        SendingLongboards = 2,
        GettingInfoAboutLongBoards = 3,
        ShouldContinueGettingsInfoAboutLongBoards = 4,
        ShouldFinishChoosing = 5
    }
    public static class StageHandling
    {
        public static async Task<IEnumerable<BotUser>> HandleUpdate(TelegramBotClient client, Update update, Stage stage, IEnumerable<BotUser> storage)
        {
            if (stage == Stage.GettingName)
            {
                if (update.Message == null)
                    return storage;

                return UpdateUsersNameAndUsername(client, update.Message, storage);
            }
            else if (stage == Stage.GettingPhone)
            {
                if (update.Message == null)
                    return storage;

                return UpdateUsersPhoneAndUsername(client, update.Message, storage);
            }
            else if (stage == Stage.SendingLongboards)
            {
                await SendLongBoards(client, update.Message.Chat.Id);
                
                return storage;
            }

            return storage;
        }
    }

    public static class Functions
    {
        public static async Task SendLongBoards(TelegramBotClient client, long chatId)
        {
            var directory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), LBDirectory));
            var files = directory.GetFiles();
            var images = new List<KeyValuePair<InputMediaPhoto, MemoryStream>>(files.Length);
            var buttons = new KeyboardButton[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                var nameWithoutExt = files[i].Name.Remove(files[i].Name.Length - files[i].Extension.Length, files[i].Extension.Length);
                var bitmap = new Bitmap(files[i].FullName);
                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;

                var photo = new InputMediaPhoto( new InputMedia(stream, nameWithoutExt ) );

                images.Add(new KeyValuePair<InputMediaPhoto, MemoryStream>(photo, stream));
                buttons[i] = new KeyboardButton(nameWithoutExt);
            }

            var photos = images.Select(i => i.Key).ToArray(); // what if more than 10 photos? TODO!

            await client.SendMediaGroupAsync(chatId, photos);

            foreach (var i in images)
            {
                i.Value.Dispose();
            }

            var myReplyMarkup = new ReplyKeyboardMarkup(buttons, true);

            await client.SendTextMessageAsync(chatId, "Choose longboard!", replyMarkup: myReplyMarkup);
        }

        public static IEnumerable<BotUser> UpdateUsersPhoneAndUsername(TelegramBotClient client, Message message, IEnumerable<BotUser> storage)
        {
            var id = message.Chat.Id;
            var phone = message.Text;
            var username = message.Chat.Username;
            var entry = storage.First(i => i.Id == id);

            entry.UserName = username;
            entry.Phone = phone;

            client.SendTextMessageAsync(id, $"Вы успешно поменяли свой номер телефона для обратной связи на {phone}");

            return storage;
        }

        public static IEnumerable<BotUser> UpdateUsersNameAndUsername(TelegramBotClient client, Message message, IEnumerable<BotUser> storage)
        {
            var result = new List<BotUser>();

            if (storage != null)
            {
                result.AddRange(storage);
            }

            var id = message.Chat.Id;
            var name = message.Text;
            string phone = null;
            var username = message.Chat.Username;

            if (result.Any( a => a.Id == id ))
            {
                var first = result.First(i => i.Id == id);
                phone = first.Phone;
                result.Remove(first);
            }

            result.Add(
                new BotUser
                {
                    Id = id,
                    Name = name,
                    Phone = phone,
                    UserName = username
                });

            client.SendTextMessageAsync(id, $"Здравствуйте, {name}");

            return result;
        }
    }
}
