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

namespace LongBoardsBot.Models
{
    public class StageHandler
    {
        private readonly LongboardistDBContext ctx;

        public StageHandler(LongboardistDBContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task HandleUpdate(TelegramBotClient client, Update update)
        {
            var message = update.Message;

            if (message == null)
                return;

            var storage = ctx.BotUsers;

            var includedQuery = storage
                .Include(i => i.BotUserLongBoards)
                    .ThenInclude(j => j.Longboard)
                .Include(i => i.BotUserLongBoards)
                    .ThenInclude(j => j.BotUser)
                .Include(i => i.Pending)
                .Include(i => i.History)
                    .ThenInclude(j => j.User);

            var chatId = message.Chat.Id;
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
                    History = new List<ChatMessage>(16)
                };

                storage.Add(instance);
            }

            async Task DoWork()
            {
                instance.History.Add(new ChatMessage(message.MessageId, false));

                if (text == RestartCommand)
                {
                    if (!absent)
                    {
                        await ReloadUserChat(client, instance);
                    }
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
                                instance.Stage = Stage.WhatLongBoard;

                                await UpdatePhoneInfo(client, message, instance);

                                await SendAllLongboards(client, message, instance);
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
                            if (text == CancelText && instance.BotUserLongBoards.Count == 0)
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
                            if (Int32.TryParse(text, out var amount) && amount > 0)
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
                            else if (text == CancelText)
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
                }
            }
            async Task SaveChanges()
            {
                if (!absent)
                {
                    ctx.Entry(instance).State = EntityState.Modified;
                }

                await ctx.SaveChangesAsync();
            }

            await DoWork();
            await SaveChanges();
        }

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

        private static async Task<bool> SendNotInBasketLBoards(TelegramBotClient client, long chatId, BotUser instance)
        {
            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, PhotosAreBeingSentText);
            var thereAreLBsLeftTask =
                SendLongBoards(
                    client,
                    instance.ChatId,
                    instance.History,
                    instance.BotUserLongBoards.Select(i => i.Longboard));

            await Task.WhenAll(waitForPhotosMsgTask, thereAreLBsLeftTask);

            instance.History.AddMessage(waitForPhotosMsgTask.Result, false);

            return thereAreLBsLeftTask.Result;
        }

        private static async Task AddToBasketAndNotify(TelegramBotClient client, BotUser instance, int amount)
        {
            var chatId = instance.ChatId;
            var pending = instance.Pending;
            var reportText = Format(AddedToBasketNotificationText, amount, pending.Style);
            var msgTask = client.SendTextMessageAsync(chatId, reportText);

            instance.BotUserLongBoards.Add(
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

        private static async Task SendAllLongboards(TelegramBotClient client, Message message, BotUser instance)
        {
            var chatId = instance.ChatId;

            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, PhotosAreBeingSentText);
            var photosTask = SendLongBoards(client, message.Chat.Id, instance.History);

            await Task.WhenAll(waitForPhotosMsgTask, photosTask);

            instance.History.AddMessage(waitForPhotosMsgTask.Result, false);
        }

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
            instance.BotUserLongBoards.Clear();

            return clearHistory;
        }

        private async Task OnPurchaseFinishing(BotUser instance, TelegramBotClient client, string adressToDeliver = null)
        {
            instance.Stage = Stage.ShouldRestartDialog;

            await NotifyAboutPurchase(instance, client, adressToDeliver);

            await ClearHistory(instance, client);

            await AskIfShouldRestartPurchasing(instance, client);
        }

        private static async Task NotifyAboutPurchase(BotUser instance, TelegramBotClient client, string adressToDeliver)
        {
            var readFinalMsgToUserTask = Texts.GetFinalTextToUserAsync();
            var readFinalMsgToAdminTask = Texts.GetFinalTextToAdminsAsync();

            var lbrds = Join(ElementsSeparator, instance.BotUserLongBoards);
            var cost = Math.Round(instance.BotUserLongBoards.GetCost(), 2);
            var adressInfo = adressToDeliver == null ? NoDeliveryInfo : adressToDeliver;

            var textToAdminGroup = Format(await readFinalMsgToAdminTask, instance.UserName, lbrds, cost.ToString(), instance.Name, instance.Phone, adressInfo);
            var textToUser = Format(UserPurchaseInfoText, lbrds, cost.ToString()); // TODO: price in USD UAH

            var msgUserTask = client.SendTextMessageAsync(instance.ChatId, textToUser);
            var msgAdminTask = client.SendTextMessageAsync(AdminGroupChatId, textToAdminGroup);

            await readFinalMsgToUserTask;
            await msgUserTask;

            var finalMsgToUser = client.SendTextMessageAsync(instance.ChatId, readFinalMsgToUserTask.Result);

            await Task.WhenAll(msgAdminTask, finalMsgToUser);

            instance.History.AddMessages(new[] { msgUserTask.Result, finalMsgToUser.Result }, true);
        }

        private static async Task AskIfShouldRestartPurchasing(BotUser instance, TelegramBotClient client)
        {
            var shouldRestartMsg = await client.SendTextMessageAsync(instance.ChatId, ShouldRestartText, replyMarkup: RestartKBoard);

            instance.History.AddMessage(shouldRestartMsg, false);
        }
    }
}
