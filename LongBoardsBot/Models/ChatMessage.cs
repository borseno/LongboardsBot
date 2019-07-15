namespace LongBoardsBot.Models
{
    public class ChatMessage
    {
        public int MsgId { get; }
        public bool IgnoreDelete { get; }

        public ChatMessage(int msgId, bool ignoreDelete)
        {
            MsgId = msgId;
            IgnoreDelete = ignoreDelete;
        }
    }
}
