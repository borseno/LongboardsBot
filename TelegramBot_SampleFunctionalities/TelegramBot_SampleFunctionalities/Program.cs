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
                if (args.Update.Message.Chat.Id == AdminGroupChatId)
                    return;

                await HandleUpdate(Bot, args.Update);
            };

            Console.ReadKey();
            Bot.StopReceiving();
        }
    }
}
