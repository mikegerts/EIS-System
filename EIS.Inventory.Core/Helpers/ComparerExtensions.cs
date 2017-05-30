using System;
using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Core.Helpers
{
    public static class ComparerExtensions
    {
        /// <summary>
        /// Produces the se difference of two sequences by using the specified 
        /// System.Collections.Generic.IEqualityComparer<T> to compare values
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences</typeparam>
        /// <param name="first">An System.Collections.Generic.IEnumerable<T> whose elements that are not also in second will be returned.</param>
        /// <param name="second">An System.Collections.Generic.IEnumerable<T> whose elements that also occur in the first sequence will cause those elements to be removed from the returned sequence.</param>
        /// <param name="comparer"> An Func<TSource, TSource, bool> to compare values.</param>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Except(second, new LamdaComparer<TSource>(comparer));
        }
    }

    public class LamdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lamdaComaparer;
        private readonly Func<T, int> _lamdaHash;

        public LamdaComparer(Func<T, T, bool> lamdaComparer)
            : this(lamdaComparer, o => 0)
        {
        }

        public LamdaComparer(Func<T, T, bool> lamdaComparer, Func<T, int> lamdaHash)
        {
            if (lamdaComparer == null)
                throw new ArgumentException("lamdaComparer");
            if (lamdaHash == null)
                throw new ArgumentException("lamdaHash");

            _lamdaComaparer = lamdaComparer;
            _lamdaHash = lamdaHash;
        }

        public bool Equals(T x, T y)
        {
            return _lamdaComaparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 0; // obj.GetHashCode();
        }
    }
}
