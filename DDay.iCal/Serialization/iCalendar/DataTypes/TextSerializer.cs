using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

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
            // added null check - you can't call .Replace on a null
            // string, but you can just return null as a string
            if (value != null) 
            {
                value = value.Replace(@"\", @"\\");
                value = value.Replace("\n", @"\n");
                value = value.Replace(";", @"\;");
                value = value.Replace(",", @"\,");
            }
            return value;
        }        

        #endregion
    }
}
