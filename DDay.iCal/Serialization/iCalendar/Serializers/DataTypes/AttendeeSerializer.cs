using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class AttendeeSerializer :
        StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(Attendee); }
        }

        public override string SerializeToString(object obj)
        {
            IAttendee a = obj as IAttendee;
            if (a != null)
                return Encode(a, a.Value.OriginalString);
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            try
            {
                IAttendee a = CreateAndAssociate() as IAttendee;
                if (a != null)
                {
                    // Decode and unescape the value as needed
                    a.Value = new Uri(Unescape(Decode(a, value)));
                    return a;
                }
            }
            catch { }

            return null;
        }
    }
}
