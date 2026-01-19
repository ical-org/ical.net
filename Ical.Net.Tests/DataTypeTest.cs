//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class DataTypeTest
{
    [Test, Category("DataType")]
    public void OrganizerConstructorMustAcceptNull()
        => Assert.DoesNotThrow(() => { _ = new Organizer(null); });

    [Test, Category("DataType")]
    public void AttachmentConstructorMustAcceptNull()
    {
        Assert.DoesNotThrow(() => { _ = new Attachment((byte[]?) null); });
        Assert.DoesNotThrow(() => { _ = new Attachment((string?) null); });
    }

    public static IEnumerable<TestCaseData> TestWeekDayEqualsTestCases => [
        new(new WeekDay("MO"), new WeekDay("1MO")),
        new(new WeekDay("1TU"), new WeekDay("-1TU")),
        new(new WeekDay("2WE"), new WeekDay("-2WE")),
        new(new WeekDay("TH"), new WeekDay("FR")),
        new(new WeekDay("-5FR"), new WeekDay("FR")),
        new(new WeekDay("SA"), null),
    ];

    [Test, TestCaseSource(nameof(TestWeekDayEqualsTestCases)), Category("DataType")]
    public void TestWeekDayEquals(WeekDay w1, WeekDay w2)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(w1.Equals(w1), Is.True);
            Assert.That(w1.Equals(w2), Is.False);
        }
    }

    [Test, TestCaseSource(nameof(TestWeekDayEqualsTestCases)), Category("DataType")]
    public void TestWeekDayCompareTo(WeekDay w1, WeekDay w2)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(w1.CompareTo(w1), Is.EqualTo(0));

            if (w2 != null)
                Assert.That(w1.CompareTo(w2), Is.Not.EqualTo(0));
        }
    }
}
