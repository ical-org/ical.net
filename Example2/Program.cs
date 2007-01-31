using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using DDay.iCal.Components;
using DDay.iCal.Serialization;

namespace Example2
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Create a new iCalendar
            iCalendar iCal = new iCalendar();
            
            // Create the event, and add it to the iCalendar
            Event evt = Event.Create(iCal);

            // Set information about the event
            evt.Start = DateTime.Today;
            evt.End = DateTime.Today.AddDays(1); // This also sets the duration
            evt.DTStamp = DateTime.Now;
            evt.Description = "The event description";
            evt.Location = "Event location";
            evt.Summary = "The summary of the event";
            
            // Serialize (save) the iCalendar
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"iCalendar.ics");
        }
    }
}
