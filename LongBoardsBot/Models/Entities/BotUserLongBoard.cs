using LongBoardsBot.Models.Entities;
using System.ComponentModel.DataAnnotations;
using static System.String;
using static LongBoardsBot.Models.Constants;

namespace LongBoardsBot.Models
{
    public class BotUserLongBoard
    {
        public int Id { get; set; }

        public long BotUserId { get; set; }
        public int LongboardId { get; set; }
        public LongBoard Longboard { get; set; }
        public BotUser BotUser { get; set; }

        [Range(0, int.MaxValue)]
        public int Amount { get; set; }

        public override string ToString()
            => Format(LBInBasketInfo, Longboard.Style, Amount);      
    }
}