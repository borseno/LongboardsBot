using System;
using Telegram.Bot;

namespace TestingMiscalleneousFunctionalitiesOfTelegramBotApi
{
    static partial class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("921368344:AAG9DoV42gfdGJkaxwxCPKuw8Cw5CIYhju8");

        static void Main(string[] args)
        {
            TestMarkDownParseMode();

            Console.ReadLine();
        }
    }

    static partial class Program
    {
        static void TestMarkDownParseMode()
        {
            Bot.StartReceiving();

            Bot.OnMessage += (_, a) =>
            {
                Bot.SendTextMessageAsync(a.Message.Chat.Id, $"[{a.Message.From.FirstName + a.Message.From.LastName}](tg://user?id={a.Message.From.Id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
            };
        }
    }
}
