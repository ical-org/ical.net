//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;

namespace Ical.Net.Serialization;

public partial class CalendarSerializer : ComponentSerializer
{
    private readonly Calendar? _calendar;

    [Obsolete("Use the static CalendarSerializer methods instead.")]
    public CalendarSerializer()
        : this(new SerializationContext()) { }

    [Obsolete("Use the static CalendarSerializer methods instead.")]
    public CalendarSerializer(Calendar cal)
    {
        _calendar = cal;
    }

    [Obsolete("Use the static CalendarSerializer methods instead.")]
    public CalendarSerializer(SerializationContext ctx) : base(ctx) { }

    [Obsolete("Use the static CalendarSerializer.Serialize() instead.")]
    public virtual string? SerializeToString() => SerializeToString(_calendar);

    [Obsolete("This will be removed with no public replacement.")]
    protected override IComparer<ICalendarProperty> PropertySorter => new CalendarPropertySorter();

    [Obsolete("Use the static CalendarSerializer.Deserialize() instead.")]
    public override object? Deserialize(TextReader tr) => null;

    private class CalendarPropertySorter : IComparer<ICalendarProperty>
    {
        public int Compare(ICalendarProperty? x, ICalendarProperty? y)
        {
            if (x == y)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            // Alphabetize all properties except VERSION, which should appear first.
            if (string.Equals("VERSION", x.Name, StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }
            return string.Equals("VERSION", y.Name, StringComparison.OrdinalIgnoreCase)
                ? 1
                : string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
