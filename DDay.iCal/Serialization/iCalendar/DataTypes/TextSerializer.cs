using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class TextSerializer : EncodableDataTypeSerializer
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
            Text t = (Text)m_Text.Copy();
            t.Escape();

            return Encode(t.Value);            
        }        

        #endregion
    }
}
