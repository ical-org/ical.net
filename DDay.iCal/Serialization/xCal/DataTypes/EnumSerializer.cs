using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class EnumSerializer : IXCalSerializable
    {
        #region Private Fields

        private Enum m_Enum;

        #endregion

        #region Constructors

        public EnumSerializer(Enum e)
        {
            this.m_Enum = e;
        }

        #endregion

        #region IXCalSerializable Members

        public void Serialize(System.Xml.XmlTextWriter xtw)
        {            
            xtw.WriteString(SerializeToString());            
        }

        public DDay.iCal.Components.iCalObject Deserialize(System.Xml.XmlTextReader xtr, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region ISerializable Members

        public string SerializeToString()
        {
            return Enum.GetName(m_Enum.GetType(), m_Enum).ToUpper();
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
