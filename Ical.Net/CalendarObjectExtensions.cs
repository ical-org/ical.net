//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
namespace Ical.Net;

public static class CalendarObjectExtensions
{
    public static void AddChild<TItem>(this ICalendarObject obj, TItem child) where TItem : ICalendarObject
        => obj.Children.Add(child);

    public static void RemoveChild<TItem>(this ICalendarObject obj, TItem child) where TItem : ICalendarObject
        => obj.Children.Remove(child);
}
