using LongBoardsBot.Models.Entities;

namespace LongBoardsBot.Models
{
    public class BotUserLongBoard
    {
        public long BotUserId { get; set; }
        public int LongboardId { get; set; }
        public LongBoard Longboard { get; set; }
        public BotUser BotUser { get; set; }
    }
}