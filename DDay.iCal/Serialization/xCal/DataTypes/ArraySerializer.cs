using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class ArraySerializer : IXCalSerializable
    {
        #region Private Fields

        private Array m_Array;

        #endregion

        #region Constructors

        public ArraySerializer(Array array)
        {
            this.m_Array = array;
        }

        #endregion        

        #region IXCalSerializable Members

        public void Serialize(System.Xml.XmlTextWriter xtw)
        {
            foreach (object obj in m_Array)
            {
                IXCalSerializable serializer = SerializerFactory.Create(obj);
                if (serializer != null)
                    serializer.Serialize(xtw);
            }
        }

        public DDay.iCal.Components.iCalObject Deserialize(System.Xml.XmlTextReader xtr, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ISerializable Members

        public string SerializeToString()
        {
            return string.Empty;
        }

        public void Serialize(System.IO.Stream stream, Encoding encoding)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DDay.iCal.Components.iCalObject Deserialize(System.IO.Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
