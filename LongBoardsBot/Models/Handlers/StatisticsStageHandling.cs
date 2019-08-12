using LongBoardsBot.Models.Entities;
using System.Threading.Tasks;
using Telegram.Bot;
using LongBoardsBot.Helpers;
using Telegram.Bot.Types;
using System;

namespace LongBoardsBot.Models.Handlers
{
    public static partial class StatisticsStageHandling
    {
        private const StatisticsStage last = StatisticsStage.Age;

        // indicates whether a move to a next state should be done or not
        private delegate Task<bool> AsyncStatisticsMessageProcessor(TelegramBotClient client, Message message, BotUser botUser);
        private delegate Task AsyncStatisticsStageInitializer(TelegramBotClient client, BotUser botUser);

        public static async Task InitStatisticsStageAsync(TelegramBotClient client, StatisticsStage statisticsStage, BotUser botUser)
        {
            var initializer = GetAsyncStatisticsStageInitializer(statisticsStage);

            await initializer.Invoke(client, botUser);

            botUser.StatisticsStage = statisticsStage;
        }

        public static async Task ProcessStatisticsMessageAsync(TelegramBotClient client, Message message, BotUser botUser)
        {
            var stage = botUser.StatisticsStage;
            var isLast = stage == last;

            var processor = GetAsyncStatisticsMessageProcessor(stage);

            await processor.Invoke(client, message, botUser);

            if (botUser.IsOneTimeStatistics || isLast)
            {
                botUser.State = State.Default;
                botUser.StatisticsStage = StatisticsStage.None;

                await client.SendMenuAsync(botUser);
                botUser.Stage = Stage.ReceivingMenuItem;
            }
            else
            {
                var nextStage = stage.GetNext();
                var nextStageInitializer = GetAsyncStatisticsStageInitializer(nextStage);
                await nextStageInitializer.Invoke(client, botUser);
            }
        }

        private static Task<bool> ProcessWorkingOrStudyingAsync(TelegramBotClient client, Message message, BotUser botUser)
        {
            throw new NotImplementedException();
        }
    }

    public static partial class StatisticsStageHandling
    {
        private static async Task InitAgeStageAsync(TelegramBotClient client, BotUser botUser)
        {
            var msg = await client.AskAge(botUser.ChatId);

            botUser.History.AddMessage(msg, false);
        }

        private static async Task<bool> TryProcessAgeAsync(
            TelegramBotClient client, Message message, 
            BotUser botUser)
        {
            var text = message.Text;

            if (text == null)
                return false;

            var success = Int32.TryParse(text, out var age);

            if (success)
            {
                botUser.StatisticsInfo.Age = age;

                return true;
            }

            return false;
        }
    }

    public static partial class StatisticsStageHandling
    {
        private static AsyncStatisticsMessageProcessor GetAsyncStatisticsMessageProcessor(StatisticsStage stage)
        {
            if (stage == StatisticsStage.Age)
            {
                return TryProcessAgeAsync;
            }
            if (stage == StatisticsStage.WorkingOrStudying)
            {
                return ProcessWorkingOrStudyingAsync;
            }

            throw new NotSupportedException(stage.ToString() + " is not supported");
        }

        private static AsyncStatisticsStageInitializer GetAsyncStatisticsStageInitializer(StatisticsStage stage)
        {
            if (stage == StatisticsStage.Age)
            {
                return InitAgeStageAsync;
            }

            throw new NotSupportedException(stage.ToString() + " is not supported");
        }
    }
}
