//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

internal class CollectionHelpersTests
{
    private static readonly DateTime _now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    private static List<PeriodList> GetExceptionDates()
        => new List<PeriodList> { new PeriodList { new Period(new CalDateTime(_now.AddDays(1).Date)) } };

    [Test]
    public void ExDateTests()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetExceptionDates(), Is.Not.Null);
            Assert.That(GetExceptionDates(), Is.Not.EqualTo(null));
        }

        var changedPeriod = GetExceptionDates();
        changedPeriod[0][0] = new Period(new CalDateTime(_now.AddHours(-1)), changedPeriod[0][0].EndTime);

        Assert.That(changedPeriod, Is.Not.EqualTo(GetExceptionDates()));
    }

    [TestCase(new[] { 1, 3, 5, 7 }, new[] { 2, 4, 6 }, new[] { 1, 2, 3, 4, 5, 6, 7 })]
    [TestCase(new int[] { }, new int[] { }, new int[] { })]
    [TestCase(new int[] { }, new[] { 2, 4, 6 }, new[] { 2, 4, 6 })]
    [TestCase(new[] { 2, 4, 6 }, new int[] { }, new[] { 2, 4, 6 })]
    [TestCase(new[] { 3, 4 }, new[] { 1, 2 }, new[] { 1, 2, 3, 4 })]
    [TestCase(new[] { 1, 2, 3 }, new[] { 2, 3, 4 }, new[] { 1, 2, 2, 3, 3, 4 })]
    public void TestMerge(IList<int> seq1, IList<int> seq2, IList<int> expected)
    {
        var result = seq1.OrderedMerge(seq2).ToList();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(new int[] { }, new int[] { }, new int[] { })]
    [TestCase(new int[] { }, new[] { 2, 4, 6 }, new int[] { })]
    [TestCase(new[] { 2, 4, 6 }, new int[] { }, new[] { 2, 4, 6 })]
    [TestCase(new[] { 1, 2, 3, 5, 6, 7 }, new[] { 2, 4, 6 }, new[] { 1, 3, 5, 7 })]
    public void TestMergeExclude(IList<int> seq, IList<int> exclude, IList<int> expected)
    {
        var result = seq.OrderedExclude(exclude).ToList();

        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<int> GetNaturalNumbers()
    {
        var i = 1;
        while (true)
            yield return i++;
    }

    [Test]
    public void TestMergeIndefinite()
    {
        var result = GetNaturalNumbers().Select(x => x * 3).OrderedMerge(GetNaturalNumbers().Select(x => x * 2))
            .Take(7);
        Assert.That(result, Is.EqualTo(new[] { 2, 3, 4, 6, 6, 8, 9 }));
    }

    [Test]
    public void TestMergeExcludeIndefinite()
    {
        var result = GetNaturalNumbers().Select(x => x * 3).OrderedExclude(GetNaturalNumbers().Select(x => x * 2))
            .Take(4);
        Assert.That(result, Is.EqualTo(new[] { 3, 9, 15, 21 }));
    }

    [Test]
    public void TestMergeMulti()
    {
        var result = CollectionHelpers.OrderedMergeMany([[4], [2], [3], [1]], Comparer<int>.Default);
        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void TestOrderedDistinct()
    {
        var result = new[] { 1, 2, 2, 3 }.OrderedDistinct();
        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void TestOrderedNestedMergeMany()
    {
        var result = new[]
        {
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 3, 6, 9 },
        }.OrderedNestedMergeMany(Comparer<int>.Default);

        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
    }

    [Test]
    public void TestOrderedNestedMergeMany_AllowsDuplicates()
    {
        var result = new[]
        {
            new[] { 1, 2, 2, 4 },
            new[] { 1, 2, 3, 4 },
        }.OrderedNestedMergeMany(Comparer<int>.Default);

        Assert.That(result, Is.EqualTo(new[] { 1, 1, 2, 2, 2, 3, 4, 4 }));
    }

    [Test]
    public void TestOrderedNestedMergeMany_HandlesEmptyInnerSequences()
    {
        IEnumerable<IEnumerable<int>> withEmpty =
        [
            [],
            [1, 3],
            [2],
        ];

        var result = withEmpty
            .OrderedNestedMergeMany(Comparer<int>.Default)
            .ToList();

        Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void TestOrderedNestedMergeMany_StreamsOuterSequence(bool increasingOuter, bool increasingInner)
    {
        IEnumerable<int> GetIndefiniteSequence(int start)
        {
            var i = start;
            while (true)
            {
                yield return i;
                i += increasingInner ? 1 : 0;
            }
        }

        IEnumerable<IEnumerable<int>> GetOrderedInnerSequences()
        {
            var i = 0;
            while (true)
            {
                yield return GetIndefiniteSequence(i);
                i += increasingOuter ? 1 : 0;
            }
        }

        var result = GetOrderedInnerSequences()
            .OrderedNestedMergeMany(Comparer<int>.Default)
            .Take(10)
            .ToList();

        var expected = increasingOuter && increasingInner
            ? new[] { 0, 1, 1, 2, 2, 2, 3, 3, 3, 3 }
            : new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestOrderedNestedMergeMany_PrefersVerticalOverHorizontalEnumeration()
    {
        IEnumerable<int> Seq1()
        {
            yield return 0;
            while (true) yield return 1;
        }

        IEnumerable<int> Seq2()
        {
            // This will be returned in the final sequence, as it is less than the indefinite sequence of 1s.
            yield return 0;

            // This value will be returned by this sequence but not to the final sequence, as
            // iteration of should go into the depth first, so returning all the (indefinite) 1s from
            // the first sequence first before continuing there.
            // It is relevant that the iteration first happens into the depth and only then into the breadth,
            // because otherwise the operator would have to maintain more IEnumerators than needed,
            // which would increase memory pressure.
            yield return 1;
            Assert.Fail();
        }

        IEnumerable<int> Seq3()
        {
            // This item will be queried but the sequence will not be considered active as long as the first
            // value is not returned in the final sequence, which will never happen.
            yield return 1;
            Assert.Fail();
        }

        IEnumerable<int> Seq4()
        {
            // The final sequence shouldn't be queried at all, because no values are
            // being returned from the previous.
            Assert.Fail();
            yield break;
        }

        var result =
            new[] { Seq1(), Seq2(), Seq3(), Seq4() }
            .OrderedNestedMergeMany()
            .Take(10)
            .ToList();

        Assert.That(result, Is.EqualTo(new[] { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 }));
    }

    [Test]
    public void TestOrderedNestedMergeMany_SubsequenceOnlyEnumeratedWhenNeeded()
    {
        IEnumerable<int> FailingSeq()
        {
            Assert.Fail();
            yield break;
        }

        var result =
            new[] { [1], FailingSeq() }
            .OrderedNestedMergeMany()
            .Take(1)
            .ToList();

        Assert.That(result, Is.EqualTo(new[] { 1 }));
    }
}
