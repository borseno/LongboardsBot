using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LongBoardsBot.Helpers
{
    public static class CollectionExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> toAdd)
        {
            toAdd.ForEach(i => collection.Add(i));
            return collection;
        }
        
    }
}
