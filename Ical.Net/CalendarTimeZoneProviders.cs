//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NodaTime;
using NodaTime.TimeZones;

namespace Ical.Net;

public static class CalendarTimeZoneProviders
{
    /// <summary>
    /// Gets a time zone provider that returns time zones from the TZDB,
    /// allowing for common aliases and mappable Windows IDs to be used
    /// to find the canonical TZDB time zones.
    /// </summary>
    public static readonly IDateTimeZoneProvider TzdbWithAliases = new InternalTzdbWithAliases();

    public static IDateTimeZoneProvider FromCalendar(Calendar calendar, params IDateTimeZoneProvider[] fallbackProviders)
    {
        var calendarProvider = new DateTimeZoneCache(calendar.TimeZoneSource);
        var combinedProvider = new CombinedDateTimeZoneProvider([calendarProvider, .. fallbackProviders]);
        return combinedProvider;
    }

    private sealed class CombinedDateTimeZoneProvider : IDateTimeZoneProvider
    {
        private readonly IDateTimeZoneProvider[] providers;

        public ReadOnlyCollection<string> Ids { get; }

        /// <summary>
        /// A map of IDs to providers so that the same ID
        /// always uses the same time zone provider.
        /// </summary>
        private readonly ReadOnlyDictionary<string, IDateTimeZoneProvider> providerMap;

        public CombinedDateTimeZoneProvider(params IDateTimeZoneProvider[] providers)
        {
            this.providers = providers;

            var idMap = new Dictionary<string, IDateTimeZoneProvider>();
            foreach (var provider in providers)
            {
                foreach (var id in provider.Ids)
                {
                    if (!idMap.ContainsKey(id))
                    {
                        idMap.Add(id, provider);
                    }
                }
            }

            providerMap = new ReadOnlyDictionary<string, IDateTimeZoneProvider>(idMap);

            // List supported IDs as the from the ID mapping to
            // make sure all IDs are unique.
            var idList = new List<string>(providerMap.Keys);
            idList.Sort(StringComparer.Ordinal);
            Ids = new ReadOnlyCollection<string>(idList);
        }

        public DateTimeZone this[string id]
        {
            get
            {
                var zone = GetZoneOrNull(id)
                    ?? throw new DateTimeZoneNotFoundException($"Time zone ID {id} is unknown");

                return zone;
            }
        }

        public string VersionId => field ??= "combined: " + string.Join(", ", providers.Select(x => x.VersionId));


        public DateTimeZone GetSystemDefault() => throw new NotImplementedException();

        public DateTimeZone? GetZoneOrNull(string id)
        {
            foreach (var source in providers)
            {
                var zone = source.GetZoneOrNull(id);

                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }
    }


    /// <summary>
    /// The TZDB but allows time zone ID to match by common aliases.
    /// </summary>
    private sealed class InternalTzdbWithAliases : IDateTimeZoneProvider
    {
        public DateTimeZone this[string id]
        {
            get
            {
                var zone = GetZoneOrNull(id)
                    ?? throw new DateTimeZoneNotFoundException($"Time zone ID {id} could not be found in TZDB");

                return zone;
            }
        }

        public string VersionId => DateTimeZoneProviders.Tzdb.VersionId;

        /// <summary>
        /// This returns the canonical TZDB list only.
        /// </summary>
        public ReadOnlyCollection<string> Ids => DateTimeZoneProviders.Tzdb.Ids;

        public DateTimeZone GetSystemDefault() => DateTimeZoneProviders.Tzdb.GetSystemDefault();

        public DateTimeZone? GetZoneOrNull(string id)
        {
            // RFC allows a prefixing "/" to indicate a "unique ID
            // in a globally defined time zone registry." Ignore since
            // this is already searching the TZDB.
            if (id.StartsWith("/", StringComparison.Ordinal))
            {
                id = id.Substring(1);
            }

            var p = DateTimeZoneProviders.Tzdb;
            var zone = p.GetZoneOrNull(id);
            if (zone is not null)
            {
                return zone;
            }

            if (TzdbDateTimeZoneSource.Default.WindowsToTzdbIds.TryGetValue(id, out var tzdbId))
            {
                zone = p.GetZoneOrNull(tzdbId);
                if (zone is not null)
                {
                    return zone;
                }
            }

            return p.GetZoneOrNull(id.Replace("-", "/"));
        }
    }
}
