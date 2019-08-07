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
using static LongBoardsBot.Models.TextsFunctions.Texts;
using static LongBoardsBot.Models.TextsFunctions.FormattedTexts;
using Telegram.Bot.Exceptions;

namespace LongBoardsBot.Models.Handlers
{
    public class CallbackHandler
    {
        private readonly LongboardistDBContext ctx;

        public CallbackHandler(LongboardistDBContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task HandleCallback(TelegramBotClient client, CallbackQuery query)
        {
            var includedPurchasesQuery = ctx.Purchases
                    .Include(i => i.Basket)
                    .ThenInclude(i => i.Longboard)
                    .Include(i => i.BotUser)
                    .ThenInclude(i => i.CurrentPurchase);
            var includedCommentsQuery = ctx.Comments
                    .Include(i => i.Author);

            if (query.Data.StartsWith(DeliveredData)) // delivered a longboard...
            {
                var purchaseId = query.Data.Substring(DeliveredData.Length);
                var purchase = await includedPurchasesQuery.FirstAsync(i => i.Guid.ToString() == purchaseId);
                var user = purchase.BotUser;

                var wantsToSendAReviewOrNotKBoard = new ReplyKeyboardMarkup(
                    new[] {
                        new KeyboardButton(WantsAddComment),
                        new KeyboardButton(NotWantsAddComment)
                    }, true, true);

                var textToAdminGroupTask = GetFormattedFinalTextToAdminsAsync(user, purchase);
                var deliveryNotificationTextTask = GetDeliveryNotification();

                await Task.WhenAll(textToAdminGroupTask, deliveryNotificationTextTask);

                var textToAdminGroup = textToAdminGroupTask.Result;
                var deliveryNotificationText = deliveryNotificationTextTask.Result;

                var msgTask = client.SendTextMessageAsync(
                    user.ChatId,
                    Format(deliveryNotificationText, purchase.Guid.ToStringHashTag()),
                    replyMarkup: wantsToSendAReviewOrNotKBoard);

                var answerTask = client.AnswerCallbackQueryAsync(query.Id, SuccessfullySent);

                var editTask = client.EditMessageTextAsync(
                    query.Message.Chat.Id,
                    query.Message.MessageId,
                    textToAdminGroup + Environment.NewLine + SoldMessage,
                    Telegram.Bot.Types.Enums.ParseMode.Markdown
                    );

                await Task.WhenAll(msgTask, answerTask, editTask);

                user.Stage = Entities.Stage.ProcessingWantsToComment;
                await ctx.SaveChangesAsync();

                return;
            }
            else if (query.Data.StartsWith(CancelDeliveryData))
            {
                var purchaseId = query.Data.Substring(CancelDeliveryData.Length);
                var purchase = await includedPurchasesQuery.FirstAsync(i => i.Guid.ToString() == purchaseId);
                var chat = purchase.BotUser.ChatId; // nullreference

                var cancelledText = await GetCancelledOrderingNotificationText();

                var msgTask = client.SendTextMessageAsync(chat, Format(cancelledText, purchase.Guid));
                var answerTask = client.AnswerCallbackQueryAsync(query.Id, SuccessfullySent);
                var deleteTask = client.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);

                await Task.WhenAll(msgTask, answerTask, deleteTask);

                return;
            }
            else if (query.IsCommentsQuery())
            {
                try
                {
                    var comments = includedCommentsQuery.Select(i => i.Data).ToArray();

                    await HandleCommentsQueryAsync(client, query, comments);
                }
                catch (Exception exception)
                {
                    if (exception is MessageIsNotModifiedException)
                        return;

                    if (exception is ApiRequestException)
                        return;

                    if (exception is WrongPageException)
                    {
                        await client.AnswerCallbackQueryAsync(callbackQueryId: query.Id, exception.Message);
                        return;
                    }

                    throw;
                }
            }
            else
            {
                throw new NotSupportedException("Другие Callback data не обрабатываются, callback data был: " + query.Data);
            }
        }

        private Task HandleCommentsQueryAsync(TelegramBotClient bot, CallbackQuery query, string[] comments)
        {
            var isFinish = query.Data == FinishComment;

            if (isFinish)
            {
                return bot.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);
            }

            var isPrev = query.Data.StartsWith(PreviousComment);

            Int32.TryParse(isPrev ? query.Data.Substring(PreviousComment.Length) : query.Data.Substring(PreviousComment.Length), out var index);

            if (isPrev)
            {
                if (index == 0)
                {
                    throw new WrongPageException("Already on the first page");
                }
                else
                {
                    index--;
                }
            }
            else
            {
                if (index == comments.Length - 1 || comments.Length == 0)
                {
                    throw new WrongPageException("Already on the last page");
                }
                else
                {
                    index++;
                }
            }

            return bot.SendOrEditCommentsView(query.Message.Chat.Id, comments, index, query.Message.MessageId);
        }

        private class WrongPageException : Exception
        {
            public WrongPageException(string message) : base(message)
            {

            }
        }
    }
}
