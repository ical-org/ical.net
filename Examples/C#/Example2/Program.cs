using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;

namespace Example2
{
    public class Program
    {
        /// <summary>
        /// Creates a string representation of an event.
        /// </summary>
        /// <param name="evt">The event to display</param>
        /// <returns>A string representation of the event.</returns>
        static string GetDescription(Event evt)
        {
            string summary = evt.Summary + ": " + evt.Start.Local.ToShortDateString();

            if (evt.IsAllDay)
            {
                return summary + " (all day)";
            }
            else
            {
                summary += ", " + evt.Start.Local.ToShortTimeString();
                return summary + " (" + Math.Round((double)evt.End.Subtract(evt.Start).TotalHours) + " hours)";
            }
        }

        /// <summary>
        /// The main program execution.
        /// </summary>
        static void Main(string[] args)
        {
            // Create a new iCalendar
            iCalendar iCal = new iCalendar();
            
            // Create the event, and add it to the iCalendar
            Event evt = iCal.Create<Event>();

            // Set information about the event
            evt.Start = iCalDateTime.Today.AddHours(8);            
            evt.End = evt.Start.AddHours(18); // This also sets the duration            
            evt.Description = "The event description";
            evt.Location = "Event location";
            evt.Summary = "18 hour event summary";

            // Set information about the second event
            evt = iCal.Create<Event>();
            evt.Start = iCalDateTime.Today.AddDays(5);
            evt.End = evt.Start.AddDays(1);
            evt.IsAllDay = true;
            evt.Summary = "All-day event";

            // Display each event
            foreach(Event e in iCal.Events)
                Console.WriteLine("Event created: " + GetDescription(e));
            
            // Serialize (save) the iCalendar
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, @"iCalendar.ics");
            Console.WriteLine("iCalendar file saved." + Environment.NewLine);
            
            // Load the calendar from the file we just saved
            IICalendarCollection calendars = iCalendar.LoadFromFile(@"iCalendar.ics");
            Console.WriteLine("iCalendar file loaded.");

            // Iterate through each event to display its description
            // (and verify the file saved correctly)
            foreach (IICalendar calendar in calendars)
            {
                foreach (Event e in calendar.Events)
                    Console.WriteLine("Event loaded: " + GetDescription(e));
            }
        }
    }
}
