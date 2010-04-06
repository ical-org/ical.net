using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
    public class RequestStatus :
        EncodableDataType,
        IRequestStatus
    {
        #region Private Fields

        private string m_Description;
        private string m_ExtraData;
        private IStatusCode m_StatusCode;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public string ExtraData
        {
            get { return m_ExtraData; }
            set { m_ExtraData = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public IStatusCode StatusCode
        {
            get { return m_StatusCode; }
            set { m_StatusCode = value; }
        }

        #endregion

        #region Constructors

        public RequestStatus() { }
        public RequestStatus(string value)
            : this()
        {
            RequestStatusSerializer serializer = new RequestStatusSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IRequestStatus)
            {
                IRequestStatus rs = (IRequestStatus)obj;                
                if (rs.StatusCode != null)
                    StatusCode = rs.StatusCode.Copy<IStatusCode>();
                Description = rs.Description;
                rs.ExtraData = rs.ExtraData;
            }
        }

        public override string ToString()
        {
            RequestStatusSerializer serializer = new RequestStatusSerializer();
            return serializer.SerializeToString(this);
        }

        #endregion
    }
}
