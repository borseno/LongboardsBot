using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public enum SocialStatus
    {
        Unknown,
        Employeed,
        Studying
    }

    public class UserStatistics
    {
        public long BotUserId { get; set; }
        public BotUser BotUser { get; set; }

        [Range(0, 150)]
        public int Age { get; set; }

        public SocialStatus SocialStatus { get; set; }
    }
}