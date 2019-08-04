using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class UserStatistics
    {
        public long BotUserId { get; set; }
        public BotUser BotUser { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }
    }
}