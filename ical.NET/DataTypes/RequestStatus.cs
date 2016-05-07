using System;
using System.IO;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class RequestStatus :
        EncodableDataType,
        IRequestStatus
    {
        #region Private Fields

        private string _mDescription;
        private string _mExtraData;
        private IStatusCode _mStatusCode;

        #endregion

        #region Public Properties

        public virtual string Description
        {
            get { return _mDescription; }
            set { _mDescription = value; }
        }

        public virtual string ExtraData
        {
            get { return _mExtraData; }
            set { _mExtraData = value; }
        }

        public virtual IStatusCode StatusCode
        {
            get { return _mStatusCode; }
            set { _mStatusCode = value; }
        }

        #endregion

        #region Constructors

        public RequestStatus() { }
        public RequestStatus(string value)
            : this()
        {
            var serializer = new RequestStatusSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IRequestStatus)
            {
                var rs = (IRequestStatus)obj;                
                if (rs.StatusCode != null)
                    StatusCode = rs.StatusCode.Copy<IStatusCode>();
                Description = rs.Description;
                rs.ExtraData = rs.ExtraData;
            }
        }

        public override string ToString()
        {
            var serializer = new RequestStatusSerializer();
            return serializer.SerializeToString(this);
        }

        protected bool Equals(RequestStatus other)
        {
            return string.Equals(_mDescription, other._mDescription) && string.Equals(_mExtraData, other._mExtraData) &&
                   Equals(_mStatusCode, other._mStatusCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((RequestStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_mDescription != null ? _mDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_mExtraData != null ? _mExtraData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_mStatusCode != null ? _mStatusCode.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
