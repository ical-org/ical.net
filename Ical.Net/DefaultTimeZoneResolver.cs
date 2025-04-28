//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

public static class DefaultTimeZoneResolver
{
    private static Dictionary<string, string> InitializeWindowsMappings()
        => TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping
        .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<Dictionary<string, string>> _windowsMapping
        = new Lazy<Dictionary<string, string>>(InitializeWindowsMappings, LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Use this method to turn a raw string into a NodaTime DateTimeZone. It searches all time zone providers (IANA, BCL, serialization, etc) to see if
    /// the string matches. If it doesn't, it walks each provider, and checks to see if the time zone the provider knows about is contained within the
    /// target time zone string. Some older icalendar programs would generate nonstandard time zone strings, and this secondary check works around
    /// that.
    /// </summary>
    /// <param name="tzId">A BCL, IANA, or serialization time zone identifier</param>
    /// <exception cref="ArgumentException">Processing failed</exception>
    /// <remarks>The DateTimeZone if found or null otherwise.</remarks>
    public static DateTimeZone? GetZone(string? tzId)
    {
        var exMsg = $"Unrecognized time zone id {tzId}";

        if (string.IsNullOrWhiteSpace(tzId))
        {
            return DateTimeZoneProviders.Tzdb.GetSystemDefault();
        }

        if (tzId.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            tzId = tzId.Substring(1, tzId.Length - 1);
        }

        var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
        if (zone != null)
        {
            return zone;
        }

        if (_windowsMapping.Value.TryGetValue(tzId, out var ianaZone))
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaZone) ?? throw new ArgumentException(exMsg);
        }

        zone = NodaTime.Xml.XmlSerializationSettings.DateTimeZoneProvider.GetZoneOrNull(tzId);
        if (zone != null)
        {
            return zone;
        }

        // US/Eastern is commonly represented as US-Eastern
        var newTzId = tzId.Replace("-", "/");
        zone = NodaTime.Xml.XmlSerializationSettings.DateTimeZoneProvider.GetZoneOrNull(newTzId);
        if (zone != null)
        {
            return zone;
        }

        var providerId = DateTimeZoneProviders.Tzdb.Ids.FirstOrDefault(tzId.Contains);
        if (providerId != null)
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(providerId) ?? throw new ArgumentException(exMsg);
        }

        if (_windowsMapping.Value.Keys
            .Where(tzId.Contains)
            .Any(pId => _windowsMapping.Value.TryGetValue(pId, out ianaZone))
           )
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaZone!) ?? throw new ArgumentException(exMsg);
        }

        providerId = NodaTime.Xml.XmlSerializationSettings.DateTimeZoneProvider.Ids.FirstOrDefault(tzId.Contains);
        if (providerId != null)
        {
            return NodaTime.Xml.XmlSerializationSettings.DateTimeZoneProvider.GetZoneOrNull(providerId) ?? throw new ArgumentException(exMsg);
        }

        return null;
    }

    internal static readonly DateTimeZone LocalDateTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
}
