using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public abstract class SerializerBase :
        ISerializer
    {
        #region Private Fields

        ISerializationContext m_SerializationContext;

        #endregion

        #region Constructors

        public SerializerBase()
        {
            m_SerializationContext = DDay.iCal.Serialization.SerializationContext.Default;
        }

        #endregion

        #region ISerializer Members

        virtual public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }

        virtual public T GetService<T>()
        {
            if (SerializationContext != null)
            {
                object obj = SerializationContext.GetService(typeof(T));
                if (obj != null)
                    return (T)obj;
            }
            return default(T);
        }

        public abstract string SerializeToString(ICalendarObject obj);
        public abstract object Deserialize(TextReader tr);

        public object Deserialize(Stream stream, Encoding encoding)
        {
            object obj = null;
            using (StreamReader sr = new StreamReader(stream, encoding))
            {
                obj = Deserialize(sr);
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
