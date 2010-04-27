using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Serialization.iCalendar
{
    public class OrganizerSerializer :
        StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(Organizer); }
        }

        public override string SerializeToString(object obj)
        {
            try
            {
                IOrganizer o = obj as IOrganizer;
                if (o != null)
                    return Encode(o, Escape(o.Value.OriginalString));
                return null;
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            try
            {
                IOrganizer o = CreateAndAssociate() as IOrganizer;
                if (o != null)
                {
                    o.Value = new Uri(Unescape(Decode(o, value)));
                    return o;
                }
            }
            catch { }

            return null;
        }
    }
}
