using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using static TelegramBot_SampleFunctionalities.Functions;
using static TelegramBot_SampleFunctionalities.StageHandling;
using static TelegramBot_SampleFunctionalities.Constants;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramBot_SampleFunctionalities
{

    public static class Program
    {
        static void Main()
        {
            var Bot = new TelegramBotClient(ApiToken);
            Bot.StartReceiving();
            Bot.OnUpdate += async (obj, args) =>
            {
                var chatId = args?.Update?.Message?.Chat?.Id;

                if (chatId == null)
                    return;

                if (args.Update.Message.Text == "Id")
                {
                    await Bot.SendTextMessageAsync(args.Update.Message.Chat.Id, args.Update.Message.Chat.Id.ToString());
                }
                if (args.Update.Message.Sticker != null)
                {
                    await Bot.SendTextMessageAsync(args.Update.Message.Chat.Id, args.Update.Message.Sticker.FileId);
                }
                if (args.Update.Message.Photo != null)
                {
                    var id = args.Update.Message.Photo.First().FileId;

                    var t1 = Bot.SendPhotoAsync(chatId.Value, id);
                    var t2 = Bot.SendTextMessageAsync(chatId.Value, id);

                    await Task.WhenAll(t1, t2);
                }
                else
                {
                    var photos = new InputMediaPhoto[]
                    {
                        new InputMediaPhoto( "AgADBQADB6kxG-xlmVWsl9ir-VKfm5y3-TIABJlYPORxEcvGroYAAgI" ),
                        new InputMediaPhoto( "AgADBQADzKgxG3tImFX6wWIPlEeI24tz-TIABJDfvPsKF95DLiMBAAEC" ),
                        new InputMediaPhoto( "AgADBQADy6gxG3tImFVnNQXF4jYjEwfy3zIABA-1lPMy85aKqeEFAAEC" )
                    };

                    var t3 = await Bot.SendMediaGroupAsync(chatId, photos);
                }

                if (args.Update.Message.Chat.Id == AdminGroupChatId)
                    return;

                //await HandleUpdate(Bot, args.Update);
            };

            Console.ReadKey();
            Bot.StopReceiving();
        }
    }
}
