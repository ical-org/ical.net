// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

/// <summary>
/// Provides a custom <see cref="DateTimeZone"/> for a given TZID created from a <see cref="CalendarComponents.VTimeZone"/>
/// </summary>
internal class VTimeZoneProvider : IDateTimeZoneProvider
{
    private static readonly Lazy<VTimeZoneProvider> _instance = new(() => new VTimeZoneProvider());
    private readonly Dictionary<string, VDateTimeZone> _customTimeZones;

    /// <summary>
    /// Initializes a new singleton instance of the <see cref="VTimeZoneProvider"/> class.
    /// </summary>
    private VTimeZoneProvider()
    {
        _customTimeZones = new();
    }

    /// <summary>
    /// Adds a <see cref="VDateTimeZone"/> to the provider.
    /// </summary>
    /// <param name="tzId">The timezone ID</param>
    /// <param name="intervals">The list of <see cref="ZoneInterval"/>s for the timezone ID.</param>
    /// <returns>This instance.</returns>
    public VTimeZoneProvider Add(string tzId, List<ZoneInterval> intervals)
        => Add(new VDateTimeZone(tzId, intervals));

    /// <summary>
    /// Adds a <see cref="VDateTimeZone"/> to the provider if it does not already exist.
    /// </summary>
    /// <param name="timeZone">The <see cref="VDateTimeZone"/> to add.</param>
    /// <returns>This instance.</returns>
    public VTimeZoneProvider Add(VDateTimeZone timeZone)
    {
#if NET6_0_OR_GREATER
        _customTimeZones.TryAdd(timeZone.Id, timeZone);
#else
        if (!_customTimeZones.ContainsKey(timeZone.Id))
            _customTimeZones.Add(timeZone.Id, timeZone);
#endif
        return this;
    }

    /// <summary>
    /// Removes all <see cref="VDateTimeZone"/>s from the provider.
    /// </summary>
    public void Clear() => _customTimeZones.Clear();

    /// <summary>
    /// Gets the singleton instance of the <see cref="VTimeZoneProvider"/>.
    /// </summary>
    public static VTimeZoneProvider Instance => _instance.Value;

    /// <inheritdoc/>
    public DateTimeZone GetSystemDefault() => DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <inheritdoc/>
    public DateTimeZone? GetZoneOrNull(string tzId)
    {
        // Check for fixed-offset timezones, as described for IDateTimeZoneProvider
        if (tzId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
            return DateTimeZone.Utc;
        
        if (tzId.StartsWith("UTC", StringComparison.OrdinalIgnoreCase))
        {
            var parseResult = NodaTime.Text.OffsetPattern.GeneralInvariant.Parse(tzId.Substring(3));
            if (parseResult.Success)
                return DateTimeZone.ForOffset(parseResult.Value);
        }

        // Check custom timezones
        var found = _customTimeZones.TryGetValue(tzId, out var timeZone);
        return found ? timeZone : null;
    }

    /// <inheritdoc/>
    public string VersionId => "1.0.0";

    /// <inheritdoc/>
    public ReadOnlyCollection<string> Ids => new(_customTimeZones.Keys.ToList());

    /// <inheritdoc/>
    public DateTimeZone this[string id] => _customTimeZones[id];
}
