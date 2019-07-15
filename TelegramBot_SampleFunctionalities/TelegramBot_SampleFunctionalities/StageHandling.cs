using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using static TelegramBot_SampleFunctionalities.Functions;
using static TelegramBot_SampleFunctionalities.Constants;
using System.Threading;
using static System.String;
using System.Text.RegularExpressions;

namespace TelegramBot_SampleFunctionalities
{
    public static class StageHandling
    {
        private static readonly List<BotUser> storage = new List<BotUser>(16);

        public static async Task HandleUpdate(TelegramBotClient client, Update update)
        {
            if (update.Message == null)
                return;

            var chatId = update.Message.Chat.Id;
            var entry = storage.FirstOrDefault(i => i.Id == chatId);

            if (entry == null)
            {
                entry = new BotUser
                {
                    Id = chatId,
                    Stage = 0
                };

                storage.Add(entry);
            }

            entry.History.Add(new ChatMessage(update.Message.MessageId, false));

            switch (entry.Stage)
            {
                case Stage.AskingName:
                    {
                        var msg = await AskName(client, chatId);

                        entry.History.Add(new ChatMessage(msg.MessageId, false));

                        entry.Stage = Stage.GettingName;
                        break;
                    }

                case Stage.GettingName:
                    {
                        var msg2 = await UpdateUsersNameAndUsername(client, update.Message, storage);
                        var msg1 = await AskPhone(client, update.Message.Chat.Id, !String.IsNullOrWhiteSpace(update.Message.Chat.Username));

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
                        var lb = update.Message.Text;

                        var isLongBoard = ValidateLongBoard(lb);

                        if (isLongBoard)
                        {
                            entry.Pending = lb;
                            entry.Stage = Stage.ProcessingBasketKeyboardInput; // set before messages are sent so that there won't be multiple msg sent

                            // TODO: add here sending more info about lb

                            await SendInfoAbout(entry.Pending, entry, client);
                            var msgShouldAddToKBoard = await SendShouldAddToBasketKeyboard(client, chatId, lb);

                            entry.History.AppendMsg(false, msgShouldAddToKBoard);
                        }

                        break;
                    }

                case Stage.ProcessingBasketKeyboardInput:
                    {
                        bool ValidateResult(string value) => value == CancelText || value == AddText;

                        var result = update.Message.Text;

                        if (ValidateResult(result))
                        {
                            if (result == AddText)
                            {
                                entry.Longboards.Add(entry.Pending);

                                var msg = await client.SendTextMessageAsync(chatId, $"Вы успешно добавили {entry.Pending} лонгборд в корзину!");

                                entry.History.Add(new ChatMessage(msg.MessageId, false));
                            }

                            entry.Pending = null;
                            entry.Stage = Stage.AskingIfShouldContinueAddingToBasket; // 

                            var msg1 = await SendInfoAboutBasket(client, entry);
                            var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                            entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));
                        }

                        break;
                    }

                case Stage.AskingIfShouldContinueAddingToBasket:
                    {
                        var result = update.Message.Text;

                        switch (result)
                        {
                            case YesText:
                                {
                                    var successful = await SendLongBoards(client, entry.Id, entry.History, entry.Longboards);

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
                                    var lbrds = Join(", ", entry.Longboards);
                                    var phone = entry.Phone;
                                    var username = entry.UserName;
                                    var name = entry.Name;
                                    var userChatId = entry.Id;

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

                                    entry.History.AppendMsg(true, msgUserTask.Result);
                                    entry.History.AppendMsg(true, msgUserTask2.Result);

                                    await ClearHistory(entry, client);     
                                    entry.Stage = Stage.ShouldRestartDialog;
                                    await client.SendTextMessageAsync(chatId, "Начать покупки заново?", replyMarkup: RestartKBoard);

                                    break;
                                }
                        }

                        break;

                    }
                case Stage.ShouldRestartDialog:
                    {
                        var text = update.Message.Text;
                        if (text == RestartText)
                        {
                            await StartNewDialog(entry, client);
                        }

                        break;
                    }
            }
        }
    }
}
