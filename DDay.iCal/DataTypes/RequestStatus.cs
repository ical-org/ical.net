using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
    [DebuggerDisplay("{StatusCode} - {StatusDesc}")]
    public class RequestStatus : iCalDataType
    {
        #region Private Fields

        private Text m_StatusDesc;
        private Text m_ExtData;
        private StatusCode m_StatusCode;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public Text StatusDesc
        {
            get { return m_StatusDesc; }
            set { m_StatusDesc = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public Text ExtData
        {
            get { return m_ExtData; }
            set { m_ExtData = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public StatusCode StatusCode
        {
            get { return m_StatusCode; }
            set { m_StatusCode = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        virtual public string Language
        {
            get { return Parameters.Get<string>("LANGUAGE"); }
            set { Parameters.Set("LANGUAGE", value); }
        }

        #endregion

        #region Constructors

        public RequestStatus() { }
        public RequestStatus(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is RequestStatus)
            {
                RequestStatus rs = (RequestStatus)obj;
                if (rs.StatusCode != null)
                    StatusCode = rs.StatusCode.Copy<StatusCode>();
                if (rs.StatusDesc != null)
                    StatusDesc = rs.StatusDesc.Copy<Text>();
                if (rs.ExtData != null)
                    ExtData = rs.ExtData.Copy<Text>();
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarObject obj)
        {
            RequestStatus rs = (RequestStatus)obj;
            Match match = Regex.Match(value, @"(.+);(.+)(;(.*))?");
            if (match.Success)
            {
                if (!match.Groups[1].Success ||
                    !match.Groups[2].Success)
                    return false;

                StatusCode = new StatusCode(match.Groups[1].Value);
                StatusDesc = new Text(match.Groups[1].Value, true);
                if (match.Groups[3].Success)
                    ExtData = new Text(match.Groups[4].Value, true);

                return true;
            }
            return false;
        }

        #endregion
    }
}
