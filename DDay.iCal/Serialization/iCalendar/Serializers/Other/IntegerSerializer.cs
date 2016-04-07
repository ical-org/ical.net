using System;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class IntegerSerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(int); }
        }

        public override string SerializeToString(object integer)
        {
            try
            {
                var i = Convert.ToInt32(integer);

                var obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Encode the value as needed.
                    var dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    return Encode(dt, i.ToString());
                }
                return i.ToString();
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            try
            {
                var obj = SerializationContext.Peek() as ICalendarObject;
                if (obj != null)
                {
                    // Decode the value, if necessary!
                    var dt = new EncodableDataType();
                    dt.AssociatedObject = obj;
                    value = Decode(dt, value);
                }

                int i;
                if (Int32.TryParse(value, out i))
                    return i;
            }
            catch {}

            return value;
        }
    }
}
