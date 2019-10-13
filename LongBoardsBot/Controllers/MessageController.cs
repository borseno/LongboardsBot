using System;
using System.Linq;
using System.Threading.Tasks;
using BotLogic;
using LongBoardsBot.Models;
using LongBoardsBot.Models.Handlers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LongBoardsBot.Controllers
{
    [Route("")]
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly BotRequestHandler handler;

        public MessageController(BotRequestHandler handler)
        {
            this.handler = handler;
        }

        [Route("")]
        [Route("Update")]
        [Route("[controller]/[action]")]
        [HttpPost]
        public async Task<ActionResult> Update([FromBody]Update update)
        {
            await handler.HandleRequest(update);

            return new OkResult();
        }
    }
}
