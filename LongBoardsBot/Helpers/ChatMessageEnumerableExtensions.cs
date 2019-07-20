using LongBoardsBot.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace LongBoardsBot.Helpers
{
    public static class ChatMessageExtensions
    {
        public static ICollection<ChatMessage> AddMessage(this ICollection<ChatMessage> collection, Message toAdd, bool ignoreDelete)
        {
            var chatMsg = new ChatMessage
            {
                MessageId = toAdd.MessageId,
                IgnoreDelete = ignoreDelete
            };

            collection.Add(chatMsg);

            return collection;
        }

        public static ICollection<ChatMessage> AddMessages(this ICollection<ChatMessage> collection, IEnumerable<Message> toAdd, bool ignoreDelete)
        {
            collection.AddRange(toAdd.Select(i => new ChatMessage(i.MessageId, ignoreDelete)));

            return collection;
        }
    }
}
