using System.Collections.Generic;

namespace LongBoardsBot.Models
{
    internal class BotUser
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string UserName { get; set; }

        public Stage Stage { get; set; }

        public List<string> Longboards { get; set; } = new List<string>(8);

        public string Pending { get; set; }

        public List<ChatMessage> History { get; set; } = new List<ChatMessage>(16);
    }
}