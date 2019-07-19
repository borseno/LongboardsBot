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
using static LongBoardsBot.Helpers.FileExtensions;
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

            if (instance == null)
            {
                instance = new BotUser
                {
                    ChatId = chatId,
                    UserName = message.Chat.Username,
                    Stage = Stage.AskingName,
                    History = new List<ChatMessage>(16)
                };

                storage.Add(instance);
                await ctx.SaveChangesAsync(); // test removing this line, my bet: shouldn't make a difference
            }

            async Task DoWork()
            {
                instance.History.Add(new ChatMessage(message.MessageId, false));

                if (text == RestartCommand)
                {
                    await ReloadUserChat(client, instance);
                }

                switch (instance.Stage)
                {
                    case Stage.AskingName:
                        {
                            await GreetAndAskName(client, chatId, instance);

                            instance.Stage = Stage.GettingName;

                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();

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
                                await UpdateNameAndAskPhone(client, message, storage, instance);
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

                                await UpdatePhoneInfo(client, message, storage, instance);

                                await SendAllLongboards(client, message, chatId, instance);
                            }

                            break;
                        }
                    case Stage.WhatLongBoard:
                        {
                            var isLongBoard = ExistsLongBoard(text);

                            if (isLongBoard)
                            {
                                instance.Stage = Stage.ShouldAddLongboardToBasket;
                                await SendInfoAboutLongboard(client, chatId, text, instance);
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
                                await AskToTypeAmountOfLBoards(client, chatId, instance);
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
                                await AddToBasketAndNotify(client, chatId, instance, amount);
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

                                await AskAboutDelivery(client, chatId, instance.History);
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
                ctx.Entry(instance).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
            }

            await DoWork();
            await SaveChanges();
        }

        private static async Task AskToTypeAdressToDeliver(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, WriteHomeAdressText);

            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private static async Task SendPlaceForTakingLBInfo(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, PlaceToTakeLongBoardText);

            instance.History.Add(new ChatMessage(msg.MessageId, true));
        }

        private static async Task AskAboutDelivery(TelegramBotClient client, long chatId, IList<ChatMessage> history)
        {
            var msg = await client.SendTextMessageAsync(
                chatId,
                DeliverOrNotText,
                replyMarkup: DeliverOrNotKBoard);

            history.Add(new ChatMessage(msg.MessageId, false));
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

            instance.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));

            return thereAreLBsLeftTask.Result;
        }

        private static async Task AddToBasketAndNotify(TelegramBotClient client, long chatId, BotUser instance, int amount)
        {
            var pending = instance.Pending;

            instance.BotUserLongBoards.Add(
                new BotUserLongBoard
                {
                    BotUser = instance,
                    Longboard = pending,
                    BotUserId = instance.ChatId,
                    LongboardId = pending.Id,
                    Amount = amount
                });

            var reportText = Format(AddedToBasketNotificationText, amount, pending.Style);
            var msg = await client.SendTextMessageAsync(chatId, reportText);
            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private static async Task AskIfShouldContinueAddingToBasket(TelegramBotClient client, BotUser instance)
        {
            instance.Pending = null;

            var msg1 = await SendInfoAboutBasket(client, instance);
            var msg2 = await SendShouldContinueAddingToBasket(client, instance);

            instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));
        }

        private static async Task AskToTypeAmountOfLBoards(TelegramBotClient client, long chatId, BotUser instance)
        {
            var style = instance.Pending.Style;
            var msg = await client
                .SendTextMessageAsync(chatId,
                Format(AskingToEnterAmountOfLBText, style));

            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private async Task SendInfoAboutLongboard(TelegramBotClient client, long chatId, string text, BotUser instance)
        {
            var lb = await ctx.Longboards.FirstAsync(i => i.Style == text);

            instance.Pending = lb;

            await SendInfoAbout(lb, instance, client);
            var msgShouldAddToKBoard = await SendShouldAddToBasketKeyboard(client, chatId, text);

            instance.History.Add(new ChatMessage(msgShouldAddToKBoard.MessageId, false));
        }

        private static async Task SendAllLongboards(TelegramBotClient client, Message message, long chatId, BotUser instance)
        {
            var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, PhotosAreBeingSentText);
            var photosTask = SendLongBoards(client, message.Chat.Id, instance.History);

            await Task.WhenAll(waitForPhotosMsgTask, photosTask);

            instance.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));
        }

        private static async Task UpdatePhoneInfo(TelegramBotClient client, Message message, DbSet<BotUser> storage, BotUser instance)
        {
            var msgUpdate = await UpdateUsersPhoneAndUsername(client, message, storage);
            instance.History.Add(new ChatMessage(msgUpdate.MessageId, false));
        }

        private static async Task AskToEnterCorrectPhone(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg = await client.SendTextMessageAsync(chatId, EnterCorrectPhoneText);

            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private static async Task UpdateNameAndAskPhone(TelegramBotClient client, Message message, DbSet<BotUser> storage, BotUser instance)
        {
            var msg2 = await UpdateUsersNameAndUsername(client, message, storage);
            var msg1 = await client.AskPhone(message.Chat.Id, !IsNullOrWhiteSpace(message.Chat.Username));

            instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false))); // simplify
        }

        private static async Task AskToEnterCorrectName(TelegramBotClient client, BotUser instance, long chatId)
        {
            var msg = await client.SendTextMessageAsync(chatId, EnterCorrectNameText);
            instance.History.Add(new ChatMessage(msg.MessageId, false));
        }

        private static async Task GreetAndAskName(TelegramBotClient client, long chatId, BotUser instance)
        {
            var msg2 = await client.SendStickerAsync(chatId, "CAADAgADKwcAAmMr4gmfxHm1DmV88gI");
            var msg1 = await client.SendTextMessageAsync(chatId, GreetingText);
            var msg = await client.AskName(chatId);

            instance.History.Add(new ChatMessage(msg.MessageId, false));
            instance.History.Add(new ChatMessage(msg1.MessageId, false));
            instance.History.Add(new ChatMessage(msg2.MessageId, false));
        }

        private static async Task ReloadUserChat(TelegramBotClient client, BotUser instance)
        {
            await ClearHistory(instance, client, deleteAll: true);

            instance.Pending = null;
            instance.BotUserLongBoards.Clear();
            instance.Stage = Stage.AskingName;
        }

        private async Task OnPurchaseFinishing(BotUser instance, TelegramBotClient client, string adressToDeliver = null)
        {
            var chatId = instance.ChatId;

            instance.Stage = Stage.ShouldRestartDialog;

            var readFinalMsgTask = ReadAllLinesAsync(FinalMessagePath);
            var lbrds = Join(", ", instance.BotUserLongBoards.Select(i => i.Longboard.Style + "{" + i.Amount + "}"));
            var phone = instance.Phone;
            var username = instance.UserName;
            var name = instance.Name;
            var userChatId = instance.ChatId;
            var cost = Math.Round(
                instance
                .BotUserLongBoards
                .Select(i => i.Longboard.Price * i.Amount)
                .Sum(), 2);

            var msgToAdminGroup =
                $"@{username} хочет купить {{{lbrds}}} (итого стоимость: {cost.ToString()})" +
                $"{Environment.NewLine}" +
                $"Имя: {name}, Телефон: {phone}" +
                Environment.NewLine +
                $"{(adressToDeliver != null ? "Адрес для доставки:" : "Заберет сам")} {adressToDeliver}";

            var msgToUser = // TODO
                $"Вы купили {lbrds}. Стоимость = {cost.ToString()} "; // price in 

            var msgUserTask = client.SendTextMessageAsync(userChatId, msgToUser);
            var msgAdminTask = client.SendTextMessageAsync(AdminGroupChatId, msgToAdminGroup);

            await Task.Delay(100);

            var msgUserTask2 = client
                .SendTextMessageAsync(userChatId, await readFinalMsgTask);

            await Task.WhenAll(msgUserTask, msgAdminTask, msgUserTask2);

            instance.History.Add(new ChatMessage(msgUserTask.Result.MessageId, true));
            instance.History.Add(new ChatMessage(msgUserTask2.Result.MessageId, true));

            await ClearHistory(instance, client);

            var shouldRestartMsg = await client.SendTextMessageAsync(chatId, "Начать покупки заново?", replyMarkup: RestartKBoard);

            instance.History.Add(new ChatMessage(shouldRestartMsg.MessageId, false));

            ctx.Entry(instance).State = EntityState.Modified;
            await ctx.SaveChangesAsync();

        }
    }
}
