using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace LongBoardsBot.Models
{
    public static class ChatMessageListExtensions
    {
        public static List<ChatMessage> AppendMsg(this List<ChatMessage> chatMessages, bool ignoreDeleted, IEnumerable<Message> messages)
        {
            chatMessages.AddRange(messages.Select(i => new ChatMessage(i.MessageId, ignoreDeleted)));
            return chatMessages;
        }

        public static List<ChatMessage> AppendMsg(this List<ChatMessage> chatMessages, bool ignoreDeleted, Message message)
        {
            chatMessages.Add(new ChatMessage(message.MessageId, ignoreDeleted));
            return chatMessages;
        }
    }
}
