//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture(SerializerVersion.Current)]
[TestFixture(SerializerVersion.Obsolete)]
internal class RecurrenceIdentifierTests(SerializerVersion serializerVersion)
{
    private readonly SerializerSwitch _serializerSwitch = new(serializerVersion);


    [TestCase(RecurrenceRange.ThisAndFuture, ";RANGE=THISANDFUTURE")]
    [TestCase(RecurrenceRange.ThisInstance, "")] // This means no RANGE parameter
    [TestCase(9999, "")] // Invalid values should be treated as ThisInstance
    public void RecurrenceIdentifierWithTzId_ShouldSerializeCorrectly(RecurrenceRange range, string rangeString)
    {
        var evt = new CalendarEvent
        {
            RecurrenceIdentifier = new RecurrenceIdentifier(new CalDateTime(2025, 7, 1, 10, 0, 0, "America/New_York"), range)
        };

        var serialized = _serializerSwitch.Serialize(evt);
        var expected = $"RECURRENCE-ID;TZID=America/New_York{rangeString}:20250701T100000";

        Assert.That(serialized, Does.Contain(expected));
    }

    [TestCase(RecurrenceRange.ThisAndFuture, ";RANGE=THISANDFUTURE", "20250701T100000")]
    [TestCase(RecurrenceRange.ThisAndFuture, ";VALUE=DATE;RANGE=THISANDFUTURE", "20250701")]
    [TestCase(RecurrenceRange.ThisInstance, "", "20250701T100000")]
    [TestCase(RecurrenceRange.ThisInstance, ";VALUE=DATE", "20250701")]
    public void RecurrenceIdentifierWithoutTzId_ShouldSerializeCorrectly(RecurrenceRange range, string dtRangeString, string dateTime)
    {
        var evt = new CalendarEvent
        {
            RecurrenceIdentifier = new RecurrenceIdentifier(new CalDateTime(dateTime), range)
        };

        var serialized = _serializerSwitch.Serialize(evt);
        var expected = $"RECURRENCE-ID{dtRangeString}:{dateTime}";

        Assert.That(serialized, Does.Contain(expected));
    }

    [TestCase("20250701T100000", ";RANGE=THISANDFUTURE", RecurrenceRange.ThisAndFuture)]
    [TestCase("20250701", ";VALUE=DATE;RANGE=THISANDFUTURE", RecurrenceRange.ThisAndFuture)]
    [TestCase("20250701T100000", "", RecurrenceRange.ThisInstance)]
    [TestCase("20250701", ";VALUE=DATE", RecurrenceRange.ThisInstance)]
    [TestCase("20250701T100000", ";RANGE=invalid", RecurrenceRange.ThisInstance)]
    public void RecurrenceIdentifierWithoutTzId_ShouldDeserializeCorrectly(string dt, string recId, RecurrenceRange expected)
    {
        var cal = $"""
                  BEGIN:VEVENT
                  DTSTAMP:20250928T221419Z
                  RECURRENCE-ID{recId}:{dt}
                  SEQUENCE:1
                  UID:c03cbcb3-6b37-49d6-9e05-a06a34a3ee57
                  END:VEVENT
                  """;

        var recurrenceId = _serializerSwitch.Deserialize<CalendarEvent>(cal).RecurrenceIdentifier;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(recurrenceId!.StartTime, Is.EqualTo(new CalDateTime(dt)));
            Assert.That(recurrenceId.Range, Is.EqualTo(expected));
        }
    }

    [Test]
    public void RecurrenceIdentifierSerializer_LowLevel()
    {
        var recurrenceId = new RecurrenceIdentifier(new CalDateTime("20250930T140000", "Europe/Paris"), RecurrenceRange.ThisAndFuture);
        var serializer = new RecurrenceIdentifierSerializer();

        var serialized = serializer.SerializeToString(recurrenceId);
        // Invalid parameter type should not throw, but return null
        var serializedAsNull = serializer.SerializeToString(string.Empty);

        var param = ParameterProviderHelper.GetRecurrenceIdentifierParameters(recurrenceId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(serializer.TargetType == recurrenceId.GetType());
            Assert.That(serialized, Is.EqualTo("20250930T140000"));
            Assert.That(serializedAsNull, Is.Null);
            Assert.That(param, Has.Exactly(2).Items);
            Assert.That(param[0].Name, Is.EqualTo("TZID"));
            Assert.That(param[0].Value, Is.EqualTo("Europe/Paris"));
            Assert.That(param[1].Name, Is.EqualTo("RANGE"));
            Assert.That(param[1].Value, Is.EqualTo("THISANDFUTURE"));
        }
    }

}
