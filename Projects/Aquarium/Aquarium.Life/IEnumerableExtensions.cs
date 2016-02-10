using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    static class IEnumerableExtensions
    {
        public static IEnumerable<T> Circular<T>(this IEnumerable<T> coll)
        {
            while (true)
            {
                foreach (T t in coll)
                    yield return t;
            }
        }

        public static T Next<T>(this IEnumerator<T> enumerator)
        {
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}
