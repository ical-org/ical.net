using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public abstract class SerializableBase :
        ISerializable
    {
        #region Private Fields

        ISerializationContext m_SerializationContext;

        #endregion

        #region ISerializable Members

        virtual public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }

        public abstract string SerializeToString(ICalendarObject obj);
        public abstract object Deserialize(TextReader tr, Type iCalendarType);

        public object Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            object obj = null;
            using (StreamReader sr = new StreamReader(stream, encoding))
            {
                obj = Deserialize(sr, iCalendarType);
            }
            return obj;
        }

        public void Serialize(ICalendarObject obj, Stream stream, Encoding encoding)
        {            
            using (StreamWriter sw = new StreamWriter(stream, encoding))
                sw.Write(SerializeToString(obj));
        }

        #endregion
    }
}
