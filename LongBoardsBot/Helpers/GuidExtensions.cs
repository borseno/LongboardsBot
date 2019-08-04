using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class GuidExtensions
    {
        public static string ToStringHashTag(this Guid guid)
            => "#" + guid.ToString().Replace('-', '_');
    }
}
