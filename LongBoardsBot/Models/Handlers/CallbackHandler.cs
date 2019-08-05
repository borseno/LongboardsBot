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
using static System.String;
using static LongBoardsBot.Models.Texts;

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

            if (query.Data.StartsWith(DeliveredData)) // delivered a longboard...
            {
                var purchaseId = query.Data.Substring(DeliveredData.Length);
                var purchase = await includedQuery.FirstAsync(i => i.Guid.ToString() == purchaseId);
                var user = purchase.Basket.First().BotUser;

                var wantsToSendAReviewOrNotKBoard = new ReplyKeyboardMarkup(
                    new[] {
                        new KeyboardButton(WantsAddComment),
                        new KeyboardButton(NotWantsAddComment)
                    }, true, true);

                var deliveryNotificationText = await GetDeliveryNotification();

                var msgTask = client.SendTextMessageAsync(
                    user.ChatId,
                    Format(deliveryNotificationText, purchase.Guid.ToStringHashTag()),
                    replyMarkup: wantsToSendAReviewOrNotKBoard);

                var answerTask = client.AnswerCallbackQueryAsync(query.Id, SuccessfullySent);

                var editTask = client.EditMessageTextAsync(
                    query.Message.Chat.Id, 
                    query.Message.MessageId, 
                    query.Message.Text + Environment.NewLine + SoldMessage,
                    Telegram.Bot.Types.Enums.ParseMode.Markdown
                    );

                await Task.WhenAll(msgTask, answerTask, editTask);

                user.Stage = Entities.Stage.ProcessingShouldReview;
                await ctx.SaveChangesAsync();

                return msgTask.Result;
            }
            else if (query.Data.StartsWith(CancelDeliveryData))
            {
                var purchaseId = query.Data.Substring(CancelDeliveryData.Length);
                var purchase = await includedQuery.FirstAsync(i => i.Guid.ToString() == purchaseId);
                var chat = purchase.Basket.First().BotUser.ChatId; // nullreference

                var cancelledText = await GetCancelledOrderingNotificationText();

                var msgTask = client.SendTextMessageAsync(chat, Format(cancelledText, purchase.Guid));
                var answerTask = client.AnswerCallbackQueryAsync(query.Id, SuccessfullySent);
                var deleteTask = client.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

                await Task.WhenAll(msgTask, answerTask, deleteTask);

                return msgTask.Result;
            }
            else
            {
                throw new NotSupportedException("Другие Callback data не обрабатываются, callback data был: " + query.Data);
            }
        }
    }
}
