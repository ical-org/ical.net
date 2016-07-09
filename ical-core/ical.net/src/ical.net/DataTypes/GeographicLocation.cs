using System.Diagnostics;
using System.IO;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the geographical location of an
    /// <see cref="Components.Event"/> or <see cref="Components.Todo"/> item.
    /// </summary>
    [DebuggerDisplay("{Latitude};{Longitude}")]
    public class GeographicLocation : EncodableDataType, IGeographicLocation
    {
        private double _mLatitude;
        private double _mLongitude;

        public double Latitude
        {
            get { return _mLatitude; }
            set { _mLatitude = value; }
        }

        public double Longitude
        {
            get { return _mLongitude; }
            set { _mLongitude = value; }
        }

        public GeographicLocation() {}

        public GeographicLocation(string value) : this()
        {
            var serializer = new GeographicLocationSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public GeographicLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override bool Equals(object obj)
        {
            if (obj is IGeographicLocation)
            {
                var g = (IGeographicLocation) obj;
                return g.Latitude.Equals(Latitude) && g.Longitude.Equals(Longitude);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IGeographicLocation)
            {
                var g = (IGeographicLocation) obj;
                Latitude = g.Latitude;
                Longitude = g.Longitude;
            }
        }

        public override string ToString()
        {
            return Latitude.ToString("0.000000") + ";" + Longitude.ToString("0.000000");
        }
    }
}