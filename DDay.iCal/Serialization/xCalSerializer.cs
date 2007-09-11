using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DDay.iCal.Serialization.xCal;
using DDay.iCal.Serialization.xCal.Components;
using System.IO;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// A class that serializes <see cref="iCalendar"/>s into the
    /// xCal (.xcs) format.
    /// </summary>
    public class xCalSerializer : ComponentBaseSerializer
    {
        #region Private Fields

        private DDay.iCal.iCalendar m_iCalendar;

        #endregion

        #region Public Properties

        public DDay.iCal.iCalendar iCalendar
        {
            get { return m_iCalendar; }
            set
            {
                if (!object.Equals(m_iCalendar, value))
                {
                    m_iCalendar = value;
                    base.Component = value;
                }
            }
        }

        #endregion

        #region Constructors

        public xCalSerializer() { }
        public xCalSerializer(DDay.iCal.iCalendar iCal)
            : base(iCal)
        {
            iCalendar = iCal;
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

        public override void Serialize(XmlTextWriter xtw)
        {            
            xtw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
            xtw.WriteStartElement("xCal", "iCalendar", "urn:ietf:params:xml:ns:xcal");
            base.Serialize(xtw);
            xtw.WriteEndElement();
        }
        

        #endregion
    }
}
