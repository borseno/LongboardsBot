﻿using LongBoardsBot.Models.Entities;
using System.Threading.Tasks;
using Telegram.Bot;
using LongBoardsBot.Helpers;
using Telegram.Bot.Types;
using System;
using static LongBoardsBot.Models.Constants;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;

namespace LongBoardsBot.Models.Handlers
{
    public static partial class StatisticsStageHandling
    {
        private const StatisticsStage last = StatisticsStage.Profession;

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
            var isCancelled = message?.Text == CancelText;

            if (!isCancelled)
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

            await client.SendMenuAsync(botUser);
            botUser.Stage = Stage.ReceivingMenuItem;
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
            var msg = await client.SendTextMessageAsync(
                    botUser.ChatId,
                    "Вы работаете или учитесь?",
                    replyMarkup: new ReplyKeyboardMarkup(
                        new[]
                        {
                            new KeyboardButton(StudyingText),
                            new KeyboardButton(WorkingText),
                            new KeyboardButton(CancelText)
                        }, true, true)
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

            if (text == null)
                return Task.FromResult(false);

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
            var text = message?.Text;

            if (text == null)
            {
                return Task.FromResult(false);
            }

            if (!Regex.IsMatch(text, ProfessionRegexp))
            {
                return Task.FromResult(false);
            }

            botUser.StatisticsInfo.Profession = text;

            return Task.FromResult(true);
        }

        private static async Task InitProfessionAsync(TelegramBotClient client, BotUser botUser)
        {
            var msg = await client.SendTextMessageAsync(
                botUser.ChatId,
                "Напишите, пожалуйста, вашу профессию",
                replyMarkup: CancelKeyboard);

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
                default:
                    throw new NotSupportedException(stage.ToString() + " is not supported");
            }
        }
    }
}
