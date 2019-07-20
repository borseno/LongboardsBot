using System.ComponentModel.DataAnnotations;

namespace LongBoardsBot.Models.Entities
{
    public class ChatMessage : IChatMessage
    {
        public ChatMessage(int msgId, bool ignoreDelete)
        {
            MessageId = msgId;
            IgnoreDelete = ignoreDelete;
        }

        public ChatMessage()
        {

        }

        public int MessageId { get; set; }
        public bool IgnoreDelete { get; set; }

        [Required]
        public BotUser User { get; set; }
    }
}
