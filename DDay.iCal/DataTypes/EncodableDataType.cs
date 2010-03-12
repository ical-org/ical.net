using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "EncodableDataType", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class EncodableDataType :
        CalendarDataType,
        IEncodableDataType
    {
        #region Private Fields

        string m_Encoding;

        #endregion

        #region IEncodableDataType Members

        virtual public string Encoding
        {
            get
            {
                if (AssociatedParameters != null) 
                    return AssociatedParameters.Get("ENCODING");
                return m_Encoding;
            }
            set
            {
                if (AssociatedParameters != null)
                    AssociatedParameters.Set("ENCODING", value);
                m_Encoding = value; 
            }
        }

        #endregion
    }
}
