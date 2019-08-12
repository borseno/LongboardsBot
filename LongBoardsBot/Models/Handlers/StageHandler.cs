using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using static LongBoardsBot.Models.Functions;
using static LongBoardsBot.Models.Constants;
using static System.String;
using System.Text.RegularExpressions;
using LongBoardsBot.Helpers;
using Microsoft.EntityFrameworkCore;
using LongBoardsBot.Models.Entities;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.Types.Enums.ParseMode;
using static LongBoardsBot.Models.TextsFunctions.FormattedTexts;
using LongBoardsBot.Models.TextsFunctions;
using Telegram.Bot.Types.Enums;
using System.Globalization;

namespace LongBoardsBot.Models.Handlers
{
    public partial class StageHandler
    {
        private readonly LongboardistDBContext ctx;

        public StageHandler(LongboardistDBContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task HandleMessage(TelegramBotClient client, Message message)
        {
            if (message == null)
                return;

            var storage = ctx.BotUsers;

            var includedQuery = storage
                .Include(i => i.CurrentPurchase)
                    .ThenInclude(j => j.Basket)
                        .ThenInclude(k => k.Longboard)
                .Include(i => i.CurrentPurchase)
                    .ThenInclude(j => j.BotUser)
                .Include(i => i.CurrentPurchase)
                    .ThenInclude(i => i.Basket)
                        .ThenInclude(i => i.BotUser)
                .Include(i => i.Pending)
                .Include(i => i.History)
                    .ThenInclude(j => j.User)
                .Include(i => i.CurrentPurchase)
                .Include(i => i.Purchases)
                .Include(i => i.Comments)
                .Include(i => i.TestingInfo)
                    .ThenInclude(j => j.BotUser);

            var chatId = message.Chat.Id;
            var userId = message.From.Id;
            var text = message.Text;
            var instance = await includedQuery.FirstOrDefaultAsync(i => i.ChatId == chatId);
            var absent = instance == null;

            if (absent)
            {
                instance = new BotUser
                {
                    ChatId = chatId,
                    UserName = message.Chat.Username,
                    Stage = Stage.AskingName,
                    History = new List<ChatMessage>(16),
                    UserId = message.From.Id
                };

                storage.Add(instance);
            }
            else
            {
                // check if new properties (new columns in db) are inited for this user

                if (instance.CurrentPurchase == null)
                {
                    instance.CurrentPurchase = new Purchase
                    {
                        Basket = new List<BotUserLongBoard>(),
                        BotUser = instance,
                        Guid = Guid.NewGuid()
                    };
                }

                instance.UserId = message.From.Id;
                
                if (instance.TestingInfo == null)
                {
                    instance.TestingInfo = new TestingInfo();
                }
            }

            async Task DoWork()
            {
                instance.History.Add(new ChatMessage(message.MessageId, false));

                if (text == RestartCommand && !absent)
                {
                    await ReloadUserChat(client, instance);
                }
                if (text == GetCommentsCommand)
                {
                    var comments = await ctx.Comments.Select(i => i.Data).ToArrayAsync();

                    var msg = await client.SendOrEditCommentsView(chatId, comments);

                    instance.History.AddMessage(msg, false);

                    return;
                }

                switch (instance.Stage)
                {
                    case Stage.AskingName:
                        {
                            instance.Stage = Stage.GettingName;
                            await GreetAndAskName(client, chatId, instance);
                            break;
                        }
                    case Stage.GettingName:
                        {
                            if (!Regex.IsMatch(text, NameRegexp))
                            {
                                await AskToEnterCorrectName(client, instance, chatId);
                            }
                            else
                            {
                                instance.Stage = Stage.GettingPhone;
                                await UpdateNameAndAskPhone(client, message, instance);
                            }

                            break;
                        }
                    case Stage.GettingPhone:
                        {
                            if (!Regex.IsMatch(text, PhoneRegexp))
                            {
                                await AskToEnterCorrectPhone(client, chatId, instance);
                            }
                            else
                            {
                                instance.Stage = Stage.ReceivingIsLivingInKharkivOrNot;

                                await UpdatePhoneInfo(client, message, instance);

                                var replyMarkup = new ReplyKeyboardMarkup(
                                new[] {
                                    new KeyboardButton(YesText),
                                    new KeyboardButton(NoText)
                                }, resizeKeyboard: true, oneTimeKeyboard: true);

                                var toUserText = "Вы живете в Харькове?";

                                var msg = await client.SendTextMessageAsync(chatId, toUserText, replyMarkup: replyMarkup);

                                instance.History.AddMessage(msg, false);
                            }

                            break;
                        }
                    case Stage.ReceivingIsLivingInKharkivOrNot:
                        {
                            if (text == YesText)
                            {
                                instance.IsLivingInKharkiv = true;
                            }
                            else if (text == NoText)
                            {
                                instance.IsLivingInKharkiv = false;
                            }
                            else
                            {
                                return;
                            }

                            var msg = await client.AskToTypeStatisticsAsync(instance.ChatId);

                            instance.History.AddMessage(msg, false);
                            instance.Stage = Stage.ReceivingDoesWantToTypeStatistics;

                            break;
                        }
                    case Stage.ReceivingDoesWantToTypeStatistics:
                        {
                            if (text == NoText)
                            {
                                var msg = await client.SendMenuAsync(instance);

                                instance.History.AddMessage(msg, false);
                                instance.Stage = Stage.ReceivingMenuItem;
                            }
                            else if (text == YesText)
                            {
                                // TODO...
                            }

                            break;
                        }
                    case Stage.ReceivingMenuItem:
                        {
                            if (text == StartPurchasingText)
                            {
                                await SendAllLongboards(client, message, instance);
                                instance.Stage = Stage.WhatLongBoard;
                            }
                            else if (text == StartTestingText)
                            {
                                var tasks = client.AskDateOfVisit(chatId);

                                await Task.WhenAll(tasks);

                                instance.History.AddMessages(tasks.Select(i => i.Result), false);

                                instance.Stage = Stage.ReceivingDateOfVisit;
                            }

                            break;
                        }
                    case Stage.ReceivingDateOfVisit:
                        {
                            if (text == CancelText)
                            {
                                await AskIfShouldRestartDialog(instance, client);
                                instance.Stage = Stage.ShouldRestartDialog;

                                return;
                            }

                            if (!DateTime.TryParseExact(text, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                            {
                                var msg = await client.SendTextMessageAsync(chatId, "Неверный формат. Попробуйте еще раз");

                                instance.History.AddMessage(msg, false);
                            }
                            else if (date.CompareTo(DateTimeExtensions.GetNowKharkiv()) <= 0)
                            {
                                var msg = await client.SendTextMessageAsync(chatId, "Невозможно записаться на прошлое время 😒");

                                instance.History.AddMessage(msg, false);
                            }
                            else
                            {
                                instance.TestingInfo.VisitDateTime = date;

                                await OnTestingFinishing(instance, client);
                            }

                            break;
                        }
                    case Stage.WhatLongBoard:
                        {
                            var isLongBoard = ExistsLongBoard(text);

                            if (isLongBoard)
                            {
                                instance.Stage = Stage.ShouldAddLongboardToBasket;
                                await SendInfoAboutLongboard(client, text, instance);
                            }

                            break;
                        }
                    case Stage.ShouldAddLongboardToBasket:
                        {
                            // cancelling with 0 lb chosen
                            if (text == CancelText && instance.CurrentPurchase.Basket.Count == 0)
                            {
                                await RestartPurchasing(instance, client);
                            }
                            else if (text == AddText)
                            {
                                instance.Stage = Stage.HowManyLongboards;
                                await AskToTypeAmountOfLBoards(client, instance);
                            } // cancelling with more than 1 lb chosen
                            else if (text == CancelText)
                            {
                                instance.Stage = Stage.ShouldContinueAddingToBasket;
                                await AskIfShouldContinueAddingToBasket(client, instance);
                            }

                            break;
                        }
                    case Stage.HowManyLongboards:
                        {
                            if (int.TryParse(text, out var amount) && amount > 0)
                            {
                                instance.Stage = Stage.ShouldContinueAddingToBasket;
                                await AddToBasketAndNotify(client, instance, amount);
                                await AskIfShouldContinueAddingToBasket(client, instance);
                            }

                            break;
                        }
                    case Stage.ShouldContinueAddingToBasket:
                        {
                            if (text == YesText)
                            {
                                var success = await SendNotInBasketLBoards(client, chatId, instance);

                                if (success)
                                {
                                    instance.Stage = Stage.WhatLongBoard;
                                }
                                else
                                {
                                    await AskIfShouldContinueAddingToBasket(client, instance);
                                }
                            }
                            else if (text == CleanUpBasketText)
                            {
                                await RestartPurchasing(instance, client);
                            }
                            else if (text == FinishText)
                            {
                                instance.Stage = Stage.GettingShouldDeliverToHomeOrNot;

                                await AskAboutDelivery(client, instance);
                            }

                            break;
                        }
                    case Stage.GettingShouldDeliverToHomeOrNot:
                        {
                            if (text == DeliverBtnText)
                            {
                                instance.Stage = Stage.GettingHomeAdress;

                                await AskToTypeAdressToDeliver(client, chatId, instance);
                            }
                            else if (text == TakeMySelfBtnText)
                            {
                                await SendPlaceForTakingLBInfo(client, chatId, instance);
                                await OnPurchaseFinishing(instance, client);
                            }

                            break;
                        }
                    case Stage.GettingHomeAdress:
                        {
                            await OnPurchaseFinishing(instance, client, text);

                            break;
                        }
                    case Stage.ShouldRestartDialog:
                        {
                            if (text == RestartText)
                            {
                                await StartNewDialog(instance, client);
                            }

                            break;
                        }
                    case Stage.ProcessingWantsToComment:
                        {
                            if (text == WantsAddComment)
                            {
                                instance.Stage = Stage.TypingComment;

                                await client.SendChatActionAsync(chatId, ChatAction.Typing);

                                var msg = await client.SendTextMessageAsync(chatId,
                                    "Напишите комментарий",
                                    replyMarkup: new ReplyKeyboardMarkup(new[] { new KeyboardButton(CancelText) }, true, true));

                                instance.History.AddMessage(msg, false);
                            }
                            else if (text == NotWantsAddComment)
                            {
                                instance.Stage = Stage.ShouldRestartDialog;

                                await AskIfShouldRestartDialog(instance, client);
                            }

                            break;
                        }
                    case Stage.TypingComment:
                        {
                            if (text == CancelText)
                            {
                                instance.Stage = Stage.ShouldRestartDialog;
                                await AskIfShouldRestartDialog(instance, client);
                            }
                            else
                            {
                                if (text == null)
                                    return;

                                instance.Comments.Add(new Comment
                                {
                                    Author = instance,
                                    Data = text
                                });

                                instance.Stage = Stage.ShouldRestartDialog;

                                await AskIfShouldRestartDialog(instance, client);
                            }

                            break;
                        }
                }
            }
            Task SaveChanges()
            {
                if (!absent)
                {
                    ctx.Entry(instance).State = EntityState.Modified;
                }

                return ctx.SaveChangesAsync();
            }

            await DoWork();
            await SaveChanges();
        }
    }

    public partial class StageHandler
    {
        private static async Task<Message> AskToTypeAdressToDeliver(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, WriteHomeAdressText);

            instance.History.AddMessage(msg, false);

            return msg;
        }

        private static async Task<Message> SendPlaceForTakingLBInfo(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, PlaceToTakeLongBoardText);

            instance.History.AddMessage(msg, true);

            return msg;
        }
        private static async Task<Message> AskAboutDelivery(TelegramBotClient client, BotUser instance)
        {
            var chatId = instance.ChatId;
            var history = instance.History;

            var msg = await client.SendTextMessageAsync(chatId, DeliverOrNotText, replyMarkup: DeliverOrNotKBoard);

            history.AddMessage(msg, false);

            return msg;
        }

        private static Task<bool> SendNotInBasketLBoards(TelegramBotClient client, long chatId, BotUser instance)
        {
            var thereAreLBsLeftTask =
                SendLongBoards(
                    client,
                    instance.ChatId,
                    instance.History,
                    instance.CurrentPurchase.Basket.Select(i => i.Longboard));

            return thereAreLBsLeftTask;
        }

        private static async Task AddToBasketAndNotify(TelegramBotClient client, BotUser instance, int amount)
        {
            var chatId = instance.ChatId;
            var pending = instance.Pending;
            var reportText = Format(AddedToBasketNotificationText, amount, pending.Style);
            var msgTask = client.SendTextMessageAsync(chatId, reportText);

            instance.CurrentPurchase.Basket.Add(
                new BotUserLongBoard
                {
                    BotUser = instance,
                    Longboard = pending,
                    BotUserId = instance.ChatId,
                    LongboardId = pending.Id,
                    Amount = amount
                });

            instance.History.AddMessage(await msgTask, false);
        }

        private static async Task AskIfShouldContinueAddingToBasket(TelegramBotClient client, BotUser instance)
        {
            instance.Pending = null;

            var msg1 = await SendInfoAboutBasket(client, instance);
            var msg2 = await SendShouldContinueAddingToBasket(client, instance);

            instance.History.AddMessages(new[] { msg1, msg2 }, false);
        }

        private static async Task AskToTypeAmountOfLBoards(TelegramBotClient client, BotUser instance)
        {
            var chatId = instance.ChatId;
            var style = instance.Pending.Style;
            var msg = await client
                .SendTextMessageAsync(chatId,
                Format(AskingToEnterAmountOfLBText, style));

            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private async Task SendInfoAboutLongboard(TelegramBotClient client, string text, BotUser instance)
        {
            var chatId = instance.ChatId;

            var lb = await ctx.Longboards.FirstAsync(i => i.Style == text);

            instance.Pending = lb;

            await SendInfoAbout(lb, instance, client);
            var msg = await SendShouldAddToBasketKeyboard(client, chatId, text);

            instance.History.AddMessage(msg, false);
        }

        private static Task<bool> SendAllLongboards(TelegramBotClient client, Message message, BotUser instance)
                    => SendLongBoards(client, message.Chat.Id, instance.History);
        
        private static async Task UpdatePhoneInfo(TelegramBotClient client, Message message, BotUser instance)
        {
            var msgUpdate = await UpdateUsersPhoneAndUsername(client, message, instance);
            instance.History.AddMessage(msgUpdate, false);
        }

        private static async Task AskToEnterCorrectPhone(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, EnterCorrectPhoneText);

            instance.History.AddMessage(msg, false);
        }

        private static async Task UpdateNameAndAskPhone(TelegramBotClient client, Message message, BotUser instance)
        {
            var msg2 = await UpdateUsersNameAndUsername(client, message, instance);
            var msg1 = await client.AskPhone(message.Chat.Id);

            instance.History.AddMessages(new[] { msg1, msg2 }, false);
        }
        
        private static async Task AskToEnterCorrectName(TelegramBotClient client, BotUser instance, long chatId)
        {
            var msg = await client.SendTextMessageAsync(chatId, EnterCorrectNameText);
            instance.History.AddMessage(msg, false);
        }

        private static async Task GreetAndAskName(TelegramBotClient client, long chatId, BotUser instance)
        {
            await client.SendChatActionAsync(chatId, ChatAction.Typing);

            var greetingTextTask = Texts.GetGreetingTextAsync();

            var msg2 = await client.SendStickerAsync(chatId, GreetingStickerId);
            var msg1 = await client.SendTextMessageAsync(chatId, await greetingTextTask);
            var msg = await client.AskName(chatId);

            instance.History.AddMessages(new[] { msg, msg1, msg2 }, false);
        }

        private static Task ReloadUserChat(TelegramBotClient client, BotUser instance)
        {
            instance.Stage = Stage.AskingName;

            var clearHistory = ClearHistory(instance, client, deleteAll: true);

            instance.Pending = null;

            instance.CurrentPurchase = new Purchase
            {
                BotUser = instance,
                Basket = new List<BotUserLongBoard>(5),
                Guid = Guid.NewGuid()
            };

            return clearHistory;
        }

        private async Task OnTestingFinishing(BotUser instance, TelegramBotClient client)
        {
            await NotifyAboutTesting(instance, client);

            await AskIfShouldRestartDialog(instance, client);
            instance.Stage = Stage.ShouldRestartDialog;
        }

        private Task NotifyAboutTesting(BotUser instance, TelegramBotClient client)
        {
            var toAdmins = NotifyAdminsAboutTesting(instance, client);
            var toUser = NotifyUserAboutTesting(instance, client);

            return Task.WhenAll(toAdmins, toUser);
        }

        private async Task<Message> NotifyAdminsAboutTesting(BotUser instance, TelegramBotClient client)
        {
            var text = await GetFormattedFinalTestingTextToAdminsAsync(instance);

            var inlineKBoard = new InlineKeyboardMarkup(
                        new[] {
                            new InlineKeyboardButton
                            {
                               Text = TestedText, CallbackData = $"{TestedData}{instance.ChatId}"
                            },
                            new InlineKeyboardButton
                            {
                               Text = CancelTestingText, CallbackData = $"{CancelTestingData}{instance.ChatId}"
                            }
                        });

            return await client.SendTextMessageAsync(AdminGroupChatId, text, parseMode: ParseMode.Markdown, replyMarkup: inlineKBoard);
        }

        private async Task<Message> NotifyUserAboutTesting(BotUser instance, TelegramBotClient client)
        {
            // TODO: add pin down here... 
            var msg = await client.SendTextMessageAsync(instance.ChatId, $"Вы записаны на тестирование на {instance.TestingInfo.VisitDateTime}");

            instance.History.AddMessage(msg, true);

            return msg;
        }

        private async Task OnPurchaseFinishing(BotUser instance, TelegramBotClient client, string adressToDeliver = null)
        {
            instance.CurrentPurchase.Cost = instance.CurrentPurchase.Basket.GetCost();
            instance.CurrentPurchase.Delivered = false;
            instance.CurrentPurchase.AdressToDeliver = adressToDeliver;

            instance.Purchases.Add(instance.CurrentPurchase);

            await NotifyAboutPurchase(instance, client);

            await ClearHistory(instance, client);

            await AskIfShouldRestartDialog(instance, client);
            instance.Stage = Stage.ShouldRestartDialog;
        }

        private static async Task NotifyAboutPurchase(BotUser instance, TelegramBotClient client)
        {
            var toAdmins = NotifyAdmins(instance, client);
            var toUser = NotifyUser(instance, client);

            var finalTextToUserTask = Texts.GetFinalTextToUserAsync();

            await Task.WhenAll(finalTextToUserTask, toUser, toAdmins);

            var finalMsgToUser = await client.SendTextMessageAsync(instance.ChatId, finalTextToUserTask.Result);

            instance.History.AddMessages(new[] { toUser.Result, finalMsgToUser }, true);
        }

        private static Task<Message> NotifyUser(BotUser instance, TelegramBotClient client)
        {
            var lbrds = Join(ElementsSeparator, instance.CurrentPurchase.Basket);
            var cost = Math.Round(instance.CurrentPurchase.Cost, 2);

            var text = Format(
                UserPurchaseInfoText,
                lbrds,
                cost.ToString(),
                instance.CurrentPurchase.Guid.ToStringHashTag()); // TODO: price in USD UAH

            var message = client.SendTextMessageAsync(instance.ChatId, text);

            return message;
        }

        private static async Task<Message> NotifyAdmins(BotUser instance, TelegramBotClient client)
        {
            var textToAdminGroup = await GetFormattedFinalTextToAdminsAsync(instance);

            var inlineKBoard =
                new InlineKeyboardMarkup(
                    new[] {
                        new InlineKeyboardButton
                        {
                           Text = DeliveredText, CallbackData = $"{DeliveredData}{instance.CurrentPurchase.Guid}"
                        },
                        new InlineKeyboardButton
                        {
                           Text = CancelDeliveryText, CallbackData = $"{CancelDeliveryData}{instance.CurrentPurchase.Guid}"
                        }
                    });

            return await client.SendTextMessageAsync(AdminGroupChatId, textToAdminGroup, Markdown, replyMarkup: inlineKBoard);
        }

        private static async Task AskIfShouldRestartDialog(BotUser instance, TelegramBotClient client)
        {
            var shouldRestartMsg = await client.SendTextMessageAsync(instance.ChatId, ShouldRestartText, replyMarkup: RestartKBoard);

            instance.History.AddMessage(shouldRestartMsg, false);
        }
    }
}
