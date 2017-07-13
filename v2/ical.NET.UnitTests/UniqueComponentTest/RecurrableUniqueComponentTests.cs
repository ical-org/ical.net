using Ical.Net.Interfaces.Components;
using NUnit.Framework;
using System;
using System.IO;


namespace Ical.Net.UnitTests.UniqueComponentTest
{
    [TestFixture]
    public class RecurrableUniqueComponentTests
    {

        /// <summary>
        /// Verify that the comparer when getting occurences uses the UID as part of the comparison to that
        /// occurences that appear identical on the surface can be differentiated by the underlying event data
        /// </summary>
        [Test]
        public void RecurrableUniqueComponentTestGetOccurences()
        {

            const string ical =
@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:Data::ICal 0.13
X-WR-CALNAME:Ranney School
CALSCALE:GREGORIAN
X-WR-TIMEZONE:US/Eastern
BEGIN:VTIMEZONE
TZID:US/Eastern
X-LIC-LOCATION:US/Eastern
BEGIN:DAYLIGHT
DTSTART:19700308T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
TZNAME:EDT
TZOFFSETFROM:-0500
TZOFFSETTO:-0400
END:DAYLIGHT
BEGIN:STANDARD
DTSTART:19701101T020000
RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
TZNAME:EST
TZOFFSETFROM:-0400
TZOFFSETTO:-0500
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Catholic H.S. Comments:  
DTSTART;TZID=US/Eastern:20170501T153000
DTEND;TZID=US/Eastern:20170501T153000
DTSTAMP:20170526T134609Z
LOCATION:Suneagles Golf Club
SUMMARY:Golf:  Girls Varsity Match vs Red Bank Catholic H.S. (Date Changed) 05-10-17
UID:742290@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Mater Dei Prep Comments: The first game of a double header 
DTSTART;TZID=US/Eastern:20170501T154500
DTEND;TZID=US/Eastern:20170501T170000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Softball:  Varsity Game vs Mater Dei Prep (Away)  
UID:756283@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Middle 
DTSTART;TZID=US/Eastern:20170501T154500
DTEND;TZID=US/Eastern:20170501T171500
DTSTAMP:20170526T134609Z
LOCATION:Count Basie Park
SUMMARY:Softball:  Middle School Game vs Red Bank Middle (Home)  
UID:751879@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Rumson Country Day 
DTSTART;TZID=US/Eastern:20170501T154500
DTEND;TZID=US/Eastern:20170501T171500
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Lacrosse:  Girls Middle School Game vs Rumson Country Day (Home)  
UID:689591@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Middle 
DTSTART;TZID=US/Eastern:20170501T154500
DTEND;TZID=US/Eastern:20170501T170000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Baseball:  Middle School Game vs Red Bank Middle (Home)  (Rescheduled from 05-01-17)
UID:757080@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Middle Comments:  
DTSTART;TZID=US/Eastern:20170501T154500
DTEND;TZID=US/Eastern:20170501T170000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Baseball:  Middle School Game vs Red Bank Middle Rescheduled to 05-01-17
UID:757039@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Regional Comments:  
DTSTART;TZID=US/Eastern:20170501T160000
DTEND;TZID=US/Eastern:20170501T160000
DTSTAMP:20170526T134609Z
LOCATION:Red Bank Regional High School
SUMMARY:Tennis:  Boys JV Game vs Red Bank Regional (Date Changed) 05-04-17
UID:505085@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Mater Dei Prep 
DTSTART;TZID=US/Eastern:20170501T160000
DTEND;TZID=US/Eastern:20170501T160000
DTSTAMP:20170526T134609Z
LOCATION:Mater Dei Prep High School
SUMMARY:Baseball:  Varsity Game vs Mater Dei Prep (Away)  (Rescheduled from 04-26-17)
UID:749367@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Gill St. Bernard's Comments:  
DTSTART;TZID=US/Eastern:20170501T160000
DTEND;TZID=US/Eastern:20170501T173000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Lacrosse:  Boys Varsity Game vs Gill St. Bernard's (Date Changed) 05-08-17
UID:587538@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Central Reg. H.S. Comments: Shore Conference Tournament 1st Round 
DTSTART;TZID=US/Eastern:20170501T160000
DTEND;TZID=US/Eastern:20170501T160000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Tennis:  Boys Varsity Shore Conference Tournament Game/Match vs Central Reg. H.S. (Home)  
UID:750787@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Red Bank Regional Comments:  
DTSTART;TZID=US/Eastern:20170501T160000
DTEND;TZID=US/Eastern:20170501T160000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Tennis:  Boys Varsity Game vs Red Bank Regional (Cancelled)
UID:747623@srv1.advancedview.rschooltoday.com
END:VEVENT
BEGIN:VEVENT
DESCRIPTION:Type: nonconference Opponent: Mater Dei Prep Comments: This is the second game of a double header. 
DTSTART;TZID=US/Eastern:20170501T171500
DTEND;TZID=US/Eastern:20170501T190000
DTSTAMP:20170526T134609Z
LOCATION:Ranney School
SUMMARY:Softball:  Varsity Game vs Mater Dei Prep (Home)  
UID:756276@srv1.advancedview.rschooltoday.com
END:VEVENT
END:VCALENDAR";

            var collection = Calendar.LoadFromStream(new StringReader(ical));
            var startCheck = new DateTime(2017, 04, 01);
            var endCheck = startCheck.AddMonths(2);
            var occurrences = collection.GetOccurrences<IEvent>(startCheck, startCheck.AddMonths(2));

            Assert.AreEqual(12, occurrences.Count);


        }

    }

}
