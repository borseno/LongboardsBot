using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using static LongBoardsBot.Models.Constants;
using static LongBoardsBot.Models.StageHandling;

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

            if (update.Message.Chat.Id == AdminGroupChatId)
                return Ok();

            await HandleUpdate(Bot, update);

            return Ok();
        }
    }
}
