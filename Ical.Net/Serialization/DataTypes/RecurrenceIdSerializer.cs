//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Logging;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.DataTypes;

/// <summary>
/// Provides serialization and deserialization functionality for <see cref="RecurrenceId"/> objects.
/// </summary>
public class RecurrenceIdSerializer : SerializerBase
{
    private const string ThisAndFuture = "THISANDFUTURE";
    private readonly ILogger _logger;

    /// <summary>
    /// This constructor is required for the SerializerFactory to work.
    /// </summary>
    public RecurrenceIdSerializer()
    {
        _logger = LoggingProvider.CreateLogger<RecurrenceIdSerializer>();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DateTimeSerializer"/> class.
    /// </summary>
    /// <param name="ctx"></param>
    public RecurrenceIdSerializer(SerializationContext ctx) : base(ctx)
    {
        _logger = LoggingProvider.CreateLogger<RecurrenceIdSerializer>();
    }

    public override Type TargetType => typeof(RecurrenceId);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not RecurrenceId recurrenceId)
        {
            return null;
        }
        
        var factory = GetService<ISerializerFactory>();
        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as DateTimeSerializer;

        recurrenceId.Parameters.AddRange(dtSerializer!.GetParameters(recurrenceId.StartTime));
        if (recurrenceId.Range == RecurrenceRange.ThisAndFuture)
        {
            recurrenceId.Parameters.Add(new CalendarParameter("RANGE", ThisAndFuture));
        }

        return dtSerializer.SerializeToString(recurrenceId.StartTime);
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
            case ThisAndFuture:
                recurrenceRange = RecurrenceRange.ThisAndFuture;
                break;
            default:
                recurrenceRange = RecurrenceRange.ThisInstance;
                _logger.LogWarning("Ignored invalid RANGE parameter '{Range}' for RECURRENCE-ID", rangeString);
                break;
        }
        
        var factory = GetService<ISerializerFactory>();

        var dtSerializer = factory.Build(typeof(CalDateTime), SerializationContext) as IStringSerializer;
        var start = dtSerializer?.Deserialize(new StringReader(value)) as CalDateTime;

        return start is null
            ? null
            : new RecurrenceId(start, recurrenceRange);
    }
}
