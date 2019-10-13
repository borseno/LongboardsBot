using LongBoardsBot.Models.Entities;
using System.Threading.Tasks;
using Telegram.Bot;
using LongBoardsBot.Helpers;
using Telegram.Bot.Types;
using System;
using static LongBoardsBot.Models.Constants;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
using System.Linq;
using BotLogic.Helpers;

namespace LongBoardsBot.Models.Handlers
{
    public static partial class StatisticsStageHandling
    {
        private const StatisticsStage last = StatisticsStage.Hobby;

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
            var text = message?.Text;

            if (text == null)
                return;

            var stage = botUser.StatisticsStage;
            var isLast = stage == last;
            var isCancelled = text == CancelText;
            var shouldSkip = text == SkipText;

            if (!isCancelled && !shouldSkip)
            {
                var processor = GetAsyncStatisticsMessageProcessor(stage);

                var success = await processor.Invoke(client, message, botUser);

                if (!success)
                {
                    return;
                }
            }

            if (botUser.IsOneTimeStatistics || isLast || isCancelled)
            {
                await ExitStatisticsPoll(client, botUser);
            }
            else
            {
                var nextStage = stage.GetNext();

                await InitStatisticsStageAsync(client, nextStage, botUser);
            }
        }

        private static async Task ExitStatisticsPoll(TelegramBotClient client, BotUser botUser)
        {
            botUser.State = State.Default;
            botUser.StatisticsStage = StatisticsStage.None;

            var msg = await client.SendMenuAsync(botUser);

            botUser.Stage = Stage.ReceivingMenuItem;
            botUser.History.AddMessage(msg, false);
        }
    }

    public static partial class StatisticsStageHandling
    {
        private const string StudyingText = "Учусь";
        private const string WorkingText = "Работаю";

        private static Task<bool> ProcessWorkingOrStudyingAsync(TelegramBotClient client, Message message, BotUser botUser)
        {
            var status =
                message?.Text == StudyingText ? SocialStatus.Studying
                : message?.Text == WorkingText ? SocialStatus.Employeed
                : SocialStatus.Unknown;

            if (status == SocialStatus.Unknown)
            {
                return Task.FromResult(false);
            }
            else
            {
                botUser.StatisticsInfo.SocialStatus = status;
                return Task.FromResult(true);
            }
        }

        private static async Task InitWorkingOrStudyingAsync(TelegramBotClient client, BotUser botUser)
        {
            var workingButton = new KeyboardButton(WorkingText);
            var studyingButton = new KeyboardButton(StudyingText);
            var kboard = DefaultStatisticsKeyboard.Append(workingButton, studyingButton);

            var msg = await client.SendTextMessageAsync(
                    botUser.ChatId,
                    "Вы работаете или учитесь?",
                    replyMarkup: kboard
                    );

            botUser.History.AddMessage(msg, false);
        }

        private static async Task InitAgeStageAsync(TelegramBotClient client, BotUser botUser)
        {
            var msg = await client.AskAge(botUser.ChatId);

            botUser.History.AddMessage(msg, false);
        }

        private static Task<bool> TryProcessAgeAsync(
            TelegramBotClient client, Message message, 
            BotUser botUser)
        {
            var text = message.Text;

            var success = Int32.TryParse(text, out var age);

            if (success)
            {
                botUser.StatisticsInfo.Age = age;

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private static Task<bool> ProcessProfessionAsync(TelegramBotClient client, Message message, BotUser botUser)
        {
            var text = message.Text;

            if (!Regex.IsMatch(text, ProfessionRegexp))
            {
                return Task.FromResult(false);
            }

            botUser.StatisticsInfo.Profession = text;

            return Task.FromResult(true);
        }
        private static async Task InitHobbyAsync(TelegramBotClient client, BotUser botUser)
        {
            var msg = await client
                .SendTextMessageAsync(
                botUser.ChatId, 
                $"Напишите пожалуйста о своем хобби (макс: {MaxHobbySymbols} символов)",
                replyMarkup: DefaultStatisticsKeyboard);

            botUser.History.AddMessage(msg, false);
        }
        private static async Task<bool> ProcessHobbyAsync(TelegramBotClient client, Message message, BotUser botUser)
        {
            var text = message.Text;

            if (text.Length > MaxHobbySymbols)
            {
                var msg = await client.SendTextMessageAsync(botUser.ChatId, 
                    $"Макс колво символов: {MaxHobbySymbols}. Было введено: {text.Length}");

                botUser.History.AddMessage(msg, false);
            }

            botUser.StatisticsInfo.Hobby = text;

            return true;
        }

        private static async Task InitProfessionAsync(TelegramBotClient client, BotUser botUser)
        {
            var msg = await client.SendTextMessageAsync(
                botUser.ChatId,
                "Напишите, пожалуйста, вашу профессию",
                replyMarkup: DefaultStatisticsKeyboard);

            botUser.History.AddMessage(msg, false);
        }
    }

    public static partial class StatisticsStageHandling
    {
        private static AsyncStatisticsMessageProcessor GetAsyncStatisticsMessageProcessor(StatisticsStage stage)
        {
            switch (stage)
            {
                case StatisticsStage.Age:
                    return TryProcessAgeAsync;
                case StatisticsStage.WorkingOrStudying:
                    return ProcessWorkingOrStudyingAsync;
                case StatisticsStage.Profession:
                    return ProcessProfessionAsync;
                case StatisticsStage.Hobby:
                    return ProcessHobbyAsync;
                default:
                    throw new NotSupportedException(stage.ToString() + " is not supported");
            }
        }


        private static AsyncStatisticsStageInitializer GetAsyncStatisticsStageInitializer(StatisticsStage stage)
        {
            switch (stage)
            {
                case StatisticsStage.Age:
                    return InitAgeStageAsync;
                case StatisticsStage.WorkingOrStudying:
                    return InitWorkingOrStudyingAsync;
                case StatisticsStage.Profession:
                    return InitProfessionAsync;
                case StatisticsStage.Hobby:
                    return InitHobbyAsync;
                default:
                    throw new NotSupportedException(stage.ToString() + " is not supported");
            }
        }

    }
}
