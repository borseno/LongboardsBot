using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static LongBoardsBot.Constants;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

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
              
            var text = update.Message.Text;
            var chatId = update.Message.Chat.Id;

            if (text == "/start")
            {
                await Bot.SendTextMessageAsync(chatId, "Здравствуйте! Введите, пожалуйста, ваше имя:");
            }

            var url = LongBoardDirectory + "1";
            var image = new Bitmap(url);

            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);

                ms.Position = 0;

                await Bot.SendPhotoAsync(chatId, new InputOnlineFile(ms), "Choose longboard!");
            }

            return Ok();
        }
    }
}
