using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class AttachmentSerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(Attachment); }
        }

        public override string SerializeToString(object obj)
        {
            IAttachment a = obj as IAttachment;
            if (a != null)
            {
                if (a.Uri != null)
                {
                    // Ensure no VALUE type is provided
                    if (a.AssociatedParameters != null)
                        a.AssociatedParameters.Remove("VALUE");

                    return Encode(a, a.Uri.OriginalString);
                }
                else if (a.Data != null)
                {
                    // Ensure the VALUE type is set to BINARY
                    if (a.AssociatedParameters != null)
                    {
                        a.AssociatedParameters.Remove("VALUE");
                        a.AssociatedParameters.Add(new CalendarParameter("VALUE", "BINARY"));
                    }

                    return Encode(a, a.Data);
                }
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IAttachment a = CreateAndAssociate() as IAttachment;
            if (a != null)
            {
                // Decode the value, if necessary
                value = Decode(a, value);

                if (a.AssociatedParameters != null && 
                    a.AssociatedParameters.ContainsKey("VALUE"))
                {
                    // If the VALUE type is specifically set to BINARY,
                    // then set the Data property instead.
                    string valueType = a.AssociatedParameters.Get("VALUE");
                    if (string.Compare(valueType, "BINARY", true) == 0)
                    {
                        if (value != null)
                            a.Data = System.Text.Encoding.Unicode.GetBytes(value);
                        return a;
                    }
                }

                // The default VALUE type for attachments is URI.  So, let's
                // grab the URI by default.
                if (value != null)
                    a.Uri = new Uri(value);
                                
                return a;
            }
            return null;
        }
    }
}
