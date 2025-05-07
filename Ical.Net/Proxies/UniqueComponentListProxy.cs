//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.Collections;

namespace Ical.Net.Proxies;

public class UniqueComponentListProxy<TComponentType> :
    CalendarObjectListProxy<TComponentType>,
    IUniqueComponentList<TComponentType>
    where TComponentType : class, IUniqueComponent
{
    private readonly Dictionary<string, TComponentType> _lookup;

    public UniqueComponentListProxy(IGroupedCollection<string, ICalendarObject> children) : base(children)
    {
        _lookup = new Dictionary<string, TComponentType>();
    }

    private TComponentType? Search(string uid)
    {
        if (_lookup.TryGetValue(uid, out var componentType))
        {
            return componentType;
        }

        var item = this.FirstOrDefault(c => string.Equals(c.Uid, uid, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            return null;
        }

        _lookup[uid] = item;
        return item;
    }

    /// <summary>
    /// The indexer for the <see cref="IUniqueComponentList{T}"/> interface. This is the virtual, primary implementation of the indexer.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns>
    /// Returns the item at the specified index, or null if the index is invalid.
    /// </returns>
    public virtual TComponentType? this[string uid]
    {
        get => Search(uid);
        set
        {
            // Find the item matching the UID
            var item = Search(uid);

            if (item != null)
            {
                Remove(item);
            }

            if (value != null)
            {
                Add(value);
            }
        }
    }

    /// <summary>
    /// This is an explicit non-nullable implementation of the indexer for the <see cref="IUniqueComponentList{T}"/> interface.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    TComponentType IUniqueComponentList<TComponentType>.this[string uid]
    {
        get => Search(uid) ?? throw new ArgumentOutOfRangeException(nameof(uid), "The specified UID does not exist.");
        set
        {
            var item = Search(uid);

            if (item != null)
            {
                Remove(item);
            }

            Add(value);
        }
    }

    public void AddRange(IEnumerable<TComponentType> collection)
    {
        foreach (var element in collection)
        {
            Add(element);
        }
    }
}
