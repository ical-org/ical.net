//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ical.Net.CalendarComponents;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net.TimeZone;

/// <summary>
/// Represents a provider that allows for adding custom timezones
/// created from <see cref="VTimeZone"/>s.
/// </summary>
public class CalTimeZoneProvider : IDateTimeZoneProvider
{
    private readonly Dictionary<string, DateTimeZone> _timeZones = new();

    /// <summary>
    /// Adds a <see cref="DateTimeZone"/> to the provider if it does not already exist.
    /// </summary>
    /// <param name="timeZone">The <see cref="DateTimeZone"/> to add.</param>
    private void Add(DateTimeZone timeZone)
    {
#if NET6_0_OR_GREATER
        _timeZones.TryAdd(timeZone.Id, timeZone);
#else
        if (!_timeZones.ContainsKey(timeZone.Id))
            _timeZones.Add(timeZone.Id, timeZone);
#endif
    }

    /// <summary>
    /// Adds a <see cref="DateTimeZone"/> to the provider.
    /// </summary>
    /// <param name="tzId">The timezone ID</param>
    /// <param name="intervals">The list of <see cref="ZoneInterval"/>s for the timezone ID.</param>
    public void Add(string tzId, ICollection<ZoneInterval> intervals)
        => Add(new CalDateTimeZone(tzId, intervals));

    /// <summary>
    /// Converts all <see cref="VTimeZone"/>s to <see cref="DateTimeZone"/>s
    /// and adds them to the provider.
    /// <para/>
    /// The <see cref="VTimeZone.TzId"/> is used as the <see cref="DateTimeZone.Id"/>.
    /// </summary>
    /// <param name="timeZones"></param>
    public void AddRangeFrom(ICollection<VTimeZone> timeZones)
    {
        foreach (var dateTimezone in timeZones.ToDateTimeZones())
        {
            Add(dateTimezone);
        }
    }

    /// <summary>
    /// Removes a <see cref="DateTimeZone"/> from the provider by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>
    /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.
    /// </returns>
    public bool Remove(string id) => _timeZones.Remove(id);

    /// <summary>
    /// Clears all time zones from the provider.
    /// </summary>
    public void Clear() => _timeZones.Clear();

    /// <inheritdoc/>
    public DateTimeZone? GetZoneOrNull(string id)
    {
        // Check for fixed-offset timezones, as described for IDateTimeZoneProvider
        if (id.Equals(DateTimeZone.Utc.Id, StringComparison.OrdinalIgnoreCase))
            return DateTimeZone.Utc;

        if (id.StartsWith(DateTimeZone.Utc.Id, StringComparison.OrdinalIgnoreCase))
        {
            var parseResult = NodaTime.Text.OffsetPattern.GeneralInvariant.Parse(id.Substring(3));
            if (parseResult.Success)
                return DateTimeZone.ForOffset(parseResult.Value);
        }

        return _timeZones.TryGetValue(id, out var zone) ? zone : null;
    }

    /// <inheritdoc/>
    public string VersionId => LibraryMetadata.AssemblyFileVersion;

    /// <inheritdoc/>
    public ReadOnlyCollection<string> Ids => new(_timeZones.Keys.ToList()); // NOSONAR - part of IDateTimeZoneProvider

    /// <inheritdoc/>
    public DateTimeZone GetSystemDefault() => DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <inheritdoc/>
    public DateTimeZone this[string id] => GetZoneOrNull(id) ?? throw new ArgumentOutOfRangeException(nameof(id));
}
