using LongBoardsBot.Controllers;
using LongBoardsBot.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Models
{
    internal static class Bot
    {
        private static TelegramBotClient client;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static Object locker = new Object();

        public static TelegramBotClient Get()
        {
            void InitClient()
            {
                client = new TelegramBotClient(ApiKey);

                var url = Url.AddControllerName(new MessageController());

                client.SetWebhookAsync(url).GetAwaiter().GetResult();
            }

            if (client != null)
            {
                return client;
            }

            lock (locker)
            {
                if (client == null)
                {
                    InitClient();
                }
            }

            return client;
        }

        public static async Task<TelegramBotClient> GetAsync()
        {
            Task InitClientAsync()
            {
                client = new TelegramBotClient(ApiKey);

                var url = Url.AddControllerName(new MessageController());

                return client.SetWebhookAsync(url);
            }

            if (client != null)
            {
                return client;
            }

            await semaphore.WaitAsync();
            try
            {
                if (client == null)
                {
                    await InitClientAsync();
                }
            }
            finally
            {
                semaphore.Release();
            }

            return client;
        }
    }
}
