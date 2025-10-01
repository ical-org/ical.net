//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Logging;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.DataTypes;

/// <summary>
/// Provides serialization and deserialization functionality for <see cref="RecurrenceIdentifier"/> objects.
/// </summary>
public class RecurrenceIdentifierSerializer : SerializerBase, IParameterProvider
{
    private readonly ILogger _logger;

    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    public RecurrenceIdentifierSerializer()
    {
        _logger = LoggingProvider.CreateLogger<RecurrenceIdentifierSerializer>();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DateTimeSerializer"/> class.
    /// </summary>
    /// <param name="ctx"></param>
    public RecurrenceIdentifierSerializer(SerializationContext ctx) : base(ctx)
    {
        _logger = LoggingProvider.CreateLogger<RecurrenceIdentifierSerializer>();
    }

    public override Type TargetType => typeof(RecurrenceIdentifier);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not RecurrenceIdentifier rid)
        {
            return null;
        }

        if (!Enum.IsDefined(typeof(RecurrenceRange), rid.Range))
        {
            _logger.LogWarning("Ignored invalid RANGE parameter '{Range}' for RECURRENCE-ID", rid.Range);
        }

        var factory = GetService<ISerializerFactory>();
        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as DateTimeSerializer;
        
        return dtSerializer!.SerializeToString(rid.StartTime);
    }

    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        var parent = SerializationContext.Peek();

        // The associated object is an ICalendarObject of type CalendarProperty
        // that eventually contains a "RANGE" parameter deserialized in a prior step
        var rangeString = (parent as ICalendarParameterCollectionContainer)?.Parameters.Get("RANGE")?.ToUpperInvariant();

        RecurrenceRange recurrenceRange;
        switch (rangeString)
        {
            case null:
            case "":
                recurrenceRange = RecurrenceRange.ThisInstance;
                break;
            case "THISANDFUTURE":
                recurrenceRange = RecurrenceRange.ThisAndFuture;
                break;
            default:
                recurrenceRange = RecurrenceRange.ThisInstance;
                _logger.LogWarning("Ignored invalid RANGE parameter '{Range}' for RECURRENCE-ID", rangeString);
                break;
        }
        
        var factory = GetService<ISerializerFactory>();

        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;

        return dtSerializer!.Deserialize(new StringReader(value)) is not CalDateTime start
            ? null
            : new RecurrenceIdentifier(start, recurrenceRange);
    }

    public IReadOnlyList<CalendarParameter> GetParameters(object? value)
        => ParameterProviderHelper.GetRecurrenceIdentifierParameters(value);
}
