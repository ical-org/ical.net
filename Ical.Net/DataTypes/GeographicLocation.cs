//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Diagnostics;
using System.Globalization;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes;

/// <summary>
/// A class that represents the geographical location of an
/// <see cref="Components.Event"/> or <see cref="Todo"/> item.
/// </summary>
[DebuggerDisplay("{Latitude};{Longitude}")]
public class GeographicLocation : EncodableDataType
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public GeographicLocation() { }

    public GeographicLocation(string value) : this()
    {
        var serializer = new GeographicLocationSerializer();
        serializer.Deserialize(value);
    }

    public GeographicLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        var geo = obj as GeographicLocation;
        if (geo == null)
        {
            return;
        }

        Latitude = geo.Latitude;
        Longitude = geo.Longitude;
    }

    public override string ToString() => Latitude.ToString("0.000000", CultureInfo.InvariantCulture) + ";" + Longitude.ToString("0.000000", CultureInfo.InvariantCulture);
}
