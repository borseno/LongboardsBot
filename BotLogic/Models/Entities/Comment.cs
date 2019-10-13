using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public BotUser Author { get; set; }

        [Required] // todo: length constraint
        public string Data { get; set; }
    }
}