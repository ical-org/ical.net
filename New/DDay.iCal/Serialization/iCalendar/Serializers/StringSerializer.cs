using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class StringSerializer :
        SerializerBase
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

        public override string SerializeToString(object obj)
        {
            if (obj != null)
                return obj.ToString();
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                return TextUtil.Normalize(tr, SerializationContext).ReadToEnd();
            }
            return null;
        }

        #endregion
    }
}
