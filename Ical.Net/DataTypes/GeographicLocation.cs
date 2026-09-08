//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ical.Net.CalendarComponents;

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

    [Obsolete("Use TryParse instead.")]
    public GeographicLocation(string value) : this()
    {
        if (TryParse(value, out var other))
        {
            Latitude = other.Latitude;
            Longitude = other.Longitude;
        }
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

    public override string ToString() => Latitude.ToString("0.000000", CultureInfo.InvariantCulture)
        + ";"
        + Longitude.ToString("0.000000", CultureInfo.InvariantCulture);

    #region Text Parsing

    public static bool TryParse(
        ReadOnlySpan<char> value,
#if NET
        [NotNullWhen(true)]
#endif
    out GeographicLocation? geographicLocation)
    {
        var partIndex = value.IndexOf(';');
        if (partIndex == -1)
        {
            geographicLocation = null;
            return false;
        }

#if NET
        var latStr = value.Slice(0, partIndex);
        var lonStr = value.Slice(partIndex + 1);
#else
        var latStr = value.Slice(0, partIndex).ToString();
        var lonStr = value.Slice(partIndex + 1).ToString();
#endif

        if (!double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat))
        {
            geographicLocation = null;
            return false;
        }

        if (!double.TryParse(lonStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
        {
            geographicLocation = null;
            return false;
        }

        geographicLocation = new(lat, lon);
        return true;
    }

#endregion
}
