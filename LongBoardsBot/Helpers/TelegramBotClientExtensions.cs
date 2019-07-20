using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Helpers
{
    public static class TelegramBotClientExtensions
    {
        public static Task<Message> AskName(this TelegramBotClient client, long chatId)
    => client.SendTextMessageAsync(chatId, EnterYourNameText);

        public static Task<Message> AskPhone(this TelegramBotClient client, long chatId)
            => client.SendTextMessageAsync(chatId, EnterYourPhoneText);
    }
}
