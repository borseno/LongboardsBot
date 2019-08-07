using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TestingMiscalleneousFunctionalitiesOfTelegramBotApi
{
    static partial class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("921368344:AAG9DoV42gfdGJkaxwxCPKuw8Cw5CIYhju8");

        static async Task Main(string[] args)
        {
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;
            Bot.OnMessage += Bot_OnMessage;

            Bot.StartReceiving();

            Console.ReadLine();

            Bot.StopReceiving();
        }

        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;

            Bot.TryListWithComments(chatId).GetAwaiter().GetResult();
        }

        private static void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                Bot.HandleCallBackQuery(e.CallbackQuery).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                if (exception is MessageIsNotModifiedException)
                    return;

                if (exception is ApiRequestException)
                    return;

                if (exception is WrongPageException)
                {
                    Bot.AnswerCallbackQueryAsync(callbackQueryId: e.CallbackQuery.Id, exception.Message).GetAwaiter().GetResult();
                    return;
                }

                throw;
            }
        }
    }

    class WrongPageException : Exception
    {
        public WrongPageException(string message) : base(message)
        {

        }
    }

    static partial class Program
    {
        const string next = "NEXT";
        const string prev = "PREV";
        const string finish = "FINISH";

        static string[] comments = new[]
            {
                Repeat("aaaaaaaaaaaa", 10),
                Repeat("bbbbbbbbbbbb", 13),
                "cccccccccccccccccccccc".Repeat(15),
                "dddddddddddddddddddddd".Repeat(20),
                "vvvvvvvvvvv".Repeat(5)
            };

        static void TryMarkDownParseMode()
        {
            Bot.StartReceiving();

            Bot.OnMessage += (_, a) =>
            {
                Bot.SendTextMessageAsync(a.Message.Chat.Id, $"[{a.Message.From.FirstName + a.Message.From.LastName}](tg://user?id={a.Message.From.Id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
            };
        }

        static Task<Message> TryListWithComments(this TelegramBotClient client, long chatId, int currentIndex = default, int messageId = default)
        {
            var isFirstMsg = messageId == default;

            var inlineBtnPrev = new InlineKeyboardButton
            {
                CallbackData = $"{prev}{currentIndex}",
                Text = $"{prev}"
            };
            var inlineBtnFinish = new InlineKeyboardButton
            {
                CallbackData = $"{finish}",
                Text = $"{finish}"
            };
            var inlineBtnNext = new InlineKeyboardButton
            {
                CallbackData = $"{next}{currentIndex}",
                Text = $"{next}"
            };

            var keyboard = new InlineKeyboardMarkup(new[] { inlineBtnPrev, inlineBtnFinish, inlineBtnNext });

            if (isFirstMsg)
            {
                return client.SendTextMessageAsync(chatId, comments[currentIndex], replyMarkup: keyboard);
            }
            else
            {
                return client.EditMessageTextAsync(chatId, messageId, comments[currentIndex], replyMarkup: keyboard);
            }
        }

        static Task HandleCallBackQuery(this TelegramBotClient bot, CallbackQuery query)
        {
            var isFinish = query.Data == finish;

            if (isFinish)
            {
                return bot.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);
            }

            var isPrev = query.Data.StartsWith(prev);
            var index = (isPrev ? query.Data.Substring(prev.Length) : query.Data.Substring(next.Length)).ParseInt();

            if (isPrev)
            {
                if (index != 0)
                {
                    index--;
                }
                else
                {
                    throw new WrongPageException("Already on the first page");
                }
            }
            else
            {
                if (index != comments.GetUpperBound(0))
                {
                    index++;
                }
                else
                {
                    throw new WrongPageException("Already on the last page");
                }
            }

            return TryListWithComments(bot, query.Message.Chat.Id, index, query.Message.MessageId);
        }

        static void TryMaxSymbolsInAMessage()
        {
            Bot.StartReceiving();

            Bot.OnMessage += async (_, a) =>
            {
                var chatId = a.Message.Chat.Id;

                var text = "a";
                var start = 1048576;
                var finish = start * 4;
                var count = 1;
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

        private static string Repeat(this string text, int count)
        {
            var builder = new StringBuilder(text.Length * count);

            for (int i = 0; i < count; i++)
                builder.Append(text);

            return builder.ToString();
        }
    }

    public static class MiscalleneousExtensions
    {
        public static int ParseInt(this string str) => Int32.Parse(str);
    }
}
