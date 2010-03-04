using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public class DataMapSerializer :
        SerializerBase
    {
        #region Protected Methods

        protected ISerializer GetMappedSerializer()
        {
            ISerializerFactory sf = GetService<ISerializerFactory>();
            IDataTypeMapper mapper = GetService<IDataTypeMapper>();
            if (sf != null &&
                mapper != null)
            {
                Type mappedType = mapper.Map(SerializationContext.Peek());
                if (mappedType != null)
                    return sf.Create(mappedType, SerializationContext);
            }
            return null;
        }

        #endregion

        #region Overrides

        public override string SerializeToString(object obj)
        {
            ISerializer serializer = GetMappedSerializer();
            if (serializer != null)
                return serializer.SerializeToString(obj);
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            ISerializer serializer = GetMappedSerializer();
            if (serializer != null)
                return serializer.Deserialize(tr);
            return null;
        } 

        #endregion
    }
}
