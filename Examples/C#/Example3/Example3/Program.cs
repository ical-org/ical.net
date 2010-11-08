using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace Example3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new calendar
            IICalendar iCal = new iCalendar();

            // Add the local time zone to the calendar
            ITimeZone local = iCal.AddLocalTimeZone();

            // Build a list of additional time zones
            // from .NET Framework.
            List<ITimeZone> otherTimeZones = new List<ITimeZone>();                        
            foreach (TimeZoneInfo tzi in System.TimeZoneInfo.GetSystemTimeZones())
            {
                // We've already added the local time zone, so let's skip it!
                if (tzi != System.TimeZoneInfo.Local)
                {
                    // Add the time zone to our list (but don't include it directly in the calendar).
                    otherTimeZones.Add(iCalTimeZone.FromSystemTimeZone(tzi));
                }
            }

            // Create a new event in the calendar
            // that uses our local time zone
            IEvent evt = iCal.Create<Event>();
            evt.Summary = "Test Event";
            evt.Start = iCalDateTime.Today.AddHours(8).SetTimeZone(local);            
            evt.Duration = TimeSpan.FromHours(1);

            // Get all occurrences of the event that happen today
            foreach (Occurrence occurrence in iCal.GetOccurrences<IEvent>(iCalDateTime.Today))
            {
                // Write the event with the time zone information attached
                Console.WriteLine(occurrence.Period.StartTime);

                // Note that the time printed is identical to the above, but without time zone information.
                Console.WriteLine(occurrence.Period.StartTime.Local);

                // Convert the start time to other time zones and display the relative time.
                foreach (ITimeZone otherTimeZone in otherTimeZones)
                    Console.WriteLine(occurrence.Period.StartTime.ToTimeZone(otherTimeZone));
            }
        }
    }
}
