//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Globalization;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class GeographicLocationSerializer : EncodableDataTypeSerializer
{
    public GeographicLocationSerializer() { }

    public GeographicLocationSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(GeographicLocation);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not GeographicLocation location)
        {
            return null;
        }

        var value = location.Latitude.ToString("0.000000", CultureInfo.InvariantCulture.NumberFormat) + ";"
            + location.Longitude.ToString("0.000000", CultureInfo.InvariantCulture.NumberFormat);
        return Encode(location, value);
    }

    public GeographicLocation? Deserialize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (CreateAndAssociate() is not GeographicLocation location)
        {
            return null;
        }

        // Decode the value, if necessary!
        var decoded = Decode(location, value);
        if (decoded == null) return null;

        var values = decoded.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 2)
        {
            return null;
        }

        double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var lat);
        double.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var lon);
        location.Latitude = lat;
        location.Longitude = lon;

        return location;
    }

    public override object? Deserialize(TextReader tr) => Deserialize(tr.ReadToEnd());
}
