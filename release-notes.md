# Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

> Release notes for older major versions are included in separate files:
> - [release-notes-v5.md](release-notes-v5.md) — v5.x releases
> - [release-notes-v4.md](release-notes-v4.md) — v4.x and earlier releases

## v6

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
  - Removed `CalDateTime.Value`, replaced with `ToDateTime()` method
  - Removed `CalDateTime.AsUtc`, replaced with `ToDateTimeUtc()` method
  - Removed `CalDateTime.ToTimeZone`
  - Removed invalid/ambiguous `CalDateTime` constructors; constructors are now unambiguous
  - Removed `Instant` from `CalDateTime`; removed `ToZoneDateTime`
  - Added `Kind` parameter to `CalDateTime.ToDateTime`
  - Changed to `CalDateTime.FromDateTime` factory usage patterns
  - Case-insensitive hash code behavior
- **New recurrence/occurrence engine**:
  - Brand new recurrence evaluator replacing the legacy one ([#901](https://github.com/ical-org/ical.net/pull/901))
  - Removed `Evaluator` base infrastructure
  - Removed support for multiple `RRULE`s per component ([#929](https://github.com/ical-org/ical.net/pull/929)) — only one `RRULE` per component is now supported, per RFC 5545
  - Removed `ILoaded`
  - Removed `EXRULE` support, marked obsolete since v5 ([#916](https://github.com/ical-org/ical.net/pull/916))
  - Occurrences now evaluated as `ZonedDateTime` (NodaTime), with "Zoned occurrence using NodaTime" ([#854](https://github.com/ical-org/ical.net/pull/854)) as a foundational change
  - Recurrence ID matching now by `Instant`
- **Time zone handling rewritten**:
  - New `Calendar`-scoped time zone provider system, including a combined/default provider and case-insensitive lookups
  - Removed `DateUtil`, `TimeZoneResolvers`, and `DefaultTimeZoneResolver`
  - Support for custom `VTIMEZONE` IDs and `VTIMEZONE` info with `RDATE`
- **Alarms**: Major overhaul of alarm evaluation for RFC 5545 compliance (`feat(alarms)!`), including buffered evaluation, alarm poll end time, and removal of obsolete alarm procedures
- **TODO handling**: Completion status now determined from properties only; completed date separated from status
- **Recurrence pattern API deprecations**: `RecurrencePattern`, `RecurrencePatternSerializer`, and `RecurrenceRules` deprecated in favor of `RecurrenceRule` / `RecurrenceRuleSerializer`
- **Target framework update** — "breaking: Update target frameworks" ([#905](https://github.com/ical-org/ical.net/pull/905)), aligning with the current TFMs (.NET 8/10, .NET Standard 2.0, .NET Framework 4.8)
- **Culture-specific string handling removed** — stop using culture-specific string parsing/formatting ([#954](https://github.com/ical-org/ical.net/pull/954))
- Removed unused expand grid and simplified occurrence end-time calculation

#### New Features / Enhancements

- New **UTF-8 serializer** path with BOM skipping and buffered content-line reading for performance
- `SimpleDeserializer` with improved performance ([#912](https://github.com/ical-org/ical.net/pull/912))
- Preserve parameters when copying calendars ([#987](https://github.com/ical-org/ical.net/pull/987))
- Escape backslash and split multi-value `TEXT` on unescaped separator commas (RFC 5545 TEXT escaping correctness)
- `FreeBusy` attendee filtering and status logic refactor ([#898](https://github.com/ical-org/ical.net/pull/898))
- Nullable reference types enabled in `Ical.Net.Tests.csproj` ([#907](https://github.com/ical-org/ical.net/pull/907))
- Reduced complexity of `GetAlarmOccurrences`; added tests for alarms without a trigger
- Stop todo evaluation at a given datetime ([#915](https://github.com/ical-org/ical.net/pull/915))

#### Fixes

- Fix `VTimeZone` ↔ `DateTimeZone` conversion and combined time zone id list
- Fix CalDateTime serializer documentation
- Fix warnings and comments in serializer code
- Fix incorrect merge conflict resolution / restore consistency during the `version/6.0` branch work
- Readme: fix duplicate text; update README

#### Chores / Tooling / CI

- Multiple `dependabot` bumps: `codecov/codecov-action` (5.5.1 → 7.0.0 across several steps), `actions/checkout` (6 → 7), `actions/upload-artifact` (5 → 6 → 7), `actions/setup-dotnet` (5 → 6)
- Force CRLF line endings / no BOM for `.ics` test fixtures per RFC 5545 ([#984](https://github.com/ical-org/ical.net/pull/984))
- Update encoding and line endings for consistency ([#932](https://github.com/ical-org/ical.net/pull/932))
- Update GitHub Workflows ([#936](https://github.com/ical-org/ical.net/pull/936))
- Fix unit tests with LF line-ending issues in raw strings ([#938](https://github.com/ical-org/ical.net/pull/938))
- Update `icalrecur_test.txt` from libical master ([#926](https://github.com/ical-org/ical.net/pull/926))
- Use `BenchmarkSwitcher` for benchmarks ([#982](https://github.com/ical-org/ical.net/pull/982))
- Remove `nosonar` suppressions
- Change some NodaTime constructors to static
- Various test additions/updates for `CalDateTime`, recurrence rules, alarms, and serializers


