using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LongBoardsBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private TelegramBotClient Bot { get; } = Models.Bot.Get();

        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> Update([FromBody]Update update)
        {
            if (update?.Message?.Chat == null)
                return Ok();

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "what's up!");

            return Ok();
        }
    }
}
