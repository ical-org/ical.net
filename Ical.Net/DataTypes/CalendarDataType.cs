//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Ical.Net.Proxies;
using Ical.Net.Serialization;
using NodaTime;

namespace Ical.Net.DataTypes;

/// <summary>
/// An abstract class from which all iCalendar data types inherit.
/// </summary>
public abstract class CalendarDataType : ICalendarDataType, IDeserializationCallbacks
{
    // Well be set with Initialize()
    private IParameterCollection _parameters = null!;
    private ParameterCollectionProxy _proxy = null!;

    private ICalendarObject? _associatedObject;

    protected CalendarDataType()
    {
        Initialize();
    }

    private void Initialize()
    {
        _parameters = new ParameterList();
        _proxy = new ParameterCollectionProxy(_parameters);
    }

    void IDeserializationCallbacks.OnDeserializing(StreamingContext context) => OnDeserializing(context);

    void IDeserializationCallbacks.OnDeserialized(StreamingContext context) => OnDeserialized(context);

    protected virtual void OnDeserializing(StreamingContext context)
    {
        Initialize();
    }

    protected virtual void OnDeserialized(StreamingContext context) { }

    public virtual Type? GetValueType()
    {
        // See RFC 5545 Section 3.2.20.
        if (_proxy.ContainsKey("VALUE"))
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
                    return typeof(RecurrenceRule);
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

    public virtual void SetValueType(string type) =>
        _proxy.Set("VALUE", type.ToUpperInvariant());

    public virtual ICalendarObject? AssociatedObject
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

    public virtual Calendar? Calendar => _associatedObject?.Calendar;

    /// <summary>
    /// The time zone provider of the associated Calendar, OR the default ical.net
    /// provider if there is no associated Calendar.
    /// </summary>
    internal IDateTimeZoneProvider CalendarTimeZoneProvider
        => Calendar?.TimeZoneProvider ?? CalendarTimeZoneProviders.TzdbWithAliases;

    public virtual string? Language
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
    /// Creates a new, empty instance of the concrete runtime type.
    /// </summary>
    /// <remarks>
    /// Derived types must override this with a direct <c>new</c>, which is what keeps
    /// <see cref="Copy{T}"/> free of reflection and therefore trimming- and NativeAOT-safe. The
    /// base implementation falls back to <see cref="Activator"/>, which needs the concrete type's
    /// parameterless constructor to survive trimming; under NativeAOT a type relying on it fails
    /// with a <see cref="MissingMethodException"/>.
    /// </remarks>
    /// <returns>A new instance of the runtime type of this object.</returns>
    protected virtual CalendarDataType? CreateNew() => CreateNewByActivator();

    [UnconditionalSuppressMessage("Trimming", "IL2072",
        Justification = "Fallback for derived types outside this library that do not override "
            + "CreateNew(). Every type in this library overrides it, so the trimmer is never asked "
            + "to preserve a library type's constructor on account of this call.")]
    private CalendarDataType? CreateNewByActivator()
        => Activator.CreateInstance(GetType(), true) as CalendarDataType;

    /// <summary>
    /// Creates a deep copy of the <see cref="T"/> object.
    /// </summary>
    /// <returns>The copy of the <see cref="T"/> object.</returns>
    public virtual T? Copy<T>()
    {
        var obj = CreateNew();

        if (obj is not T o) return default(T);

        ((ICopyable) obj).CopyFrom(this);
        return o;
    }

    public virtual IParameterCollection Parameters => _proxy;
}
