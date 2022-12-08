using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
    public class RequestStatus : EncodableDataType
    {
        public string Description { get; set; }

        public string ExtraData { get; set; }

        public StatusCode StatusCode { get; set; }

        public RequestStatus() {}

        public RequestStatus(string value) : this()
        {
            var serializer = new RequestStatusSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (!(obj is RequestStatus rs))
            {
                return;
            }

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

        protected bool Equals(RequestStatus other) => string.Equals(Description, other.Description) && string.Equals(ExtraData, other.ExtraData) &&
            Equals(StatusCode, other.StatusCode);

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
                var hashCode = Description?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ExtraData?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (StatusCode?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}