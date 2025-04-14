//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Collections;
using Ical.Net.Collections.Proxies;

namespace Ical.Net.Proxies;

public class ParameterCollectionProxy : GroupedCollectionProxy<string, CalendarParameter, CalendarParameter>, IParameterCollection
{
    protected GroupedValueList<string, CalendarParameter, CalendarParameter, string> Parameters
        => (GroupedValueList<string, CalendarParameter, CalendarParameter, string>) RealObject;

    public ParameterCollectionProxy(IGroupedList<string, CalendarParameter> realObject) : base(realObject) { }

    public virtual void SetParent(ICalendarObject? parent)
    {
        foreach (var parameter in this)
        {
            parameter.Parent = parent;
        }
    }

    public virtual void Add(string name, string value)
        => RealObject.Add(new CalendarParameter(name, value));

    public virtual string? Get(string name)
    {
        var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

        return parameter?.Value;
    }

    public virtual IList<string> GetMany(string name) => new GroupedValueListProxy<string, CalendarParameter, CalendarParameter, string, string>(Parameters, name);

    public virtual void Set(string name, string? value)
    {
        var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

        if (parameter == null)
        {
            RealObject.Add(new CalendarParameter(name, value));
        }
        else
        {
            parameter.SetValue(value);
        }
    }

    public virtual void Set(string name, IEnumerable<string?> values)
    {
        var parameter = RealObject.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.Ordinal));

        if (parameter == null)
        {
            RealObject.Add(new CalendarParameter(name, values));
        }
        else
        {
            parameter.SetValue(values);
        }
    }

    public virtual int IndexOf(CalendarParameter obj) => 0;

    public virtual void Insert(int index, CalendarParameter item) { }

    public virtual void RemoveAt(int index) { }

    /// <summary>
    /// The indexer for the IList interface. This is the virtual, primary implementation of the indexer.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// Returns the item at the specified index, or null if the index is invalid.
    /// </returns>
    /// <exception cref="ArgumentNullException">For a <see langword="null"/> value.</exception>
    public virtual CalendarParameter? this[int index]
    {
        get => Parameters[index];
        set => Parameters[index] = (value ?? throw new ArgumentNullException(nameof(value)));
    }

    /// <summary>
    /// This is an explicit non-nullable implementation of the indexer for the IList interface.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    CalendarParameter IList<CalendarParameter>.this[int index]
    {
        get => this[index] ?? throw new ArgumentOutOfRangeException(nameof(index));
        set => this[index] = value;
    }
}
