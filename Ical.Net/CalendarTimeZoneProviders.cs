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

    /// <summary>
    /// The same as <see cref="TzdbWithAliases"/> except time zone
    /// lookups are case-insensitive. This is non-standard and should
    /// not be used unless necessary.
    /// </summary>
    public static IDateTimeZoneProvider TzdbWithAliasesIgnoreCase => _tzdbWithAliasesIgnoreCase.Value;

    private static readonly Lazy<InternalTzdbWithAliasesIgnoreCase> _tzdbWithAliasesIgnoreCase
        = new(static () => new InternalTzdbWithAliasesIgnoreCase(), isThreadSafe: true);

    /// <summary>
    /// Creates a time zone provider using the given list of providers.
    /// </summary>
    public static IDateTimeZoneProvider Combine(params IDateTimeZoneProvider[] providers)
        => new CombinedDateTimeZoneProvider(providers);

    private sealed class CombinedDateTimeZoneProvider : IDateTimeZoneProvider
    {
        private readonly IDateTimeZoneProvider[] providers;

        public ReadOnlyCollection<string> Ids { get; }

        public string VersionId { get; }

        public CombinedDateTimeZoneProvider(params IDateTimeZoneProvider[] providers)
        {
            this.providers = providers;

            // Create list of supported IDs
            var idSet = new HashSet<string>(StringComparer.Ordinal);
            foreach (var provider in providers)
            {
                foreach (var id in provider.Ids)
                {
                    idSet.Add(id);
                }
            }

            var idList = new List<string>(idSet);
            idList.Sort(StringComparer.Ordinal);
            Ids = new ReadOnlyCollection<string>(idList);

            // Create an ID that describes all providers
            VersionId = "combined: " + string.Join(", ", providers.Select(x => x.VersionId));
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

        public DateTimeZone GetSystemDefault()
        {
            foreach (var provider in providers)
            {
                try
                {
                    return provider.GetSystemDefault();
                }
                catch (DateTimeZoneNotFoundException) { }
            }

            throw new DateTimeZoneNotFoundException(FormattableString.Invariant($"System default time zone is unknown to source {VersionId}"));
        }

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

    /// <summary>
    /// The TZDB but allows time zone ID to match by common aliases
    /// with case-insensitive matching.
    /// </summary>
    private sealed class InternalTzdbWithAliasesIgnoreCase : IDateTimeZoneProvider
    {
        private readonly Dictionary<string, string> _windowsMappingIgnoreCase =
            TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping
                .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// TZDB ID lookup with case-insensitive matching. This would be better
        /// as a HashSet, but netstandard2.0 does not allow getting the actual
        /// value from the HashSet.
        /// </summary>
        private readonly Dictionary<string, string> _tzdbIgnoreCase =
            DateTimeZoneProviders.Tzdb.Ids
                .ToDictionary(x => x, x => x, StringComparer.OrdinalIgnoreCase);

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

            DateTimeZone? zone = null;

            if (_tzdbIgnoreCase.TryGetValue(id, out var tzdbId)
                || _tzdbIgnoreCase.TryGetValue(id.Replace("-", "/"), out tzdbId)
                || _windowsMappingIgnoreCase.TryGetValue(id, out tzdbId))
            {
                zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzdbId);
            }

            return zone;
        }
    }
}
