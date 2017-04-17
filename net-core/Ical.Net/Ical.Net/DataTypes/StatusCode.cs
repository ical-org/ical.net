using System.IO;
using System.Linq;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An iCalendar status code.
    /// </summary>
    public class StatusCode : EncodableDataType
    {
        public int[] Parts { get; private set; } = new int[0];

        public int Primary => Parts.Length > 0 ? Parts[0] : 0;

        public int Secondary => Parts.Length > 1 ? Parts[1] : 0;

        public int Tertiary => Parts.Length > 2 ? Parts[2] : 0;

        public StatusCode() {}

        public StatusCode(int[] parts)
        {
            if (parts == null) { return; }

            Parts = parts;
        }

        public StatusCode(string serializedValue)
        {
            if (serializedValue == null) { return; }

            var serializer = new StatusCodeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(serializedValue)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is StatusCode)
            {
                var sc = (StatusCode) obj;
                Parts = new int[sc.Parts.Length];
                sc.Parts.CopyTo(Parts, 0);
            }
        }

        public override string ToString() => new StatusCodeSerializer().SerializeToString(this);

        protected bool Equals(StatusCode other) => Parts.SequenceEqual(other.Parts);

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
            return Equals((StatusCode) obj);
        }

        public override int GetHashCode() => CollectionHelpers.GetHashCode(Parts);
    }
}