//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using Ical.Net.Proxies;

namespace Ical.Net.DataTypes;

/// <summary>
/// An abstract class from which all iCalendar data types inherit.
/// </summary>
public abstract class CalendarDataType : ICalendarDataType
{
    private IParameterCollection _parameters;
    private ParameterCollectionProxy _proxy;

    private ICalendarObject _associatedObject;

    protected CalendarDataType()
    {
        Initialize();
    }

    private void Initialize()
    {
        _parameters = new ParameterList();
        _proxy = new ParameterCollectionProxy(_parameters);
    }

    [OnDeserializing]
    internal void DeserializingInternal(StreamingContext context)
    {
        OnDeserializing(context);
    }

    [OnDeserialized]
    internal void DeserializedInternal(StreamingContext context)
    {
        OnDeserialized(context);
    }

    protected virtual void OnDeserializing(StreamingContext context)
    {
        Initialize();
    }

    protected virtual void OnDeserialized(StreamingContext context) { }

    public virtual Type GetValueType()
    {
        // See RFC 5545 Section 3.2.20.
        if (_proxy != null && _proxy.ContainsKey("VALUE"))
        {
            switch (_proxy.Get("VALUE"))
            {
                case "BINARY":
                    return typeof(byte[]);
                case "BOOLEAN":
                    return typeof(bool);
                case "CAL-ADDRESS":
                    return typeof(Uri);
                case "DATE":
                    return typeof(CalDateTime);
                case "DATE-TIME":
                    return typeof(CalDateTime);
                case "DURATION":
                    return typeof(Duration);
                case "FLOAT":
                    return typeof(double);
                case "INTEGER":
                    return typeof(int);
                case "PERIOD":
                    return typeof(Period);
                case "RECUR":
                    return typeof(RecurrencePattern);
                case "TEXT":
                    return typeof(string);
                case "TIME":
                    // FIXME: implement ISO.8601.2004
                    throw new NotImplementedException();
                case "URI":
                    return typeof(Uri);
                case "UTC-OFFSET":
                    return typeof(UtcOffset);
                default:
                    return null;
            }
        }
        return null;
    }

    public virtual void SetValueType(string type)
    {
        _proxy?.Set("VALUE", type?.ToUpper());
    }

    public virtual ICalendarObject AssociatedObject
    {
        get => _associatedObject;
        set
        {
            if (Equals(_associatedObject, value))
            {
                return;
            }

            _associatedObject = value;
            if (_associatedObject != null)
            {
                _proxy.SetParent(_associatedObject);
                if (_associatedObject is ICalendarParameterCollectionContainer)
                {
                    _proxy.SetProxiedObject(((ICalendarParameterCollectionContainer) _associatedObject).Parameters);
                }
            }
            else
            {
                _proxy.SetParent(null);
                _proxy.SetProxiedObject(_parameters);
            }
        }
    }

    public virtual Calendar Calendar => _associatedObject?.Calendar;

    public virtual string Language
    {
        get => Parameters.Get("LANGUAGE");
        set => Parameters.Set("LANGUAGE", value);
    }

    /// <inheritdoc/>
    public virtual void CopyFrom(ICopyable obj)
    {
        if (obj is not ICalendarDataType dt)
        {
            return;
        }

        _associatedObject = dt.AssociatedObject;
        _proxy.SetParent(_associatedObject);
        _proxy.SetProxiedObject(dt.Parameters);
    }

    /// <summary>
    /// Creates a deep copy of the <see cref="T"/> object.
    /// </summary>
    /// <returns>The copy of the <see cref="T"/> object.</returns>
    public virtual T Copy<T>()
    {
        var type = GetType();
        var obj = Activator.CreateInstance(type, true) as ICopyable;

        if (obj is not T o) return default(T);

        obj.CopyFrom(this);
        return o;
    }

    public virtual IParameterCollection Parameters => _proxy;
}
