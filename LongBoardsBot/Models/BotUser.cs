using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LongBoardsBot.Models
{
    public class BotUser
    {
        public long ChatId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Phone { get; set; }

        public string UserName { get; set; }

        [Required]
        public Stage Stage { get; set; }

        public IList<BotUserLongBoard> BotUserLongBoards { get; set; }

        public LongBoard Pending { get; set; }

        public IList<ChatMessage> History { get; set; }

        public List<LongBoard> GetBasket()
            => BotUserLongBoards.Select(i => i.Longboard).ToList();
    }

    public class BotUserLongBoard
    {
        public long BotUserId { get; set; }
        public int LongboardId { get; set; }
        public LongBoard Longboard { get; set; }
        public BotUser BotUser { get; set; }
    }
}