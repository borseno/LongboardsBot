using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class BotUser
    {
        public long ChatId { get; set; }

        public int? UserId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string UserName { get; set; }

        public bool IsLivingInKharkiv { get; set; }

        public State State { get; set; } 

        [Required]
        public Stage Stage { get; set; }

        public StatisticsStage StatisticsStage { get; set; }
        public bool IsOneTimeStatistics { get; set; }

        public IList<Purchase> Purchases { get; set; }

        public IList<ChatMessage> History { get; set; }

        public IList<Comment> Comments { get; set; }

        public Purchase CurrentPurchase { get; set; }

        public TestingInfo TestingInfo { get; set; }

        public LongBoard Pending { get; set; }

        public UserStatistics StatisticsInfo { get; set; }
    }
}