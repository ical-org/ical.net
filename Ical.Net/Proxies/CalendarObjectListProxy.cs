//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Linq;
using Ical.Net.Collections;
using Ical.Net.Collections.Proxies;

namespace Ical.Net.Proxies;

public class CalendarObjectListProxy<TType> : GroupedCollectionProxy<string, ICalendarObject, TType>, ICalendarObjectList<TType>
    where TType : class, ICalendarObject
{
    public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list) { }

    /// <summary>
    /// The indexer for the <see cref="ICalendarObjectList{T}"/> interface. This is the virtual, primary implementation of the indexer.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// Returns the item at the specified index, or null if the index is invalid.
    /// </returns>
    public virtual TType? this[int index] => this.Skip(index).FirstOrDefault();

    /// <summary>
    /// This is an explicit non-nullable implementation of the indexer for the <see cref="ICalendarObjectList{T}"/> interface.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    TType ICalendarObjectList<TType>.this[int index] => this[index] ?? throw new ArgumentOutOfRangeException(nameof(index));
}
