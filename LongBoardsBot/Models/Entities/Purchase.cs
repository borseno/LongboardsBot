using System;
using System.Collections.Generic;

namespace LongBoardsBot.Models.Entities
{
    public class Purchase
    {
        public Guid Guid { get; set; }

        public IList<BotUserLongBoard> Basket { get; set; } 

        public decimal Cost { get; set; }

        public bool Delivered { get; set; }
    }
}