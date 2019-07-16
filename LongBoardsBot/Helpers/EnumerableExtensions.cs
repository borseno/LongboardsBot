using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> act)
        {
            foreach (var i in enumerable)
            {
                act(i);
            }
        }
    }
}
