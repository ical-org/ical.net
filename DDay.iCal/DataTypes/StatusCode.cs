using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{    
    /// <summary>
    /// An iCalendar status code.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "StatusCode", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class StatusCode : iCalDataType
    {
        #region Private Fields

        private int[] m_Parts;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public int[] Parts
        {
            get { return m_Parts; }
            set { m_Parts = value; }
        }

        public int Primary
        {
            get
            {
                if (m_Parts.Length > 0)
                    return m_Parts[0];
                return 0;
            }
        }

        public int Secondary
        {
            get
            {
                if (m_Parts.Length > 1)
                    return m_Parts[1];
                return 0;
            }
        }

        public int Tertiary
        {
            get
            {
                if (m_Parts.Length > 2)
                    return m_Parts[2];
                return 0;
            }
        }

        #endregion

        #region Constructors

        public StatusCode() { }
        public StatusCode(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is StatusCode)
            {
                StatusCode sc = (StatusCode)obj;
                Parts = new int[sc.Parts.Length];
                sc.Parts.CopyTo(Parts, 0);
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            StatusCode sc = (StatusCode)obj;
            Match match = Regex.Match(value, @"\d(\.\d+)*");
            if (match.Success)
            {
                int[] iparts;
                string[] parts = match.Value.Split('.');
                iparts = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    int num;
                    if (!Int32.TryParse(parts[i], out num))
                        return false;
                    iparts[i] = num;
                }

                sc.Parts = iparts;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            string[] vals = new string[Parts.Length];
            for (int i = 0; i < Parts.Length; i++)
                vals[i] = Parts[i].ToString();
            return string.Join(".", vals);
        }

        #endregion
    }
}
