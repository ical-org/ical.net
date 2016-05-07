using System;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class UriSerializer :
        EncodableDataTypeSerializer
    {
        #region Constructors

        public UriSerializer()
        {
        }

        public UriSerializer(ISerializationContext ctx)
            : base(ctx)
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
            if (obj is Uri)
            {
                var uri = (Uri)obj;

                var co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    var dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    return Encode(dt, uri.OriginalString);
                }
                return uri.OriginalString;
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                var value = tr.ReadToEnd();

                var co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    var dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    value = Decode(dt, value);
                }

                value = TextUtil.Normalize(value, SerializationContext).ReadToEnd();

                try
                {
                    var uri = new Uri(value);
                    return uri;
                }
                catch
                {
                }
            }
            return null;
        }

        #endregion
    }
}
