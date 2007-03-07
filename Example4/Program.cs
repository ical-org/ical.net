using System;
using System.Collections.Generic;
using System.Text;

using DDay.iCal;
using DDay.iCal.Components;
using DDay.iCal.Serialization;

namespace Example4
{
    /// <summary>
    /// This program displays the serialization of custom properties within custom classes.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Load the example iCalendar file into our CustomICalendar object
            CustomICalendar iCal = (CustomICalendar)iCalendar.LoadFromFile(typeof(CustomICalendar), @"Example4.ics");
                        
            // Set the additional information on our custom events
            Console.WriteLine("Adding additional information to each event from Example4.ics...");
            foreach(CustomEvent evt in iCal.Events)
                evt.AdditionalInformation = "Some additional information we want to save";

            // Serializer our iCalendar
            Console.WriteLine("Saving altered iCalendar to Example4_Serialized.ics...");
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            serializer.Serialize(@"Example4_Serialized.ics");

            // Load the serialized calendar from the file we just saved,
            // and display the event summary for each event, along
            // with the additional information we saved.
            Console.WriteLine("Loading Example4_Serialized.ics to display saved events...");
            iCal = (CustomICalendar)iCalendar.LoadFromFile(typeof(CustomICalendar), @"Example4_Serialized.ics");
            foreach (CustomEvent evt in iCal.Events)                
                Console.WriteLine("\t" + evt.Summary + ": " + evt.AdditionalInformation);
        }
    }
}
