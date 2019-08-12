using LongBoardsBot.Models.Entities;
using System.Threading.Tasks;
using Telegram.Bot;
using LongBoardsBot.Helpers;
using Telegram.Bot.Types;
using System;

namespace LongBoardsBot.Models.Handlers
{
    public static class StatisticsStageHandling
    {
        private const StatisticsStage last = StatisticsStage.Age; 

        public static async Task InitStatisticsStageAsync(this TelegramBotClient client, StatisticsStage statisticsStage, BotUser botUser)
        {
            if (statisticsStage == StatisticsStage.Age)
            {
                var msg = await client.AskAge(botUser.ChatId);

                botUser.History.AddMessage(msg, false);
            }

            botUser.StatisticsStage = statisticsStage;
        }

        public static async Task ProcessStatisticsMessageAsync(this TelegramBotClient client, Message message, BotUser botUser)
        {
            var stage = botUser.StatisticsStage;
            var isLast = stage == last;

            if (stage == StatisticsStage.Age)
            {
                await ProcessAgeAsync(client, message, botUser, isLast);
            }
            if (stage == StatisticsStage.WorkingOrStudying)
            {
                await ProcessWorkingOrStudyingAsync(client, message, botUser, isLast);
            }
        }

        private static Task ProcessWorkingOrStudyingAsync(TelegramBotClient client, Message message, BotUser botUser, bool isLast)
        {
            throw new NotImplementedException();
        }

        private static async Task ProcessAgeAsync(TelegramBotClient client, Message message, BotUser botUser, bool isLast)
        {
            var text = message.Text;

            if (text == null)
                return;

            var success = Int32.TryParse(text, out var age);

            if (success)
            {
                botUser.StatisticsInfo.Age = age;

                if (botUser.IsOneTimeStatistics || isLast)
                {
                    botUser.State = State.Default;

                    await client.SendMenuAsync(botUser);
                }
                else
                {
                    await client.InitStatisticsStageAsync(StatisticsStage.WorkingOrStudying, botUser);
                }
            }
        }
    }
}
