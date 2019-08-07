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
    }
}
