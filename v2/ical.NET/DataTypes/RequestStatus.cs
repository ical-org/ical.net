using System.IO;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.General;
using ical.net.Serialization.iCalendar.Serializers.DataTypes;

namespace ical.net.DataTypes
{
    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
    public class RequestStatus : EncodableDataType, IRequestStatus
    {
        private string _mDescription;
        private string _mExtraData;
        private StatusCode _mStatusCode;

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

        public virtual StatusCode StatusCode
        {
            get { return _mStatusCode; }
            set { _mStatusCode = value; }
        }

        public RequestStatus() {}

        public RequestStatus(string value) : this()
        {
            var serializer = new RequestStatusSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (!(obj is IRequestStatus))
            {
                return;
            }

            var rs = (IRequestStatus) obj;
            if (rs.StatusCode != null)
            {
                StatusCode = rs.StatusCode;
            }
            Description = rs.Description;
            rs.ExtraData = rs.ExtraData;
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((RequestStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _mDescription?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (_mExtraData?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_mStatusCode?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}