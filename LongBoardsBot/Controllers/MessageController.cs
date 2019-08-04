using System;
using System.Linq;
using System.Threading.Tasks;
using LongBoardsBot.Models;
using LongBoardsBot.Models.Handlers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly StageHandler stageHandler;
        private readonly CallbackHandler callbackHandler;
        private readonly TelegramBotClient bot;

        public MessageController(StageHandler stageHandler, CallbackHandler callbackHandler)
        {
            this.stageHandler = stageHandler;
            this.callbackHandler = callbackHandler;
            bot = Bot.Get();
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> Update([FromBody]Update update)
        {
            try
            {
                if (update.Message?.Chat?.Id == AdminGroupChatId)
                    return Ok();

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

                return Ok();
            }
            catch (Exception e)
            {
                var nl = Environment.NewLine;

                await bot.SendTextMessageAsync(BugReportChatId, 
                    e.GetType().Name + nl + e?.Message + nl + nl + e?.InnerException?.Message + nl + nl
                    + e?.StackTrace);

                return Ok();
            }
        }
    }
}
