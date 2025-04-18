//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Runtime.Serialization;
using Ical.Net.Collections;

namespace Ical.Net;

/// <summary>
/// The base class for all iCalendar objects and components.
/// </summary>
public class CalendarObject : CalendarObjectBase, ICalendarObject
{
    // Are initialized in the constructor
    private ICalendarObjectList<ICalendarObject> _children = null!;

    internal CalendarObject()
    {
        Name = string.Empty;
        Initialize();
    }

    public CalendarObject(string name) : this()
    {
        Name = name;
    }

    public CalendarObject(int line, int col) : this()
    {
        Line = line;
        Column = col;
    }

    private void Initialize()
    {
        _children = new CalendarObjectList();
        _children.ItemAdded += Children_ItemAdded;
    }

    [OnDeserializing]
    internal void DeserializingInternal(StreamingContext context) => OnDeserializing(context);

    [OnDeserialized]
    internal void DeserializedInternal(StreamingContext context) => OnDeserialized(context);

    protected virtual void OnDeserializing(StreamingContext context) => Initialize();

    protected virtual void OnDeserialized(StreamingContext context) { }

    private void Children_ItemAdded(object? sender, ObjectEventArgs<ICalendarObject, int> e) => e.First.Parent = this;

    protected bool Equals(CalendarObject other) => string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((CalendarObject) obj);
    }

    public override int GetHashCode() => Name.GetHashCode();

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable c)
    {
        if (c is not ICalendarObject obj)
        {
            return;
        }

        // Copy the name and basic information
        Name = obj.Name;
        Parent = obj.Parent;
        Line = obj.Line;
        Column = obj.Column;

        // Add each child
        Children.Clear();
        foreach (var child in obj.Children)
        {
            // Add a deep copy of the child instead of the child itself
            this.AddChild(child.Copy<ICalendarObject>()!);
        }
    }

    /// <summary>
    /// Returns the parent iCalObject that owns this one.
    /// </summary>
    public virtual ICalendarObject? Parent { get; set; }

    /// <summary>
    /// A collection of iCalObjects that are children of the current object.
    /// </summary>
    public virtual ICalendarObjectList<ICalendarObject> Children => _children;

    /// <summary>
    /// Gets or sets the name of the iCalObject. For iCalendar components, this is the RFC 5545 name of the component.
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Gets the <see cref="Net.Calendar"/> object.
    /// The setter must be implemented in a derived class.
    /// </summary>
    public virtual Calendar? Calendar
    {
        get
        {
            ICalendarObject obj = this;
            while (obj is not Net.Calendar && obj?.Parent != null)
            {
                obj = obj.Parent;
            }

            return obj as Calendar;
        }
        protected set => throw new NotSupportedException();
    }

    public virtual int Line { get; set; }

    public virtual int Column { get; set; }

    public virtual string Group
    {
        get => Name;
        set => Name = value;
    }
}
