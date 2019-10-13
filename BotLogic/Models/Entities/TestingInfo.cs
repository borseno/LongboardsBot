using System;
using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class TestingInfo
    {

        public long BotUserId { get; set; }
        [Required]
        public BotUser BotUser { get; set; }

        public DateTime VisitDateTime { get; set; }
        public bool Occurred { get; set; }
    }
}