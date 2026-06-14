//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Tests;

[TestFixture]
public class AttendeeTest
{
    internal static CalendarEvent VEventFactory() => new CalendarEvent
    {
        Summary = "Testing",
        Start = new CalDateTime(2010, 3, 25),
        End = new CalDateTime(2010, 3, 26)
    };

    private static readonly IList<Attendee> _attendees = new List<Attendee>
    {
        new Attendee("MAILTO:james@example.com")
        {
            CommonName = "James James",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Tentative
        },
        new Attendee("MAILTO:mary@example.com")
        {
            CommonName = "Mary Mary",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Accepted
        }
    }.AsReadOnly();


    /// <summary>
    /// Ensures that attendees can be properly added to an event.
    /// </summary>
    [Test, Category("Attendee")]
    public void Add1Attendee()
    {
        var evt = VEventFactory();
        Assert.That(evt.Attendees.Count, Is.EqualTo(0));

        evt.Attendees.Add(_attendees[0]);
        Assert.That(evt.Attendees, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            //the properties below had been set to null during the Attendees.Add operation in NuGet version 2.1.4
            Assert.That(evt.Attendees[0].Role, Is.EqualTo(ParticipationRole.RequiredParticipant));
            Assert.That(evt.Attendees[0].ParticipationStatus, Is.EqualTo(EventParticipationStatus.Tentative));
        }
    }

    [Test, Category("Attendee")]
    public void Add2Attendees()
    {
        var evt = VEventFactory();
        Assert.That(evt.Attendees.Count, Is.EqualTo(0));

        evt.Attendees.Add(_attendees[0]);
        evt.Attendees.Add(_attendees[1]);
        Assert.That(evt.Attendees, Has.Count.EqualTo(2));
        Assert.That(evt.Attendees[1].Role, Is.EqualTo(ParticipationRole.RequiredParticipant));
    }

    /// <summary>
    /// Ensures that attendees can be properly removed from an event.
    /// </summary>
    [Test, Category("Attendee")]
    public void Remove1Attendee()
    {
        var evt = VEventFactory();
        Assert.That(evt.Attendees.Count, Is.EqualTo(0));

        var attendee = _attendees.First();
        evt.Attendees.Add(attendee);
        Assert.That(evt.Attendees, Has.Count.EqualTo(1));

        evt.Attendees.Remove(attendee);
        Assert.That(evt.Attendees.Count, Is.EqualTo(0));

        evt.Attendees.Remove(_attendees.Last());
        Assert.That(evt.Attendees.Count, Is.EqualTo(0));
    }
}
