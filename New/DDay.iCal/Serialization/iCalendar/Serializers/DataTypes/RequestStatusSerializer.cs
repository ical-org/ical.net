using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Serialization.iCalendar
{
    public class RequestStatusSerializer :
        StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(RequestStatus); }
        }

        public override string SerializeToString(object obj)
        {
            try
            {
                IRequestStatus rs = obj as IRequestStatus;
                if (rs != null)
                {
                    // Push the object onto the serialization stack
                    SerializationContext.Push(rs);

                    try
                    {
                        ISerializerFactory factory = GetService<ISerializerFactory>();
                        if (factory != null)
                        {
                            IStringSerializer serializer = factory.Build(typeof(IStatusCode), SerializationContext) as IStringSerializer;
                            if (serializer != null)
                            {
                                string value = Escape(serializer.SerializeToString(rs.StatusCode));
                                value += ";" + Escape(rs.Description);
                                if (!string.IsNullOrEmpty(rs.ExtraData))
                                    value += ";" + Escape(rs.ExtraData);

                                return Encode(rs, value);
                            }
                        }
                    }
                    finally
                    {
                        // Pop the object off the serialization stack
                        SerializationContext.Pop();
                    }                    
                }

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

            IRequestStatus rs = CreateAndAssociate() as IRequestStatus;
            if (rs != null)
            {
                // Decode the value as needed
                value = Decode(rs, value);

                // Push the object onto the serialization stack
                SerializationContext.Push(rs);

                try
                {
                    ISerializerFactory factory = GetService<ISerializerFactory>();
                    if (factory != null)
                    {
                        Match match = Regex.Match(value, @"(.*?[^\\]);(.*?[^\\]);(.+)");
                        if (!match.Success)
                            match = Regex.Match(value, @"(.*?[^\\]);(.+)");

                        if (match.Success)
                        {
                            IStringSerializer serializer = factory.Build(typeof(IStatusCode), SerializationContext) as IStringSerializer;
                            if (serializer != null)
                            {
                                rs.StatusCode = serializer.Deserialize(new StringReader(Unescape(match.Groups[1].Value))) as IStatusCode;
                                rs.Description = Unescape(match.Groups[2].Value);
                                if (match.Groups.Count == 4)
                                    rs.ExtraData = Unescape(match.Groups[3].Value);

                                return rs;
                            }
                        }
                    }
                }
                finally
                {
                    // Pop the object off the serialization stack
                    SerializationContext.Pop();
                }
            }
            return null;
        }
    }
}
