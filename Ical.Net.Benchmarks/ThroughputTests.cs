﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Ical.Net.Benchmarks;

public class ThroughputTests
{
    [Benchmark]
    public void DeserializeAndComputeUntilOccurrences()
    {
        const string e = @"BEGIN:VCALENDAR
PRODID:-//Microsoft Corporation//Outlook 12.0 MIMEDIR//EN
VERSION:2.0
METHOD:PUBLISH
X-CALSTART:20090621T000000
X-CALEND:20090622T000000
X-WR-RELCALID:{0000002E-6380-7FD2-FED7-97EAE70D6611}
X-WR-CALNAME:Parse Error Calendar
BEGIN:VEVENT
ATTENDEE;CN=some.attendee@event.com;RSVP=TRUE:mailto:some.attendee@event.co
	m
ATTENDEE;CN=event@calendardemo.net;RSVP=TRUE:mailto:event@calendardemo.net
ATTENDEE;CN=""4th Floor Meeting Room"";CUTYPE=RESOURCE;ROLE=NON-PARTICIPANT;R
	SVP=TRUE:mailto:4th.floor.meeting.room@somewhere.com
CLASS:PUBLIC
CREATED:20090621T201527Z
DESCRIPTION:\n
DTEND;VALUE=DATE:20090622
DTSTAMP:20090621T201612Z
DTSTART;VALUE=DATE:20090621
RRULE:FREQ=DAILY;INTERVAL=2;UNTIL=20110622
LAST-MODIFIED:20090621T201618Z
LOCATION:The Exceptionally Long Named Meeting Room Whose Name Wraps Over Se
	veral Lines When Exported From Leading Calendar and Office Software App
	lication Microsoft Office 2007
ORGANIZER;CN=""Event Organizer"":mailto:some.attendee@somewhere.com
PRIORITY:5
SEQUENCE:0
SUMMARY;LANGUAGE=en-gb:Example Calendar Export that Blows Up DDay.iCal
TRANSP:TRANSPARENT
UID:040000008200E00074C5B7101A82E00800000000900AD080B5F2C901000000000000000
	010000000B29680BF9E5DC246B5EDDE228038E71F
X-ALT-DESC;FMTTYPE=text/html:<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 3.2//E
	N"">\n<HTML>\n<HEAD>\n<META NAME=""Generator"" CONTENT=""MS Exchange Server ve
	rsion 08.00.0681.000"">\n<TITLE></TITLE>\n</HEAD>\n<BODY>\n<!-- Converted f
	rom text/rtf format -->\n\n<P DIR=LTR><SPAN LANG=""en-gb""></SPAN></P>\n\n</
	BODY>\n</HTML>
X-MICROSOFT-CDO-BUSYSTATUS:FREE
X-MICROSOFT-CDO-IMPORTANCE:1
X-MICROSOFT-DISALLOW-COUNTER:FALSE
X-MS-OLK-ALLOWEXTERNCHECK:TRUE
X-MS-OLK-APPTSEQTIME:20090621T201612Z
X-MS-OLK-AUTOFILLLOCATION:TRUE
X-MS-OLK-CONFTYPE:0
BEGIN:VALARM
TRIGGER:-PT1080M
ACTION:DISPLAY
DESCRIPTION:Reminder
END:VALARM
END:VEVENT
END:VCALENDAR";

        var calendar = Calendar.Load(e);
        var calendarEvent = calendar.Events.First();
        var searchStart = new DateTime(2009, 06, 20);
        var searchEnd = new DateTime(2011, 06, 23);
        var occurrences = calendarEvent.GetOccurrences(searchStart, searchEnd);
    }

    [Benchmark]
    public void DeserializeAndComputeCountOccurrences()
    {
        const string e = @"BEGIN:VCALENDAR
PRODID:-//Microsoft Corporation//Outlook 12.0 MIMEDIR//EN
VERSION:2.0
METHOD:PUBLISH
X-CALSTART:20090621T000000
X-CALEND:20090622T000000
X-WR-RELCALID:{0000002E-6380-7FD2-FED7-97EAE70D6611}
X-WR-CALNAME:Parse Error Calendar
BEGIN:VEVENT
ATTENDEE;CN=some.attendee@event.com;RSVP=TRUE:mailto:some.attendee@event.co
	m
ATTENDEE;CN=event@calendardemo.net;RSVP=TRUE:mailto:event@calendardemo.net
ATTENDEE;CN=""4th Floor Meeting Room"";CUTYPE=RESOURCE;ROLE=NON-PARTICIPANT;R
	SVP=TRUE:mailto:4th.floor.meeting.room@somewhere.com
CLASS:PUBLIC
CREATED:20090621T201527Z
DESCRIPTION:\n
DTEND;VALUE=DATE:20090622
DTSTAMP:20090621T201612Z
DTSTART;VALUE=DATE:20090621
RRULE:FREQ=DAILY;INTERVAL=2;COUNT=365
LAST-MODIFIED:20090621T201618Z
LOCATION:The Exceptionally Long Named Meeting Room Whose Name Wraps Over Se
	veral Lines When Exported From Leading Calendar and Office Software App
	lication Microsoft Office 2007
ORGANIZER;CN=""Event Organizer"":mailto:some.attendee@somewhere.com
PRIORITY:5
SEQUENCE:0
SUMMARY;LANGUAGE=en-gb:Example Calendar Export that Blows Up DDay.iCal
TRANSP:TRANSPARENT
UID:040000008200E00074C5B7101A82E00800000000900AD080B5F2C901000000000000000
	010000000B29680BF9E5DC246B5EDDE228038E71F
X-ALT-DESC;FMTTYPE=text/html:<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 3.2//E
	N"">\n<HTML>\n<HEAD>\n<META NAME=""Generator"" CONTENT=""MS Exchange Server ve
	rsion 08.00.0681.000"">\n<TITLE></TITLE>\n</HEAD>\n<BODY>\n<!-- Converted f
	rom text/rtf format -->\n\n<P DIR=LTR><SPAN LANG=""en-gb""></SPAN></P>\n\n</
	BODY>\n</HTML>
X-MICROSOFT-CDO-BUSYSTATUS:FREE
X-MICROSOFT-CDO-IMPORTANCE:1
X-MICROSOFT-DISALLOW-COUNTER:FALSE
X-MS-OLK-ALLOWEXTERNCHECK:TRUE
X-MS-OLK-APPTSEQTIME:20090621T201612Z
X-MS-OLK-AUTOFILLLOCATION:TRUE
X-MS-OLK-CONFTYPE:0
BEGIN:VALARM
TRIGGER:-PT1080M
ACTION:DISPLAY
DESCRIPTION:Reminder
END:VALARM
END:VEVENT
END:VCALENDAR";

        var calendar = Calendar.Load(e);
        var calendarEvent = calendar.Events.First();
        var searchStart = new DateTime(2009, 06, 20);
        var searchEnd = new DateTime(2011, 06, 23);
        var occurrences = calendarEvent.GetOccurrences(searchStart, searchEnd);
    }
}