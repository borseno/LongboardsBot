using LongBoardsBot.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Models.Handlers
{
    public class CallbackHandler
    {
        private readonly LongboardistDBContext ctx;

        public CallbackHandler(LongboardistDBContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<Message> HandleCallback(TelegramBotClient client, CallbackQuery query)
        {
            var includedQuery = ctx.Purchases
                    .Include(i => i.Basket)
                    .ThenInclude(i => i.BotUser);

            if (query.Data.StartsWith(DeliveredData))
            {
                // delivered a longboard...

                var purchaseId = query.Data.Substring(DeliveredData.Length);

                var purchase = await includedQuery
                    .FirstAsync(i => i.Guid.ToString() == purchaseId);

                var user = purchase.Basket.First().BotUser;

                var wantsToSendAReviewOrNotKBoard = new ReplyKeyboardMarkup(
                    new[] {
                        new KeyboardButton("Хорошо"),
                        new KeyboardButton("Лень)")
                    });

                var msgTask = client
                    .SendTextMessageAsync(
                    user.ChatId, 
                    $"Ваш заказ {purchase.Guid.ToStringHashTag()} был доставлен! Спасибо за покупку. Оставьте отзыв, если не сложно", 
                    replyMarkup: wantsToSendAReviewOrNotKBoard);

                var answerTask = client.AnswerCallbackQueryAsync(query.Id, "Уведомление было успешно отправлено пользователю");

                await Task.WhenAll(msgTask, answerTask);

                user.Stage = Entities.Stage.ProcessingShouldReview;

                await ctx.SaveChangesAsync();

                return msgTask.Result;
            }
            else if (query.Data.StartsWith(CancelDeliveryData))
            {
                var purchaseId = query.Data.Substring(CancelDeliveryData.Length);

                var purchase = await includedQuery
                    .FirstAsync(i => i.Guid.ToString() == purchaseId);

                var chat = purchase.Basket.First().BotUser.ChatId;

                return await client.SendTextMessageAsync(chat, $"Ваш заказ {purchase.Guid.ToStringHashTag()} был отменен.");
            }

            throw new NotSupportedException("Другие Callback data не обрабатываются, callback data был: " + query.Data);
        }
    }
}
