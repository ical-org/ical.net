//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class GroupedListTests
{
    [Test]
    public void IndexerAndIndexOfMatchEnumerationOrderAcrossGroups()
    {
        var cal = CalendarWithThreeChildren();
        var list = (IList<ICalendarObject>) cal.Children;
        var enumerated = list.ToList();

        Assert.That(enumerated.Select(x => x.Name), Is.EqualTo(new[] { "VEVENT", "VEVENT", "VTODO" }));

        using (Assert.EnterMultipleScope())
        {
            for (var i = 0; i < enumerated.Count; i++)
            {
                Assert.That(list[i], Is.SameAs(enumerated[i]), $"list[{i}]");
                Assert.That(list.IndexOf(enumerated[i]), Is.EqualTo(i), $"IndexOf item {i}");
            }
        }
    }

    [Test]
    public void RemoveAtRemovesItemFromLaterGroup()
    {
        var cal = CalendarWithThreeChildren();
        var list = (IList<ICalendarObject>) cal.Children;
        var todo = cal.Todos.Single();

        list.RemoveAt(list.IndexOf(todo));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(cal.Todos, Is.Empty);
            Assert.That(cal.Events, Has.Count.EqualTo(2));
        }
    }

    private static Calendar CalendarWithThreeChildren()
    {
        var cal = new Calendar();
        cal.Events.Add(new CalendarEvent { Summary = "a" });
        cal.Todos.Add(new Todo { Summary = "t" });
        cal.Events.Add(new CalendarEvent { Summary = "b" });
        return cal;
    }
}
