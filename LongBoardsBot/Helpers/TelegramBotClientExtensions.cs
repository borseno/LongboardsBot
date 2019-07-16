using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LongBoardsBot.Helpers
{
    public static class TelegramBotClientExtensions
    {
        public static Task<Message> AskName(this TelegramBotClient client, long chatId)
    => client.SendTextMessageAsync(chatId, "Здравствуйте! Введите ваше имя");

        public static Task<Message> AskPhone(this TelegramBotClient client, long chatId, bool hasUserName)
            => client.SendTextMessageAsync(
                chatId,
                hasUserName ? "Введите контактный номер телефона (по желанию)"
                : "У вас нет username в телеграме. Введите ваш номер телефона, иначе мы не сможем с вами связаться");
    }
}
