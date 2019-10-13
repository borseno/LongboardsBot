using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot
{
    public class BotSettings
    {
        public string WebHookUrl { get; set; }
        public string NickName { get; set; } // nickname (the one that starts with @)
        public string ApiKey { get; set; }
        public long AdminGroupChatId { get; set; }
        public long BugReportChatId { get; set; }
    }
}
