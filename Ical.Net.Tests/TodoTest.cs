//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class TodoTest
{
    private const string _tzid = "America/New_York";

    [Test, TestCaseSource(nameof(ActiveTodo_TestCases)), Category("Todo")]
    public void ActiveTodo_Tests(string calendarString, bool isActive)
    {
        var iCal = Calendar.Load(calendarString)!;
        ProgramTest.TestCal(iCal);
        var todo = iCal.Todos;

        Assert.That(todo[0]!.IsActive, Is.EqualTo(isActive));
    }

    private static IEnumerable<TestCaseData> ActiveTodo_TestCases()
    {
        yield return new TestCaseData(IcsFiles.Todo1, true).SetName("Todo1");
        yield return new TestCaseData(IcsFiles.Todo2, true).SetName("Todo2");
        yield return new TestCaseData(IcsFiles.Todo3, false).SetName("Todo3");
        yield return new TestCaseData(IcsFiles.Todo5, false).SetName("Todo5");
        yield return new TestCaseData(IcsFiles.Todo6, false).SetName("Todo6");
        yield return new TestCaseData(IcsFiles.Todo7, false).SetName("Todo7");
        yield return new TestCaseData(IcsFiles.Todo8, false).SetName("Todo8");
        yield return new TestCaseData(IcsFiles.Todo9, false).SetName("Todo9");
    }

    [Test, TestCaseSource(nameof(CompletedTodo_TestCases)), Category("Todo")]
    public void CompletedTodo_Tests(string calendarString, bool isCompleted)
    {
        var iCal = Calendar.Load(calendarString)!;
        ProgramTest.TestCal(iCal);
        var todo = iCal.Todos;

        Assert.That(todo[0]!.IsCompleted, Is.EqualTo(isCompleted));
    }

    private static IEnumerable<TestCaseData> CompletedTodo_TestCases()
    {
        yield return new TestCaseData(IcsFiles.Todo4, true).SetName("Todo4");
    }

    [Test, Category("Todo")]
    public void Todo7_1()
    {
        var iCal = Calendar.Load(IcsFiles.Todo7)!;
        var todo = iCal.Todos;

        var items = new List<CalDateTime>
        {
            new CalDateTime(2006, 7, 28, 9, 0, 0, _tzid),
            new CalDateTime(2006, 8, 4, 9, 0, 0, _tzid),
            new CalDateTime(2006, 9, 1, 9, 0, 0, _tzid),
            new CalDateTime(2006, 10, 6, 9, 0, 0, _tzid),
            new CalDateTime(2006, 11, 3, 9, 0, 0, _tzid),
            new CalDateTime(2006, 12, 1, 9, 0, 0, _tzid),
            new CalDateTime(2007, 1, 5, 9, 0, 0, _tzid),
            new CalDateTime(2007, 2, 2, 9, 0, 0, _tzid),
            new CalDateTime(2007, 3, 2, 9, 0, 0, _tzid),
            new CalDateTime(2007, 4, 6, 9, 0, 0, _tzid)
        }.Select(x => x.ToZonedDateTime(_tzid)).ToList();

        var occurrences = todo[0]!.GetOccurrences(
            new CalDateTime(2006, 7, 1, 9, 0, 0).ToZonedDateTime(_tzid))
            .TakeWhileBefore(new CalDateTime(2007, 7, 1, 9, 0, 0).ToZonedDateTime(_tzid).ToInstant()).ToList();

        Assert.That(
            occurrences,
            Has.Count.EqualTo(items.Count));
    }

    [Test, Category("Todo")]
    public void Todo_WithFutureStart_AndNoDuration_ShouldSucceed()
    {
        var today = CalDateTime.Today;

        var todo = new Todo
        {
            Start = today,
            RecurrenceRules = [new RecurrencePattern("FREQ=DAILY")]
        };

        // periodStart is in the future, so filtering the first occurrence will also require
        // looking at the todo's duration, which is unset/null. It must therefore be ignored.
        var firstOccurrence = todo.GetOccurrences(today.AddDays(2).ToZonedDateTime("America/New_York")).FirstOrDefault();

        Assert.That(firstOccurrence, Is.Not.Null);
        Assert.That(firstOccurrence.Start, Is.EqualTo(firstOccurrence.End));
    }

    [Test, Category("Todo")]
    public void Todo_RecurrenceWithNoEnd_IsCompleted()
    {
        var start = new CalDateTime(2026, 1, 15, 0, 0, 0, "UTC");

        var todo = new Todo
        {
            Start = start,
            RecurrenceRules = [new("FREQ=DAILY;BYDAY=TH")],
            Status = TodoStatus.Completed
        };

        Assert.That(todo.IsCompleted, Is.True);
    }

    [Test, Category("Todo")]
    public void Todo_CanBeCompletedBeforeStart()
    {
        var todo = new Todo
        {
            Start = new(2026, 1, 15, 0, 0, 0, "UTC"),
            Completed = new CalDateTime(2026, 1, 14, 0, 0, 0, "UTC"),
        };

        Assert.That(todo.IsCompleted, Is.True);
    }


    [Test, Category("Todo")]
    public void TodoCancelled()
    {
        var todo = new Todo
        {
            Start = new(2026, 1, 15, 0, 0, 0, "UTC"),
            Status = TodoStatus.Cancelled
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(todo.IsCancelled, Is.True);
            Assert.That(todo.IsActive, Is.False);
            Assert.That(todo.IsCompleted, Is.False);
        }
    }
}
