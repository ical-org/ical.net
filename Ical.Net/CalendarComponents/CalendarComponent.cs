//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ical.Net.CalendarComponents;

/// <summary>
/// This class is used by the parsing framework for iCalendar components.
/// Generally, you should not need to use this class directly.
/// </summary>
[DebuggerDisplay("Component: {Name}")]
public class CalendarComponent : CalendarObject, ICalendarComponent
{
    /// <summary>
    /// Returns a list of properties that are associated with the iCalendar object.
    /// </summary>
    public virtual CalendarPropertyList Properties { get; protected set; } = null!;

    public CalendarComponent() : base()
    {
        Initialize();
    }

    public CalendarComponent(string name) : base(name)
    {
        Initialize();
    }

    private void Initialize()
    {
        Properties = new CalendarPropertyList(this);
    }

    protected override void OnDeserializing(StreamingContext context)
    {
        base.OnDeserializing(context);

        Initialize();
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is not ICalendarComponent c)
        {
            return;
        }

        Properties.Clear();
        foreach (var p in c.Properties)
        {
            // Uses CalendarObjectBase.Copy<T>() for a deep copy
            Properties.Add(p.Copy<ICalendarProperty>());
        }
    }

    /// <summary>
    /// Adds a property to this component.
    /// </summary>
    public virtual void AddProperty(string name, string value)
    {
        var p = new CalendarProperty(name, value);
        AddProperty(p);
    }

    /// <summary>
    /// Adds a property to this component.
    /// </summary>
    public virtual void AddProperty(ICalendarProperty p)
    {
        p.Parent = this;
        Properties.Set(p.Name, p.Value);
    }
}
