//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Utility;

internal static class CollectionHelpers
{
    /// <summary> Commutative, stable, order-independent hashing for collections of collections. </summary>
    public static int GetHashCode<T>(IEnumerable<T>? collection)
    {
        if (collection == null)
        {
            return 0;
        }

        var hashCode = new HashCode();

        foreach (var item in collection)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }

    public static bool Equals<T>(IEnumerable<T>? left, IEnumerable<T>? right, bool orderSignificant = false)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        if (orderSignificant)
        {
            return left.SequenceEqual(right);
        }

        // No multiple enumerations
        var rightArray = right as T[] ?? right.ToArray();
        var leftArray = left as T[] ?? left.ToArray();
        try
        {
            //Many things have natural IComparers defined, but some don't, because no natural comparer exists
            return leftArray.OrderBy(l => l).SequenceEqual(rightArray.OrderBy(r => r));
        }
        catch (Exception)
        {
            //It's not possible to sort some collections of things (like Calendars) in any meaningful way. Properties can be null, and there's no natural
            //ordering for the contents therein. In cases like that, the best we can do is treat them like sets, and compare them. We don't maintain
            //fidelity with respect to duplicates, but it seems better than doing nothing
            var leftSet = new HashSet<T>(leftArray);
            var rightSet = new HashSet<T>(rightArray);
            return leftSet.SetEquals(rightSet);
        }
    }

    public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
    {
        foreach (var element in source)
        {
            destination.Add(element);
        }
    }

    /// <summary>
    /// Merge the given ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// Each input sequence must be ordered according to the given comparer. Duplicates are allowed and will be preserved.
    /// 
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedMerge<T>(this IEnumerable<T> items, IEnumerable<T> other)
        => items.OrderedMerge(other, Comparer<T>.Default);

    /// <summary>
    /// Merge the given ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// Each input sequence must be ordered according to the given comparer. Duplicates are allowed and will be preserved.
    /// 
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedMerge<T>(this IEnumerable<T> items, IEnumerable<T> other, IComparer<T> comparer)
    {
        using var it1 = items.GetEnumerator();
        using var it2 = other.GetEnumerator();

        var has1 = it1.MoveNext();
        var has2 = it2.MoveNext();

        while (has1 || has2)
        {
            var cmp = (has1, has2) switch
            {
                (true, false) => -1,
                (false, true) => 1,
                _ => comparer.Compare(it1.Current, it2.Current)
            };

            if (cmp <= 0)
            {
                yield return it1.Current;
                has1 = it1.MoveNext();
            }
            else
            {
                yield return it2.Current;
                has2 = it2.MoveNext();
            }
        }
    }

    /// <summary>
    /// Merge the given ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// Each input sequence must be ordered according to types default comparer. Duplicates are allowed and will be preserved.
    /// 
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedMergeMany<T>(this IEnumerable<IEnumerable<T>> sequences)
        => OrderedMergeMany(sequences, Comparer<T>.Default);

    /// <summary>
    /// Merge the given ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// Each input sequence must be ordered according to the given comparer. Duplicates are allowed and will be preserved.
    /// 
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedMergeMany<T>(this IEnumerable<IEnumerable<T>> sequences, IComparer<T> comparer)
    {
        var list = (sequences as IList<IEnumerable<T>>) ?? sequences.ToList();
        return OrderedMergeMany(list, 0, list.Count, comparer);
    }

    private static IEnumerable<T> OrderedMergeMany<T>(this IList<IEnumerable<T>> sequences, int offs, int length, IComparer<T> comparer)
    {
        if (length == 0)
            return [];

        if (length == 1)
            return sequences[offs];

        // Compose as a tree to ensure O(N*log(N)) complexity. Composing as a simple chain
        // would result in O(N*N) complexity, which wouldn't be a problem either, as
        // the number of sequences usually is low.
        var mid = length / 2;
        var left = OrderedMergeMany(sequences, offs, mid, comparer);
        var right = OrderedMergeMany(sequences, offs + mid, length - mid, comparer);

        return left.OrderedMerge(right, comparer);
    }

    /// <summary>
    /// Returns the elements of the first ordered sequence that are not present in the second ordered sequence.
    /// </summary>
    /// <remarks>
    /// Both sequences must be ordered according to the type's default comparer.
    ///
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedExclude<T>(this IEnumerable<T> items, IEnumerable<T> exclude)
        => items.OrderedExclude(exclude, Comparer<T>.Default);

    /// <summary>
    /// Returns the elements of the first ordered sequence that are not present in the second ordered sequence.
    /// </summary>
    /// <remarks>
    /// Both sequences must be ordered according to the specified comparer.
    ///
    /// The method operates in a streaming manner, meaning it only enumerates the input sequences while the
    /// output sequence is being enumerated, and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedExclude<T>(this IEnumerable<T> items, IEnumerable<T> exclude, IComparer<T> comparer)
    {
        using var it = items.GetEnumerator();
        using var itEx = exclude.GetEnumerator();

        var hasNextIt = it.MoveNext();
        var hasNextEx = itEx.MoveNext();

        while (hasNextIt)
        {
            var cmp = hasNextEx ? comparer.Compare(it.Current, itEx.Current) : -1;
            if (cmp <= 0)
            {
                if (cmp < 0)
                    yield return it.Current;

                hasNextIt = it.MoveNext();
            }
            else
            {
                hasNextEx = itEx.MoveNext();
            }
        }
    }

    /// <summary>
    /// Returns a sequence containing the items of the ordered input sequence, with duplicates removed.
    /// </summary>
    /// <remarks>
    /// The input sequence must be ordered according to the type's default equality comparer, such
    /// that equal items are adjacent to each other.
    ///
    /// The method operates in a streaming manner, meaning it only enumerates the input sequence while the
    /// output sequence is enumerated and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedDistinct<T>(this IEnumerable<T> items)
        => items.OrderedDistinct(EqualityComparer<T>.Default);

    /// <summary>
    /// Returns a sequence containing the items of the ordered input sequence, with duplicates removed.
    /// </summary>
    /// <remarks>
    /// The input sequence must be ordered according to the type's default equality comparer, such
    /// that equal items are adjacent to each other.
    ///
    /// The method operates in a streaming manner, meaning it only enumerates the input sequence while the
    /// output sequence is enumerated and can therefore handle indefinite sequences.
    /// </remarks>
    public static IEnumerable<T> OrderedDistinct<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer)
    {
        var prev = default(T);
        var first = true;

        foreach (var item in items)
        {
            if (first || !comparer.Equals(prev!, item))
                yield return item;

            prev = item;
            first = false;
        }
    }

    /// <summary>
    /// While iterating the source sequence, catch exceptions of the specified type and call the handler.
    /// </summary>
    public static IEnumerable<T> Catch<T, TException>(this IEnumerable<T> source, Action<TException> handler)
        where TException : Exception
    {
        using var enumerator = source.GetEnumerator();
        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                    break;
            } catch (TException ex)
            {
                handler(ex);
                break;
            }

            yield return enumerator.Current;
        }
    }

    private readonly struct DictKey<T>(T value, long idx)
    {
        public readonly T Value = value;

        /// <summary>
        /// The index is there, because the key of a SortedDictionary or SortedList must be unique.
        /// </summary>
        public readonly long Idx = idx;
    }

    /// <summary>
    /// Merge the given ordered sequence of ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// The inner as well as the outer sequences must be ordered according to the type's default comparer.
    /// The outer sequence must be ordered by the first item of the inner sequences.
    ///
    /// The method operates in a streaming manner on both, the outer as well as the inner sequences,
    /// meaning it only enumerates the input sequences while the output sequence is being enumerated.
    /// Both, the outer as well as the inner sequences may therefore be unbounded.
    ///
    /// The behavior is similar to that of <see cref="OrderedMergeMany{T}(IEnumerable{IEnumerable{T}}, IComparer{T})"/>,
    /// with following differences:
    /// * Additionally to the inner sequences, also the outer sequences must be ordered. They must be ordered
    ///   by the head of the inner sequence according to the given comparer.
    /// * The outer sequence is enumerated in a streaming manner, only as far as the outer is enumerated.
    /// </remarks>
    public static IEnumerable<T> OrderedNestedMergeMany<T>(this IEnumerable<IEnumerable<T>> sequences)
        => OrderedNestedMergeMany(sequences, Comparer<T>.Default);

    /// <summary>
    /// Merge the given ordered sequence of ordered sequences into a single ordered sequence.
    /// </summary>
    /// <remarks>
    /// The inner as well as the outer sequences must be ordered according to the given comparer.
    /// The outer sequence must be ordered by the first item of the inner sequences.
    ///
    /// The method operates in a streaming manner on both, the outer as well as the inner sequences,
    /// meaning it only enumerates the input sequences while the output sequence is being enumerated.
    /// Both, the outer as well as the inner sequences may therefore be unbounded.
    ///
    /// The behavior is similar to that of <see cref="OrderedMergeMany{T}(IEnumerable{IEnumerable{T}}, IComparer{T})"/>,
    /// with following differences:
    /// * Additionally to the inner sequences, also the outer sequences must be ordered. They must be ordered
    ///   by the head of the inner sequence according to the given comparer.
    /// * The outer sequence is enumerated in a streaming manner, only as far as the outer is enumerated.
    /// </remarks>
    public static IEnumerable<T> OrderedNestedMergeMany<T>(this IEnumerable<IEnumerable<T>> sequences, IComparer<T> comparer)
    {
        using var outer = sequences.GetEnumerator();
        var keyComparer = Comparer<DictKey<T>>.Create((a, b) =>
        {
            var res = comparer.Compare(a.Value, b.Value);
            if (res != 0) return res;
            return a.Idx.CompareTo(b.Idx);
        });

        var active = new SortedDictionary<DictKey<T>, IEnumerator<T>>(keyComparer);

        var pending = NextNonEmpty();
        if (pending == null)
            yield break;

        try
        {
            long uniqueIdx = 0;
            while ((active.Count > 0) || (pending != null))
            {
                KeyValuePair<DictKey<T>, IEnumerator<T>> first;
                if (active.Count == 0)
                {
                    first = new(new(pending!.Current, uniqueIdx++), pending);
                    active.Add(first.Key, first.Value);
                    pending = NextNonEmpty();
                }
                else
                {
                    first = active.First();

                    if ((pending != null) && (comparer.Compare(pending.Current, first.Value.Current) < 0))
                    {
                        first = new(new(pending.Current, uniqueIdx++), pending);
                        active.Add(first.Key, first.Value);
                        pending = NextNonEmpty();
                    }
                }

                yield return first.Key.Value;

                active.Remove(first.Key);
                if (first.Value.MoveNext())
                    active.Add(new(first.Value.Current, uniqueIdx++), first.Value);
                else
                    first.Value.Dispose();
            }
        }
        finally
        {
            foreach (var enumerator in active.Values)
                enumerator.Dispose();

            pending?.Dispose();
        }

        IEnumerator<T>? NextNonEmpty()
        {
            while (outer.MoveNext())
            {
                var enumerator = outer.Current?.GetEnumerator();
                if (enumerator?.MoveNext() == true)
                    return enumerator;

                enumerator?.Dispose();
            }

            return null;
        }
    }
}
