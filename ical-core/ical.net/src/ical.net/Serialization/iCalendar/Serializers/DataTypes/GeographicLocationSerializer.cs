using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public class GeographicLocationSerializer : EncodableDataTypeSerializer
    {
        public override Type TargetType => typeof (GeographicLocation);

        public override string SerializeToString(object obj)
        {
            var g = obj as IGeographicLocation;
            if (g != null)
            {
                var value = g.Latitude.ToString("0.000000") + ";" + g.Longitude.ToString("0.000000");
                return Encode(g, value);
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var g = CreateAndAssociate() as IGeographicLocation;
            if (g != null)
            {
                // Decode the value, if necessary!
                value = Decode(g, value);

                var values = value.Split(';');
                if (values.Length != 2)
                {
                    return false;
                }

                double lat;
                double lon;
                double.TryParse(values[0], out lat);
                double.TryParse(values[1], out lon);
                g.Latitude = lat;
                g.Longitude = lon;

                return g;
            }

            return null;
        }
    }
}