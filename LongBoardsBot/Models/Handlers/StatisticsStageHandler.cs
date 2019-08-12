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
        public static async Task InitStatisticsStageAsync(this TelegramBotClient client, StatisticsStage statisticsStage, BotUser botUser)
        {
            if (statisticsStage == StatisticsStage.Age)
            {
                var msg = await client.AskAge(botUser.ChatId);

                botUser.History.AddMessage(msg, false);
            }

            botUser.StatisticsStage = statisticsStage;
        }

        public static void ProcessStatisticsMessage(this TelegramBotClient client, Message message, BotUser botUser)
        {
            var stage = botUser.StatisticsStage;

            if (stage == StatisticsStage.Age)
            {
                var text = message.Text;

                if (text == null)
                    return;

                var success = Int32.TryParse(text, out var age);

                if (success)
                {
                    botUser.StatisticsInfo.Age = age;

                    if (!botUser.IsOneTimeStatistics)
                    {
                        // logic of moving to the next stage
                        // by calling Other InitStage
                    }
                    else
                    {
                        botUser.State = State.Default;    
                    }
                }
            }
        }
    }
}
