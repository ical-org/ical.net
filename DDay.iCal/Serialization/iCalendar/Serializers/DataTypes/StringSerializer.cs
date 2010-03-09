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

                ICalendarParameterListContainer c = SerializationContext.Peek() as ICalendarParameterListContainer;
                if (c != null)
                {
                    // Try to encode the string
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociateWith(c);
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

                ICalendarParameterListContainer c = SerializationContext.Peek() as ICalendarParameterListContainer;
                if (c != null)
                {
                    // Try to decode the string
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociateWith(c);
                    value = Decode(dt, value);
                }

                return TextUtil.Normalize(value, SerializationContext).ReadToEnd();
            }
            return null;
        }

        #endregion
    }
}
