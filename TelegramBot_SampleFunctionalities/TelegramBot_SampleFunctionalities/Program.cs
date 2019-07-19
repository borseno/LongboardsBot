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

                var t3 = Bot.SendPhotoAsync(chatId.Value, "AgADBQADdKkxGxIQiFW-rY6BI6W6AVVb9jIABGO4FGTWH-JGadAFAAEC");

                if (args.Update.Message.Text == "44")
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

                if (args.Update.Message.Chat.Id == AdminGroupChatId)
                    return;

                await HandleUpdate(Bot, args.Update);
            };

            Console.ReadKey();
            Bot.StopReceiving();
        }
    }
}
