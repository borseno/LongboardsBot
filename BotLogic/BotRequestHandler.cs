using LongBoardsBot.Models.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static LongBoardsBot.Models.Constants;

namespace BotLogic
{
    public class BotRequestHandler
    {
        private readonly StageHandler stageHandler;
        private readonly CallbackHandler callbackHandler;
        private readonly TelegramBotClient bot;

        public BotRequestHandler(StageHandler stageHandler, CallbackHandler callbackHandler, TelegramBotClient bot)
        {
            this.stageHandler = stageHandler;
            this.callbackHandler = callbackHandler;
            this.bot = bot;
        }

        public async Task HandleRequest(Update update)
        {
            try
            {
                if (update.Message?.Chat?.Id == AdminGroupChatId)
                    return;

                if (update.Message?.Photo != null)
                {
                    var id = update.Message.Photo.First().FileId;

                    var t2 = bot.SendTextMessageAsync(update.Message.Chat.Id, id);

                    await Task.WhenAll(t2);
                }
                else if (update.CallbackQuery != null)
                {
                    await callbackHandler.HandleCallback(bot, update.CallbackQuery);
                }
                else if (update.Message != null)
                {
                    await stageHandler.HandleMessage(bot, update.Message);
                }

                return;
            }
            catch (Exception e)
            {
                await bot.SendTextMessageAsync(BugReportChatId, e.ToString());
            }
        }
    }
}
