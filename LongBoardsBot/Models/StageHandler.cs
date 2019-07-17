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
            if (update.Message == null)
                return;

            var storage = ctx.BotUsers;

            var included = ctx
                .BotUsers
                .Include(i => i.BotUserLongBoards)
                    .ThenInclude(j => j.Longboard)
                .Include(i => i.BotUserLongBoards)
                    .ThenInclude(j => j.BotUser)
                .Include(i => i.Pending)
                .Include(i => i.History)
                    .ThenInclude(j => j.User);

            var chatId = update.Message.Chat.Id;
            var instance = await included.FirstOrDefaultAsync(i => i.ChatId == chatId);

            if (instance == null)
            {
                instance = new BotUser
                {
                    ChatId = chatId,
                    Name = "0",
                    Phone = "0",
                    UserName = update.Message.Chat.Username,
                    Stage = 0,
                    History = new List<ChatMessage>(4)
                };

                storage.Add(instance);
                await ctx.SaveChangesAsync();
            }

            instance.History.Add(new ChatMessage(update.Message.MessageId, false));

            if (update.Message.Text == @"/restart")
            {
                await ClearHistory(instance, client);

                instance.Pending = null;
                instance.BotUserLongBoards.Clear();
                instance.Stage = Stage.AskingName;
            }

            switch (instance.Stage)
            {
                case Stage.AskingName:
                    {
                        var msg = await client.AskName(chatId);

                        instance.History.Add(new ChatMessage(msg.MessageId, false));
                        instance.Stage = Stage.GettingName;

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.GettingName:
                    {
                        var msg2 = await UpdateUsersNameAndUsername(client, update.Message, storage);
                        var msg1 = await client.AskPhone(update.Message.Chat.Id, !IsNullOrWhiteSpace(update.Message.Chat.Username));

                        instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false))); // simplify
                        instance.Stage = Stage.GettingPhone;

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.GettingPhone:
                    {
                        if (!Regex.IsMatch(update.Message.Text, @"^\+?3?8?(0[5-9][0-9]\d{7})$"))
                        {
                            var msg = await client.SendTextMessageAsync(chatId, 
                                @"Вы ввели некорректный номер. Ввведите номер, начинающийся на +380...");

                            instance.History.Add(new ChatMessage(msg.MessageId, false));

                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();

                            return;
                        }

                        instance.Stage = Stage.WhatLongBoard;

                        var msgUpdate = await UpdateUsersPhoneAndUsername(client, update.Message, storage);

                        var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, "Идет отправка фотографий...");
                        var photosTask = SendLongBoards(client, update.Message.Chat.Id, instance.History);

                        await Task.WhenAll(waitForPhotosMsgTask, photosTask);

                        instance.History.Add(new ChatMessage(msgUpdate.MessageId, false));
                        instance.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.WhatLongBoard:
                    {
                        var lbText = update.Message.Text;

                        var isLongBoard = ExistsLongBoard(lbText);

                        if (!isLongBoard)
                        {
                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();
                            return;
                        }

                        var lb = await ctx.Longboards.FirstAsync(i => i.Style == lbText);

                        instance.Pending = lb;
                        instance.Stage = Stage.ShouldAddLongboardToBasket; // set before messages are sent so that there won't be multiple msg sent

                        // TODO: add here sending more info about lb
                        await SendInfoAbout(instance.Pending, instance, client);
                        var msgShouldAddToKBoard = await SendShouldAddToBasketKeyboard(client, chatId, lbText);

                        instance.History.Add(new ChatMessage(msgShouldAddToKBoard.MessageId, false));

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.ShouldAddLongboardToBasket:
                    {
                        bool ValidateResult(string value) => value == CancelText || value == AddText;

                        var result = update.Message.Text;

                        if (!ValidateResult(result))
                        {
                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();
                            return;
                        }

                        if (result == CancelText && instance.BotUserLongBoards.Count == 0)
                        {
                            await RestartDialog(instance, client);
                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();
                            return;
                        }

                        if (result == AddText)
                        {
                            instance.Stage = Stage.HowManyLongboards;

                            var msg = await client.SendTextMessageAsync(chatId, 
                                $"Вы собираетесь добавить лонгборды {instance.Pending.Style} " +
                                $"стиля катания в корзину. Укажите количество");

                            instance.History.Add(new ChatMessage(msg.MessageId, false));
                        }
                        else if (result == CancelText)
                        {
                            instance.Pending = null;
                            instance.Stage = Stage.ShouldContinueAddingToBasket;

                            var msg1 = await SendInfoAboutBasket(client, instance);
                            var msg2 = await SendShouldContinueAddingToBasket(client, instance);

                            instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));
                        }

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.HowManyLongboards:
                    {
                        if (!Int32.TryParse(update.Message.Text, out var amount))
                        {
                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();
                            return;
                        }

                        if (amount <= 0)
                        {
                            ctx.Entry(instance).State = EntityState.Modified;
                            await ctx.SaveChangesAsync();
                            return;
                        }

                        instance.BotUserLongBoards.Add(new BotUserLongBoard()
                        {
                            BotUser = instance,
                            Longboard = instance.Pending,
                            BotUserId = instance.ChatId,
                            LongboardId = instance.Pending.Id,
                            Amount = amount
                        });

                        var msg = await client.SendTextMessageAsync(chatId, 
                            $"Вы успешно добавили {amount.ToString()} лонгбордов {instance.Pending.Style} стиля катания в корзину!");

                        instance.History.Add(new ChatMessage(msg.MessageId, false));
                        instance.Pending = null;
                        instance.Stage = Stage.ShouldContinueAddingToBasket;

                        var msg1 = await SendInfoAboutBasket(client, instance);
                        var msg2 = await SendShouldContinueAddingToBasket(client, instance);

                        instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
                case Stage.ShouldContinueAddingToBasket:
                    {
                        var result = update.Message.Text;

                        switch (result)
                        {
                            case YesText:
                                {
                                    var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, "Идет отправка фотографий...");
                                    var successfulTask = SendLongBoards(client, instance.ChatId, instance.History, instance.BotUserLongBoards.Select(i => i.Longboard));

                                    await Task.WhenAll(waitForPhotosMsgTask, successfulTask);

                                    if (successfulTask.Result)
                                        instance.Stage = Stage.WhatLongBoard;
                                    else
                                    {
                                        var msg1 = await SendInfoAboutBasket(client, instance);
                                        var msg2 = await SendShouldContinueAddingToBasket(client, instance);

                                        instance.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                                        instance.Stage = Stage.ShouldContinueAddingToBasket;
                                    }
                                    instance.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));

                                    ctx.Entry(instance).State = EntityState.Modified;
                                    await ctx.SaveChangesAsync();

                                    break;
                                }

                            case CancelText:
                                await RestartDialog(instance, client);
                                ctx.Entry(instance).State = EntityState.Modified;
                                await ctx.SaveChangesAsync();
                                break;
                            case FinishText:
                                {
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
                                        $"Имя: {name}, Телефон: {phone}";
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

                                    break;
                                }
                            default:
                                return;
                        }

                        break;

                    }
                case Stage.ShouldRestartDialog:
                    {
                        var text = update.Message.Text;

                        if (text == RestartText)
                        {
                            await StartNewDialog(instance, client);
                        }
                        
                        ctx.Entry(instance).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();

                        break;
                    }
            }
        }
    }
}
