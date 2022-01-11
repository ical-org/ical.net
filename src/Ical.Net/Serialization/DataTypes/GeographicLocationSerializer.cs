using System;
using System.Globalization;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class GeographicLocationSerializer : EncodableDataTypeSerializer
    {
        public GeographicLocationSerializer() { }

        public GeographicLocationSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (GeographicLocation);

        public override string SerializeToString(object obj)
        {
            var g = obj as GeographicLocation;
            if (g == null)
            {
                return null;
            }

            var value = TruncateValue(g.Latitude, 12).ToString("0.000000######", CultureInfo.InvariantCulture.NumberFormat) + ";"
                + TruncateValue(g.Longitude, 12).ToString("0.000000######", CultureInfo.InvariantCulture.NumberFormat);
            return Encode(g, value);
        }
        private double TruncateValue(double val, int precision)
        {
            if (precision <= 0)
                return val;

            double retVal = val * Math.Pow(10, precision);
            if (double.IsInfinity(retVal))
                return val;
            retVal = Math.Truncate(retVal) / Math.Pow(10, precision);
            return retVal.Equals(Double.NaN) ? val : retVal;
        }
        public GeographicLocation Deserialize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var g = CreateAndAssociate() as GeographicLocation;
            if (g == null)
            {
                return null;
            }

            // Decode the value, if necessary!
            value = Decode(g, value);

            var values = value.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2)
            {
                return null;
            }

            double lat;
            double lon;
            double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out lat);
            double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lon);
            g.Latitude = lat;
            g.Longitude = lon;

            return g;
        }

        public override object Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
    }
}