using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class LongBoard
    {
        public int Id { get; set; }

        [Required]
        public string Style { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Amount { get; set; }

        public IList<BotUserLongBoard> BotUserLongBoards { get; set; }
    }
}