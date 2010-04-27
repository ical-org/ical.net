using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;

namespace Example6
{
    class Program
    {
        static void Main(string[] args)
        {
            // First, create a calendar
            IICalendar iCal = CreateCalendar();

            // Make a copy of the calendar and save it!
            IICalendar newCal = iCal.Copy<IICalendar>();
            SaveCalendar("Copied.ics", newCal);

            // Show the calendar in a specific time zone
            ShowCalendar(iCal, "America/New_York");
            ShowCalendar(iCal, "America/Denver");

            // Save the calendar to file
            SaveCalendar("Output.ics", iCal);
            EditAndSaveEvent("Output.ics", "1234567890");
        }

        /// <summary>
        /// Creates a calendar with 2 events, and returns it.
        /// </summary>
        static IICalendar CreateCalendar()
        {
            // First load a file containing time zone information for North & South America
            IICalendar timeZones = iCalendar.LoadFromFile("America.ics")[0];

            // Add the time zones we are going to use to our calendar
            // If we're not sure what we'll use, we could simply copy the
            // entire time zone information instead:
            //
            // IICalendar iCal = timeZones.Copy<IICalendar>();
            //
            // This will take significantly more disk space, and can slow
            // down performance, so I recommend you determine which time
            // zones will be used and only copy the necessary zones.
            IICalendar iCal = new iCalendar();
            iCal.AddChild(timeZones.GetTimeZone("America/New_York"));
            iCal.AddChild(timeZones.GetTimeZone("America/Denver"));            

            // Create an event and attach it to the iCalendar.
            Event evt = iCal.Create<Event>();

            // Set the one-line summary of the event
            evt.Summary = "The first Monday and second-to-last Monday of each month";

            // Set the longer description of the event
            evt.Description = "A more in-depth description of this event.";
            
            // Set the event to start at 11:00 A.M. New York time on January 2, 2007.
            evt.Start = new iCalDateTime(2007, 1, 2, 11, 0, 0, "America/New_York", iCal);

            // Set the duration of the event to 1 hour.
            // NOTE: this will automatically set the End time of the event
            evt.Duration = TimeSpan.FromHours(1);            

            // The event has been confirmed
            evt.Status = EventStatus.Confirmed;

            // Set the geographic location (latitude,longitude) of the event
            evt.GeographicLocation = new GeographicLocation(114.2938, 32.982);
            
            evt.Location = "Home office";
            evt.Priority = 7;

            // Add an organizer to the event.
            // This is the person who created the event (or who is in charge of it)            
            evt.Organizer = new Organizer("MAILTO:danielg@daywesthealthcare.com");
            // Indicate that this organizer is a member of another group
            evt.Organizer.Parameters.Add("MEMBER", "MAILTO:DEV-GROUP@host2.com");

            // Add a person who will attend the event
            // FIXME: re-implement this
            //evt.Attendees.Add(new Attendee("doug@ddaysoftware.com"));

            // Add categories to the event
            evt.Categories.Add("Work");
            evt.Categories.Add("Personal");

            // Add some comments to the event
            evt.Comments.Add("Comment #1");
            evt.Comments.Add("Comment #2");

            // Add resources that will be used for the event
            evt.Resources.Add("Conference Room #2");
            evt.Resources.Add("Projector #4");

            // Add contact information to this event, with an optional link to further information
            // FIXME: reimplement this:
            //evt.Contacts.Add("Doug Day (XXX) XXX-XXXX", new Uri("http://www.someuri.com/pdi/dougd.vcf"));

            // Set the identifier for the event.  NOTE: this will happen automatically
            // if you don't do it manually.  We set it manually here so we can easily
            // refer to this event later.
            evt.UID = "1234567890";

            // Now, let's add a recurrence pattern to this event.
            // It needs to happen on the first Monday and
            // second to last Monday of each month.
            RecurrencePattern rp = new RecurrencePattern();
            rp.Frequency = FrequencyType.Monthly;
            rp.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.First));
            rp.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.SecondToLast));            
            evt.RecurrenceRules.Add(rp);

            // Let's also add an alarm on this event so we can be reminded of it later.
            Alarm alarm = new Alarm();

            // Display the alarm somewhere on the screen.
            alarm.Action = AlarmAction.Display; 

            // This is the text that will be displayed for the alarm.
            alarm.Summary = "Alarm for the first Monday and second-to-last Monday of each month";

            // The alarm is set to occur 30 minutes before the event
            alarm.Trigger = new Trigger(TimeSpan.FromMinutes(-30));

            // Set the alarm to repeat twice (for a total of 3 alarms)
            // before the event.  Each repetition will occur 10 minutes
            // after the initial alarm.  In english - that means
            // the alarm will go off 30 minutes before the event,
            // then again 20 minutes before the event, and again
            // 10 minutes before the event.
            alarm.Repeat = 2;
            alarm.Duration = TimeSpan.FromMinutes(10);
            
            // Add the alarm to the event
            evt.Alarms.Add(alarm);
                        
            // Create another (much more simple) event
            evt = iCal.Create<Event>();
            evt.Summary = "Every month on the 21st";
            evt.Description = "A more in-depth description of this event.";
            evt.Start = new iCalDateTime(2007, 1, 21, 16, 0, 0, "America/New_York", iCal);
            evt.Duration = TimeSpan.FromHours(1.5);

            rp = new RecurrencePattern();
            rp.Frequency = FrequencyType.Monthly;
            evt.RecurrenceRules.Add(rp);

            return iCal;
        }

        /// <summary>
        /// Loads the iCalendar found at the path indicated by <param name="filepath"/> and
        /// makes some small changes to the event with the UID indicated by <paramref name="eventUID"/>,
        /// and saves those changes to a file with "_Altered" appended to the name.
        /// </summary>
        static void EditAndSaveEvent(string filepath, string eventUID)
        {
            IICalendar iCal = iCalendar.LoadFromFile(filepath)[0];            
            IEvent evt = iCal.Events[eventUID];
            if (evt != null)
            {
                evt.Summary = "New summary for event";
                // FIXME: re-implement this
                //evt.AddAttendee("AnotherPerson@someurl.com");
                evt.Sequence++;

                string newFilename = Path.GetFileNameWithoutExtension(filepath) + "_Altered" + Path.GetExtension(filepath);
                SaveCalendar(newFilename, iCal);
            }
        }

        /// <summary>
        /// Displays the calendar in the time zone identified by <paramref name="tzid"/>.
        /// </summary>
        static void ShowCalendar(IICalendar iCal, string tzid)
        {
            IDateTime start = new iCalDateTime(2007, 3, 1);
            IDateTime end = new iCalDateTime(2007, 4, 1).AddSeconds(-1);

            IList<Occurrence> occurrences = iCal.GetOccurrences(start, end);

            Console.WriteLine("====Events/Todos/Journal Entries in " + tzid + "====");
            foreach (Occurrence o in occurrences)
            {
                IRecurringComponent rc = o.Source as IRecurringComponent;
                if (rc != null)
                {
                    Console.WriteLine(
                        o.Period.StartTime.ToTimeZone(tzid).ToString("ddd, MMM d - h:mm") + " to " +
                        o.Period.EndTime.ToTimeZone(tzid).ToString("h:mm tt") + Environment.NewLine +
                        rc.Summary + Environment.NewLine);
                }
            }

            Console.WriteLine("====Alarms in " + tzid + "====");
            foreach (object obj in iCal.Children)
            {
                IRecurringComponent rc = obj as IRecurringComponent;
                if (rc != null)
                {
                    foreach (AlarmOccurrence ao in rc.PollAlarms(start, end))
                    {
                        Console.WriteLine(
                            "Alarm: " +
                            ao.DateTime.ToTimeZone(tzid).ToString("ddd, MMM d - h:mm") + ": " +
                            ao.Alarm.Summary);
                    }
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Saves the calendar to the specified <paramref name="filepath"/>.
        /// </summary>
        static void SaveCalendar(string filepath, IICalendar iCal)
        {
            iCalendarSerializer serializer = new iCalendarSerializer();
            serializer.Serialize(iCal, filepath);
        }
    }
}
