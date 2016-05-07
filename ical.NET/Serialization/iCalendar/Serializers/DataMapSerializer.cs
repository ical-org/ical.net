using System;
using System.IO;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers.Other;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public class DataMapSerializer :
        SerializerBase
    {
        #region Constructors

        public DataMapSerializer()
        {            
        }

        public DataMapSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Protected Methods

        protected IStringSerializer GetMappedSerializer()
        {
            var sf = GetService<ISerializerFactory>();
            var mapper = GetService<IDataTypeMapper>();
            if (sf != null &&
                mapper != null)
            {
                var obj = SerializationContext.Peek();

                // Get the data type for this object
                var type = mapper.GetPropertyMapping(obj);

                if (type != null)
                    return sf.Build(type, SerializationContext) as IStringSerializer;
                return new StringSerializer(SerializationContext);
            }
            return null;
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get
            {
                ISerializer serializer = GetMappedSerializer();
                if (serializer != null)
                    return serializer.TargetType;
                return null;
            }
        }

        public override string SerializeToString(object obj)
        {
            var serializer = GetMappedSerializer();
            if (serializer != null)
                return serializer.SerializeToString(obj);
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            var serializer = GetMappedSerializer();
            if (serializer != null)
            {
                var value = tr.ReadToEnd();
                var returnValue = serializer.Deserialize(new StringReader(value));

                // Default to returning the string representation of the value
                // if the value wasn't formatted correctly.
                // FIXME: should this be a try/catch?  Should serializers be throwing
                // an InvalidFormatException?  This may have some performance issues
                // as try/catch is much slower than other means.
                return returnValue ?? value;
            }
            return null;            
        } 

        #endregion
    }
}
