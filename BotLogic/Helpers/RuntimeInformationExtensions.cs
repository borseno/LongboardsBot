using System;
using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace LongBoardsBot.Helpers
{
    public class RuntimeInformationExtensions
    {
        public static TimeZoneInfo GetTimeZoneFromWindowsName(string windowsName)
        {
            TimeZoneInfo result;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result = TimeZoneInfo.FindSystemTimeZoneById(windowsName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string linux = TZConvert.WindowsToIana(windowsName);

                result = TimeZoneInfo.FindSystemTimeZoneById(linux);
            }
            else
            {
                throw new NotImplementedException("Other OS aren't supported. OS was: " + RuntimeInformation.OSDescription);
            }

            return result;
        }
    }
}
