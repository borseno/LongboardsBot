using LongBoardsBot.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Helpers
{
    public static class TelegramExtensions
    {
        public static Task<Message> AskName(this TelegramBotClient client, long chatId)
    => client.SendTextMessageAsync(chatId, EnterYourNameText);

        public static Task<Message> AskPhone(this TelegramBotClient client, long chatId)
            => client.SendTextMessageAsync(chatId, EnterYourPhoneText);

        public static bool IsCommentsQuery(this CallbackQuery query)
            => new[] { NextComment, FinishComment, PreviousComment }.Any(i => query.Data.StartsWith(i));

        public static Task<Message> SendOrEditCommentsView(
            this TelegramBotClient client, long chatId, 
            string[] comments, int currentIndex = default, int messageId = default)
        {
            if (comments == null || comments.Length == 0)
            {
                return client.SendTextMessageAsync(chatId, text: "Ни одного комментария пока что нет");
            }

            var isFirstMsg = messageId == default;

            var inlineBtnPrev = new InlineKeyboardButton
            {
                CallbackData = $"{PreviousComment}{currentIndex}",
                Text = $"{PreviousComment}"
            };
            var inlineBtnFinish = new InlineKeyboardButton
            {
                CallbackData = $"{FinishComment}",
                Text = $"{FinishComment}"
            };
            var inlineBtnNext = new InlineKeyboardButton
            {
                CallbackData = $"{NextComment}{currentIndex}",
                Text = $"{NextComment}"
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

        public static Task<Message> SendMenuAsync(
            this TelegramBotClient client, BotUser instance
            )
        {
            var inKharkiv = instance.IsLivingInKharkiv;
            var hasVisitedTesting = instance?.TestingInfo?.Occurred ?? false;

            var buyLboardsButton = new KeyboardButton(StartPurchasingText);
            var testLboardsButton = new KeyboardButton(StartTestingText);

            var buttons = new List<KeyboardButton>(2)
            {
                buyLboardsButton
            }; 

            if (!hasVisitedTesting && inKharkiv)
            {
                buttons.Add(testLboardsButton);
            }

            var replyMarkup = new ReplyKeyboardMarkup(buttons, true, true);

            return client.SendTextMessageAsync(instance.ChatId, MenuText, replyMarkup: replyMarkup);
        }

        public static Task<Message> AskToTypeStatisticsAsync(
            this TelegramBotClient client, long chatId
            )
        {
            var kboard = new ReplyKeyboardMarkup(
            new[] {
                new KeyboardButton(YesText),
                new KeyboardButton(NoText)
                }, true, true);

            return client.SendTextMessageAsync(chatId, WantsToTypeStatisticsText, replyMarkup: kboard);
        }

        public static Task<Message>[] AskDateOfVisit(this TelegramBotClient client, long chatId)
        {
            var kboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton(CancelText) }, true, true);
            var text = String.Format(AskDateOfVisitText, DateTimeFormat);

            var pinTask = client.SendLocationAsync(chatId, 50.035813f, 36.2205788f);
            var textTask = client.SendTextMessageAsync(chatId, text, replyMarkup: kboard);

            return new[] { pinTask, textTask };
        }
    }
}
