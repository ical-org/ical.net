# Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

> Release notes for older major versions are included in separate files:
> - [release-notes-v5.md](release-notes-v5.md) — v5.x releases
> - [release-notes-v4.md](release-notes-v4.md) — v4.x and earlier releases

## v6

### 6.0.0-pre.3 — Pre-release (2026-11-26)

#### Fixes

- Fix folding of long content lines that mix multi-byte and ASCII text ([#1022](https://github.com/ical-org/ical.net/pull/1022))
- Preserve `DTSTART` time of day when it falls in a daylight saving gap ([#1031](https://github.com/ical-org/ical.net/pull/1031))
- Prevent the BY-rule value cache from being lost when copying readonly structs ([#1023](https://github.com/ical-org/ical.net/pull/1023))
- Ignore duplicate BY values when applying `BYSETPOS` ([#1032](https://github.com/ical-org/ical.net/pull/1032))
- Fix missing and out-of-order relative alarm occurrences ([#1029](https://github.com/ical-org/ical.net/pull/1029))
- Skip empty alarm sets and merge pre-sorted alarm fire times in the alarm queue ([#1029](https://github.com/ical-org/ical.net/pull/1029))
- Preserve ordering for end-related alarms when a later occurrence ends earlier ([#1029](https://github.com/ical-org/ical.net/pull/1029))
- Fix index-based access to grouped lists across groups ([#1030](https://github.com/ical-org/ical.net/pull/1030))

#### Enhancements

- Set NodaTime 3.1.0 as the minimum supported version ([#1025](https://github.com/ical-org/ical.net/pull/1025))
- Reduce allocations when formatting `CalDateTime` values ([#1021](https://github.com/ical-org/ical.net/pull/1021))
- Reduce allocations in duration, period, UTC offset, and parameter serializers ([#1026](https://github.com/ical-org/ical.net/pull/1026))

### 6.0.0-pre.2 — Pre-release (2026-10-31)

- fix: An issue with escaping invalid characters was fixed ([#1018](https://github.com/ical-org/ical.net/pull/1018)))
- fix: Keep the `DTSTART` time of day after a daylight saving gap ([#1016](https://github.com/ical-org/ical.net/pull/1016))
- fix: Stop `RRULE` evaluation at `UNTIL` when `BY` parts never match ([#1008](https://github.com/ical-org/ical.net/pull/1008))
- fix: `VTimeZone.FromSystemTimeZone` for southern-hemisphere time zones ([#1013](https://github.com/ical-org/ical.net/pull/1013))

### 6.0.0-pre.1 — Pre-release (2026-10-05)

#### Summary

This is a **major release** dominated by three structural rewrites:
1. A simplified, non-arithmetic, unambiguous `CalDateTime` type.
2. A new recurrence/occurrence evaluation engine built on NodaTime `ZonedDateTime`, dropping multi-`RRULE` and `EXRULE` support.
3. A `Calendar`-scoped time zone provider replacing the old static resolver/`DateUtil` infrastructure.

Alarm evaluation and TODO completion logic were also significantly reworked for RFC 5545 compliance, and target frameworks were updated. Given the volume of removed/renamed public API surface (`CalDateTime.Value`, `AsUtc`, `ToTimeZone`, `DateUtil`, `TimeZoneResolvers`, `Evaluator`, `ILoaded`, multi-RRULE/EXRULE support), consumers should expect to need code changes when upgrading from v5.2.x to v6.

#### Breaking Changes

- **`CalDateTime` overhaul** — this is the single biggest source of breaking changes in v6:
  - Removed date/time arithmetic from `CalDateTime` ([#930](https://github.com/ical-org/ical.net/pull/930))
  - Removed `CalDateTime.Value`, replaced with `ToDateTimeUnspecified()` method
  - Removed `CalDateTime.AsUtc`. Use `ToZonedDateTime(...)` or ` `ToZonedOrDefault(...)` and  convert using `ToDateTimeUtc()`
  - Removed `CalDateTime.ToTimeZone`
  - Removed invalid/ambiguous `CalDateTime` constructors; constructors are now unambiguous
  - Changed to `CalDateTime.FromDateTime` factory usage patterns
  - Case-insensitive hash code behavior
- **New recurrence/occurrence engine**:
  - Brand new recurrence evaluator replacing the legacy one ([#901](https://github.com/ical-org/ical.net/pull/901))
  - Removed `Evaluator` base infrastructure
  - Removed support for multiple `RRULE`s per component ([#929](https://github.com/ical-org/ical.net/pull/929)) — only one `RRULE` per component is now supported, per RFC 5545
  - Removed `ILoaded`
  - Removed `EXRULE` support, marked obsolete since v5 ([#916](https://github.com/ical-org/ical.net/pull/916))
  - Occurrences now evaluated as `ZonedDateTime` (NodaTime), with "Zoned occurrence using NodaTime" ([#854](https://github.com/ical-org/ical.net/pull/854)) as a foundational change
  - Occurrence.Period.StartTime, Occurrence.Period.EndTime, and Occurrence.Period.Duration are replaced with Occurrence.Start, Occurrence.End

- **Time zone handling rewritten**:
  - New `Calendar`-scoped time zone provider system, including a combined/default provider and case-insensitive lookups
  - Support for custom `VTIMEZONE` IDs and `VTIMEZONE` info with `RDATE`
- **Alarms**: Major overhaul of alarm evaluation for RFC 5545 compliance (`feat(alarms)!`), including buffered evaluation, alarm poll end time, and removal of obsolete alarm procedures
- **TODO handling**: Completion status now determined from properties only; completed date separated from status
- **Recurrence pattern API deprecations**: `RecurrencePattern`, `RecurrencePatternSerializer`, and `RecurrenceRules` deprecated in favor of `RecurrenceRule` / `RecurrenceRuleSerializer`
- **Target framework update** — "breaking: Update target frameworks" ([#905](https://github.com/ical-org/ical.net/pull/905)), aligning with the current TFMs (.NET 8/10, .NET Standard 2.0)
- **Culture-specific string handling removed** — stop using culture-specific string parsing/formatting ([#954](https://github.com/ical-org/ical.net/pull/954))

#### New Features / Enhancements

- New **UTF-8 serializer** buffered content-line reading for performance
- `SimpleDeserializer` with improved performance ([#912](https://github.com/ical-org/ical.net/pull/912))
- Preserve parameters when copying calendars ([#987](https://github.com/ical-org/ical.net/pull/987))
- Escape backslash and split multi-value `TEXT` on unescaped separator commas (RFC 5545 TEXT escaping correctness)
- `FreeBusy` attendee filtering and status logic refactor ([#898](https://github.com/ical-org/ical.net/pull/898))
- Nullable reference types enabled in `Ical.Net.Tests.csproj` ([#907](https://github.com/ical-org/ical.net/pull/907))
- Reduced complexity of `GetAlarmOccurrences`
- Stop todo evaluation at a given datetime ([#915](https://github.com/ical-org/ical.net/pull/915))

#### Fixes

- Fix `VTimeZone` ↔ `DateTimeZone` conversion and combined time zone id list
