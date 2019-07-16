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
            var entry = included.FirstOrDefault(i => i.ChatId == chatId);

            if (entry == null)
            {
                entry = new BotUser
                {
                    ChatId = chatId,
                    Name = "0",
                    Phone = "0",
                    UserName = update.Message.Chat.Username,
                    Stage = 0,
                    History = new List<ChatMessage>(4)
                };

                storage.Add(entry);
                await ctx.SaveChangesAsync();
            }

            entry.History.Add(new ChatMessage(update.Message.MessageId, false));

            switch (entry.Stage)
            {
                case Stage.AskingName:
                    {
                        var msg = await client.AskName(chatId);

                        entry.History.Add(new ChatMessage(msg.MessageId, false));

                        entry.Stage = Stage.GettingName;
                        break;
                    }

                case Stage.GettingName:
                    {
                        var msg2 = await UpdateUsersNameAndUsername(client, update.Message, storage);
                        var msg1 = await client.AskPhone(update.Message.Chat.Id, !IsNullOrWhiteSpace(update.Message.Chat.Username));

                        entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false))); // simplify

                        entry.Stage = Stage.GettingPhone;
                        break;
                    }

                case Stage.GettingPhone:
                    {
                        if (!Regex.IsMatch(update.Message.Text, @"^\+?3?8?(0[5-9][0-9]\d{7})$"))
                        {
                            await client.SendTextMessageAsync(chatId, @"Вы ввели некорректный номер. Ввведите номер, начинающийся на +380...");
                            return;
                        }

                        entry.Stage = Stage.ProcessingLongboardsKeyboardInput;

                        var msgUpdate = await UpdateUsersPhoneAndUsername(client, update.Message, storage);

                        var waitForPhotosMsgTask = client.SendTextMessageAsync(chatId, "Идет отправка фотографий...");
                        var photosTask = SendLongBoards(client, update.Message.Chat.Id, entry.History);

                        await Task.WhenAll(waitForPhotosMsgTask, photosTask);

                        entry.History.Add(new ChatMessage(msgUpdate.MessageId, false));
                        entry.History.Add(new ChatMessage(waitForPhotosMsgTask.Result.MessageId, false));

                        break;
                    }

                case Stage.ProcessingLongboardsKeyboardInput:
                    {
                        var lbText = update.Message.Text;

                        var isLongBoard = ExistsLongBoard(lbText);

                        if (!isLongBoard)
                            return;

                        var lb = await ctx.Longboards.FirstAsync(i => i.Style == lbText);

                        entry.Pending = lb;
                        entry.Stage = Stage.ProcessingBasketKeyboardInput; // set before messages are sent so that there won't be multiple msg sent

                        // TODO: add here sending more info about lb
                        await SendInfoAbout(entry.Pending, entry, client);
                        var msgShouldAddToKBoard = await SendShouldAddToBasketKeyboard(client, chatId, lbText);

                        entry.History.Add(new ChatMessage(msgShouldAddToKBoard.MessageId, false));

                        break;
                    }

                case Stage.ProcessingBasketKeyboardInput:
                    {
                        bool ValidateResult(string value) => value == CancelText || value == AddText;

                        var result = update.Message.Text;

                        if (!ValidateResult(result))
                            return;

                        if (result == AddText)
                        {
                            entry.BotUserLongBoards.Add(new BotUserLongBoard()
                            {
                                BotUser = entry,
                                Longboard = entry.Pending,
                                BotUserId = entry.ChatId,
                                LongboardId = entry.Pending.Id
                            });

                            var msg = await client.SendTextMessageAsync(chatId, $"Вы успешно добавили {entry.Pending} лонгборд в корзину!");

                            entry.History.Add(new ChatMessage(msg.MessageId, false));
                        }

                        entry.Pending = null;
                        entry.Stage = Stage.AskingIfShouldContinueAddingToBasket;

                        var msg1 = await SendInfoAboutBasket(client, entry);
                        var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                        entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                        break;
                    }

                case Stage.AskingIfShouldContinueAddingToBasket:
                    {
                        var result = update.Message.Text;

                        switch (result)
                        {
                            case YesText:
                                {
                                    var successful = await SendLongBoards(client, entry.ChatId, entry.History, entry.BotUserLongBoards.Select(i => i.Longboard));

                                    if (successful)
                                        entry.Stage = Stage.ProcessingLongboardsKeyboardInput;
                                    else
                                    {
                                        var msg1 = await SendInfoAboutBasket(client, entry);
                                        var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                                        entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                                        entry.Stage = Stage.AskingIfShouldContinueAddingToBasket;
                                    }

                                    break;
                                }

                            case CancelText:
                                await RestartDialog(entry, client);
                                break;
                            case FinishText:
                                {
                                    var readFinalMsgTask = ReadAllLinesAsync(FinalMessagePath);
                                    var lbrds = Join(", ", entry.BotUserLongBoards.Select(i => i.Longboard).Select(i => i.Style));
                                    var phone = entry.Phone;
                                    var username = entry.UserName;
                                    var name = entry.Name;
                                    var userChatId = entry.ChatId;

                                    var msgToAdminGroup =
                                        $"@{username} хочет купить {{{lbrds}}}{Environment.NewLine}" +
                                        $"Имя: {name}, Телефон: {phone}";
                                    var msgToUser = // TODO
                                        $"Вы купили {lbrds}. Стоимость = 5000 долларов. "; // price in 

                                    var msgUserTask = client.SendTextMessageAsync(userChatId, msgToUser);
                                    var msgAdminTask = client.SendTextMessageAsync(AdminGroupChatId, msgToAdminGroup);

                                    await Task.Delay(50);

                                    var msgUserTask2 = client
                                        .SendTextMessageAsync(userChatId, await readFinalMsgTask);

                                    await Task.WhenAll(msgUserTask, msgAdminTask, msgUserTask2);

                                    entry.History.Add(new ChatMessage(msgUserTask.Result.MessageId, true));
                                    entry.History.Add(new ChatMessage(msgUserTask2.Result.MessageId, true));

                                    await ClearHistory(entry, client);

                                    entry.Stage = Stage.ShouldRestartDialog;
                                    await client.SendTextMessageAsync(chatId, "Начать покупки заново?", replyMarkup: RestartKBoard);

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
                        if (text != RestartText)
                        {
                            return;
                        }

                        await StartNewDialog(entry, client);

                        break;
                    }
            }

            ctx.Entry(entry).State = EntityState.Modified;

            await ctx.SaveChangesAsync();
        }
    }
}
