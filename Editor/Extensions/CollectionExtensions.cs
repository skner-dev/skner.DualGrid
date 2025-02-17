using System;
using System.Collections.Generic;
using System.Linq;

namespace skner.DualGrid.Editor.Extensions
{
    public static class CollectionExtensions
    {

        /// <summary>
        /// Checks if the values of a specific field differ among items in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="collection"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static bool HasDifferentValues<T, TField>(this IEnumerable<T> collection, Func<T, TField> selector)
        {
            if (collection == null || !collection.Any())
                return false;

            var firstValue = selector(collection.First());

            return collection.Skip(1).Any(item => !EqualityComparer<TField>.Default.Equals(selector(item), firstValue));
        }

    }
}
