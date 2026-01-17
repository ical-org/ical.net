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
}
