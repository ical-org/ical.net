using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
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
