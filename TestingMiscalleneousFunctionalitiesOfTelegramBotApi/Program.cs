using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TestingMiscalleneousFunctionalitiesOfTelegramBotApi
{
    static partial class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("921368344:AAG9DoV42gfdGJkaxwxCPKuw8Cw5CIYhju8");

        static async Task Main(string[] args)
        {
            bool hasStarted = false;

            start:

            try
            {

                if (!hasStarted)
                {
                    TestMaxSymbolsInAMessage();
                }

                hasStarted = true;

                Console.ReadLine();
            }
            catch (Exception)
            {
                goto start;
            }
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

        static void TestMaxSymbolsInAMessage()
        {
            Bot.StartReceiving();

            Bot.OnMessage += async (_, a) =>
            {
                var chatId = a.Message.Chat.Id;

                var text = "a";
                var start = 1048576;
                var finish = start * 4;

                var arr = 1048576..1048576 * 4;

                try
                {
                    for (; ; count *= 4)
                    {
                        text = Repeat(text, 4);

                        await Bot.SendTextMessageAsync(chatId, text);
                        await Task.Delay(1000);

                        Console.WriteLine(count);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(count);
                }
            };
        }

        private static string Repeat(string text, int count)
        {
            var builder = new StringBuilder(text.Length * count);

            for (int i = 0; i < count; i++)
                builder.Append(text);

            return builder.ToString();
        }
    }
}
