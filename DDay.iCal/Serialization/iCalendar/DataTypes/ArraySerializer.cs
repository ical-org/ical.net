using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class ArraySerializer : ISerializable 
    {
        #region Private Fields

        private Array m_Array;
        private ISerializationContext m_SerializationContext;

        #endregion

        #region Constructors

        public ArraySerializer(Array array)
        {
            this.m_SerializationContext = DDay.iCal.Serialization.SerializationContext.Default;
            this.m_Array = array;
        }

        #endregion

        #region ISerializable Members

        virtual public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }

        virtual public string SerializeToString()
        {
            return string.Empty;
        }

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            foreach (object obj in m_Array)
            {
                ISerializable serializer = SerializerFactory.Create(obj, SerializationContext);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }
        }

        public object Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion        
    }
}
