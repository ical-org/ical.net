using System.IO;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An iCalendar status code.
    /// </summary>
    public class StatusCode : EncodableDataType, IStatusCode
    {
        private int[] _mParts;

        public int[] Parts
        {
            get { return _mParts; }
            set { _mParts = value; }
        }

        public int Primary
        {
            get
            {
                if (_mParts.Length > 0)
                {
                    return _mParts[0];
                }
                return 0;
            }
        }

        public int Secondary
        {
            get
            {
                if (_mParts.Length > 1)
                {
                    return _mParts[1];
                }
                return 0;
            }
        }

        public int Tertiary
        {
            get
            {
                if (_mParts.Length > 2)
                {
                    return _mParts[2];
                }
                return 0;
            }
        }

        public StatusCode() {}

        public StatusCode(string value) : this()
        {
            var serializer = new StatusCodeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IStatusCode)
            {
                var sc = (IStatusCode) obj;
                Parts = new int[sc.Parts.Length];
                sc.Parts.CopyTo(Parts, 0);
            }
        }

        public override string ToString()
        {
            var serializer = new StatusCodeSerializer();
            return serializer.SerializeToString(this);
        }

        protected bool Equals(StatusCode other)
        {
            return Equals(_mParts, other._mParts);
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
            return Equals((StatusCode) obj);
        }

        public override int GetHashCode()
        {
            return _mParts?.GetHashCode() ?? 0;
        }
    }
}