using System;
using System.Collections.Generic;
using System.Linq;

namespace TrendAudioFromSpotify.UI.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            return ExceptBy(first, second, keySelector, null);
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var keys = new HashSet<TKey>(second.Select(keySelector), keyComparer);
                foreach (var element in first)
                {
                    var key = keySelector(element);
                    if (keys.Contains(key))
                        continue;

                    yield return element;
                    keys.Add(key);
                }
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rnd = new Random();

            for (var i = list.Count; i > 0; i--)
                list.Swap(0, rnd.Next(0, i));

            for (var i = list.Count; i > 0; i--)
                list.Swap(0, rnd.Next(0, i));
        }

        internal static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
