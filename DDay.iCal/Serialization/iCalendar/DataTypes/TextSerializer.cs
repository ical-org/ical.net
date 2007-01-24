using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Objects;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class TextSerializer : FieldSerializer
    {
        #region Private Fields

        private Text m_Text;

        #endregion

        #region Constructors

        public TextSerializer(Text text) : base(text)
        {
            this.m_Text = text;
        }

        #endregion

        #region ISerializable Members

        public override string SerializeToString()
        {
            string value = m_Text.Value;
            value = value.Replace(@"\", @"\\");
            value = value.Replace("\n", @"\n");
            value = value.Replace(";", @"\;");
            value = value.Replace(",", @"\,");
            return value;
        }        

        #endregion
    }
}
