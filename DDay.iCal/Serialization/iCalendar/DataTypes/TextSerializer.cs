using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
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
            Text t = m_Text.Copy<Text>();
            t.Escape();

            return Encode(t.Value);            
        }        

        #endregion
    }
}
