using System;
using System.Collections.Generic;
using System.Text;

using DDay.iCal;
using DDay.iCal.Components;

namespace Example3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an empty list of iCalendars
            List<iCalendar> iCalendars = new List<iCalendar>();

            // First, load the example iCalendar file normally
            iCalendars.Add(iCalendar.LoadFromFile(@"Example3.ics"));
            
            // Next, load the example iCalendar file into our CustomICalendar object,
            // so we can show the difference between loading it normally and loading
            // it into a custom object.
            iCalendars.Add(iCalendar.LoadFromFile(typeof(CustomICalendar), @"Example3.ics"));

            // Since our CustomICalendar objects has its ComponentBaseTypeAttribute set
            // to our CustomComponentBase class, all of our objects are created with it.
            //
            // Hence, each of our events will not be loaded as Event objects, because we
            // instructed our CustomComponentBase class to create a CustomEvent class
            // whenever a VEVENT object was encountered.

            foreach (iCalendar iCal in iCalendars)
            {
                Console.WriteLine("iCalendar is of type: " + iCal.GetType().Name);

                int i = 1;

                //
                // Iterate through each unique component in the iCalendar
                //
                foreach (ComponentBase component in iCal.UniqueComponents)
                {
                    // Display the component's type                    
                    Console.WriteLine(i + ": Component is a " + component.GetType().Name + ", which is a subclass of " + component.GetType().BaseType.Name);
                    
                    if (component is CustomEvent)
                    {
                        // The component was loaded as a CustomEvent                        
                        CustomEvent evt = (CustomEvent)component;
      
                        // So, let's set some additional information
                        evt.AdditionalInformation = @"
More information to set about this event...
Note that this information will not be saved
when the iCalendar is serialized.  We'll deal
with this in the next example.
";

                        // Let's show its summary also...
                        Console.WriteLine("\tEvent summary: " + evt.Summary);
                    }
                    else
                    {
                        // The component is not a CustomEvent, because
                        // the iCalendar was loaded normally.
                        Console.WriteLine("\tComponent is not a CustomEvent.");
                    }

                    i++;
                }

                Console.WriteLine(Environment.NewLine);
            }
        }
    }
}
