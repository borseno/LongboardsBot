using LongBoardsBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static decimal GetCost(this IEnumerable<BotUserLongBoard> elems)
            => elems.Select(i => i.Longboard.Price * i.Amount).Sum();
    }
}
