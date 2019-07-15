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
                        var msg = await UpdateUsersPhoneAndUsername(client, update.Message, storage);
                        entry.History.Add(new ChatMessage(msg.MessageId, false));

                        await SendLongBoards(client, update.Message.Chat.Id, entry.History);
                        entry.Stage = Stage.ProcessingLongboardsKeyboardInput;

                        break;
                    }

                case Stage.ProcessingLongboardsKeyboardInput:
                    {
                        var lb = update.Message.Text;

                        var isLongBoard = ValidateLongBoard(lb);

                        if (isLongBoard)
                        {
                            entry.Pending = lb;

                            // TODO: add here sending more info about lb
                            var msg = await SendShouldAddToBasketKeyboard(client, chatId, lb);

                            entry.History.Add(new ChatMessage(msg.MessageId, false));

                            entry.Stage = Stage.ProcessingBasketKeyboardInput;
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

                            var msg1 = await SendInfoAboutBasket(client, entry);
                            var msg2 = await SendShouldContinueAddingToBasket(client, entry);

                            entry.History.AddRange(new[] { msg1, msg2 }.Select(i => new ChatMessage(i.MessageId, false)));

                            entry.Stage = Stage.AskingIfShouldContinueAddingToBasket;
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
                                    var lbrds = Join(", ", entry.Longboards);
                                    var phone = entry.Phone;
                                    var username = entry.UserName;
                                    var name = entry.Name;
                                    var userChatId = entry.Id;

                                    var msgToAdminGroup =
                                        $"@{username} хочет купить {{{lbrds}}}{Environment.NewLine}" +
                                        $"Имя: {name}, Телефон: {phone}";

                                    var msgToUser = // TODO
                                        $"Вы купили {lbrds}. Стоимость = 100 долларов. " +
                                        $"Деньги отдать наличкой в бро кофе. " +
                                        $"Метро защитников украины, г. Харьков. Дима встретит";

                                    var msgUserTask = client.SendTextMessageAsync(userChatId, msgToUser);
                                    var msgAdminTask = client.SendTextMessageAsync(AdminGroupChatId, msgToAdminGroup);

                                    await Task.WhenAll(msgUserTask, msgAdminTask);

                                    entry.History.Add(new ChatMessage(msgUserTask.Result.MessageId, true));

                                    await RestartDialog(entry, client);
                                    break;
                                }
                        }

                        break;
                    }
            }
        }
    }
}
