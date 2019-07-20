namespace LongBoardsBot.Models.Entities
{
    public interface IChatMessage
    {
        int MessageId { get; set; }
        bool IgnoreDelete { get; set; }
    }
}