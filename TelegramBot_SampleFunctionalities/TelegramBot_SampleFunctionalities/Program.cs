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

namespace TelegramBot_SampleFunctionalities
{

    public static class Program
    {
        static Stage stage = Stage.SendingLongboards;

        static void Main()
        {
            var Bot = new TelegramBotClient("821836757:AAHFbFgSrbrvpGVpzCYWZAwG2Jzo7Cbl1m8");
            Bot.StartReceiving();
            Bot.OnUpdate += async (obj, args) =>
            {
                var chatId = args?.Update?.Message?.Chat?.Id;

                if (chatId == null)
                    return;

                try
                {
                    await HandleUpdate(Bot, args.Update, stage, null);
                    

                }
                catch (Exception e)
                {
                    await Bot.SendTextMessageAsync(chatId, e.Message + "\n" + e.StackTrace);
                }
            };

            Console.ReadKey();
            Bot.StopReceiving();
        }
    }
}
