using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class Purchase
    {
        public Guid Guid { get; set; }

        [Required]
        public BotUser BotUser { get; set; }

        public IList<BotUserLongBoard> Basket { get; set; } 

        public decimal Cost { get; set; }

        public bool Delivered { get; set; }
    }
}