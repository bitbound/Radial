using System.Collections.Generic;
using System.Linq;

namespace Radial.Utilities
{
    public static class IEnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        {
            for (var i = 0; i < collection.Count(); i++)
            {
                if (collection.ElementAt(i).Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
