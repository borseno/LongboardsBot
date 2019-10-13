using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LongBoardsBot.Helpers.RuntimeInformationExtensions;

namespace LongBoardsBot.Helpers
{
    public static class DateTimeExtensions
    {
        private const string KharkivWindowsTimeZoneName = @"E. Europe Standard Time";

        public static DateTime GetNowKharkiv()
        {
            var timeZone = GetTimeZoneFromWindowsName(KharkivWindowsTimeZoneName);

            var now = DateTime.UtcNow;
 
            var result = TimeZoneInfo.ConvertTime(now, timeZone);

            return result;
        }
        
        public static bool IsCoffeeSpaceOpen(this DateTime time)
        {
            return time.Hour >= 8 && time.Hour <= 22;
        }
    }
}
