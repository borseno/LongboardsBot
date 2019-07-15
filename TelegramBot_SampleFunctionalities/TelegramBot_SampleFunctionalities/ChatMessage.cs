using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot_SampleFunctionalities
{
    public struct ChatMessage
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
