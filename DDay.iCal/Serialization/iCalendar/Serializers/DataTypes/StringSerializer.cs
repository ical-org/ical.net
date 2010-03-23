using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class StringSerializer :
        EncodableDataTypeSerializer
    {
        #region Constructors

        public StringSerializer()
        {
        }

        public StringSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(string); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj != null)
            {
                string value = obj.ToString();

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    // Encode the string as needed.
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    return Encode(dt, value);
                }
                return value;
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                string value = tr.ReadToEnd();

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    // Try to decode the string
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    value = Decode(dt, value);
                }

                return TextUtil.Normalize(value, SerializationContext).ReadToEnd();
            }
            return null;
        }

        #endregion
    }
}
