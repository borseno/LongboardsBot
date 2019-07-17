using LongBoardsBot.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models
{
    public class BotUserLongBoard
    {
        public long BotUserId { get; set; }
        public int LongboardId { get; set; }
        public LongBoard Longboard { get; set; }
        public BotUser BotUser { get; set; }

        [Range(0, int.MaxValue)]
        public int Amount { get; set; }
    }
}