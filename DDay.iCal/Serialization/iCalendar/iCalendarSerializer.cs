using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Serialization.iCalendar;
using DDay.iCal.Serialization.iCalendar.Objects;

namespace DDay.iCal.Serialization
{
    public class iCalendarSerializer : ComponentBaseSerializer 
    {
        #region Private Fields

        private DDay.iCal.iCalendar m_iCal;

        #endregion

        #region Constructors

        public iCalendarSerializer(DDay.iCal.iCalendar iCal) : base(iCal)
        {
            this.m_iCal = iCal;
        }

        #endregion

        #region Public Methods

        public void Serialize(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            UTF8Encoding utf8 = new UTF8Encoding();
            Serialize(fs, utf8);
            fs.Close();
        }

        #endregion

        #region ISerializable Members

        public override void Serialize(Stream stream, Encoding encoding)
        {
            base.Serialize(stream, encoding);
        }

        #endregion
    }
}
