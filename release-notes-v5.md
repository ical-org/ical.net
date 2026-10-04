# Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

## v5

### 5.2.2 - (2026-05-08)

* Feat: Add alarm poll end time (#940)
* Chore: Remove obsolete alarm `Procedure`
* Chore: Remove obsolete `RecurrenceId`

### 5.2.1 - (2026-02-09)

* Test: Update `icalrecur_test.txt` from libical master (#926)
* Chore: Update release notes for v5.2.1 (#927)

### 5.2.0 - (2025-12-24)

#### Implemented `BYDAY` with offset and limiting behavior

The following `RRULE` cases are now implemented

1. YEARLY + BYMONTH + numeric BYDAY offsets. Pattern: `FREQ=YEARLY;BYMONTH=6,9;BYDAY=2MO`
2. YEARLY + numeric BYDAY without BYMONTH. Pattern: `FREQ=YEARLY;BYDAY=20MO`
3. YEARLY + BYMONTH + negative numeric BYDAY. Pattern: `FREQ=YEARLY;BYMONTH=6,9;BYDAY=-1SU`

### 5.1.4 - (2025-12-13)

Fix: Regression introduced in v5.1.3 affecting `RRULE:FREQ=YEARLY` when negative `BYMONTHDAY` or `BYYEARDAY` values are used. In certain combinations the evaluator could normalize negative positions incorrectly and skip valid occurrences (e.g., end‑of‑month or end‑of‑year instances). This has been fixed — please upgrade to v5.1.4.

Example of an affected rule (prior behavior could skip results):
```ics
DTSTART:20250101
RRULE:FREQ=YEARLY;BYYEARDAY=-1,1;BYMONTHDAY=-1,1
```

### 5.1.3 - (2025-12-01)

Fix: Correct handling `RRULE:FREQ=YEARLY` combined with `BYMONTH` and `BYWEEKNO`. The previous implementation could skip occurrences in some scenarios.
Fix: Correct handling `RRULE:FREQ=YEARLY` when `BYMONTH` ist missing, e.g. `RRULE:FREQ=YEARLY;INTERVAL=2;BYDAY=MO,TU`. Now takes the month of `DTSTART` as a limiter.

### 5.1.2 - (2025-11-13)

* Chore: Mark classes and interfaces using `EXRULE` as obsolete. Reasoning: `EXRULE` is marked as deprecated in RFC 5545. Neither Google Calendar nor Microsoft Outlook/Exchange support it.
* Feat: `CalDateTime.Add(Duration)` for empty `Duration`s. The methods gets called frequently during recurrence evaluation. Here it brings a performance improvement of about 10%.
* Fix: `FREQ=WEEKLY` rules that include `BYMONTH` were only including weeks that **start** inside the given months. This fix checks the end of the week also to see if **any part of the week** is inside the given months.

### 5.1.1 - (2025-10-06)

* Fix: `CalendarEvent`s with `RecurrenceId` were not properly evaluated in some scenarios.
* Feat: `RecurringComponent`s like `CalendarEvent` support `RECURRENCE-ID` with optional `RANGE` parameter for serialization and deserialization.
* Wiki: Updated articles about recurrence evaluation

### 5.1.0 - (2025-07-16)

* Fix: Exception for Blazor WebAssembly and Self-Contained Assemblies using `FileVersionInfo` 
* Fix: GetOccurrences() NPE when returning a ToDo's occurrences that don't have a duration
* Fix: Evaluation of `EXDATE` when date-only while `DTSTART` is date-time
* Feat: Use UTF-8 Encoding without BOM by default in all serializers when writing to a stream. This is expected by most iCalendar consumers, including Outlook and Google Calendar.
* Fix: `CalDateTime` CTOR using ISO 8601 UTC string resolves to UTC
* Fix: `GetOccurrences(periodStart)` to also include ongoing occurrences (beginning before `periodStart`)
* Fix: `GetOccurrences()` not properly dealing with `periodStart`'s timezone ID

### 5.0.0 GA - (2025-06-17)

* **Breaking:** Remove redundant `Equals` and `GetHashCode` implementations in https://github.com/ical-org/ical.net/pull/810
* **Breaking:** `Occurrence.Period` is determined by `StartTime` and `Duration` only in https://github.com/ical-org/ical.net/pull/808
* Update package versions in https://github.com/ical-org/ical.net/pull/813 which includes `NodaTime` version 3.2.2
* Fix: `Period.CollidesWith` calculation in https://github.com/ical-org/ical.net/pull/812
* Added a **[Migration Guide for v4 to v5](https://github.com/ical-org/ical.net/wiki/Migrating-Guides)** to the wiki
* Update the list of **[API Changes from v4 to v5](https://github.com/ical-org/ical.net/wiki/API-Changes-v4-to-v5)** in the wiki
* Publish v5.0.0 GA


### 5.0.0-pre.43 - (2025-05-21)

* **Breaking:** Enable NRT project wide in https://github.com/ical-org/ical.net/pull/769, https://github.com/ical-org/ical.net/pull/771, https://github.com/ical-org/ical.net/pull/772, https://github.com/ical-org/ical.net/pull/778, https://github.com/ical-org/ical.net/pull/786. NuGet Packages are now published with NRT enabled.
* Update license.md in https://github.com/ical-org/ical.net/pull/773
* EvaluationOptions: Fix off-by-one issue of `MaxUnmatchedIncrementsLimit` https://github.com/ical-org/ical.net/pull/775
* RecurrencePatternEvaluator: Modernize some evaluation code in https://github.com/ical-org/ical.net/pull/783
* **Breaking:** Evaluation: Remove `periodEnd` param from `GetOccurrences` et al in https://github.com/ical-org/ical.net/pull/781. To limit the elements, it's recommended to used `CollectionExtensions.TakeWhileBefore` (see below), or simple `TakeWhile`.
* Implement `CollectionExtensions.TakeWhileBefore` extensions in https://github.com/ical-org/ical.net/pull/796. This can e.g. be used on enumerations from `GetOccurrences` methods
* Evaluation: Raise `EvaluationOutOfRangeException` if year 10k is hit during evaluation in https://github.com/ical-org/ical.net/pull/785
* Remove unnecessary null checks in https://github.com/ical-org/ical.net/pull/790
* **Breaking:** Refactor handling of FREQ in recurrence pattern in https://github.com/ical-org/ical.net/pull/789. Removed `FrequencyType.None` from enum `FrequencyType`
* **Breaking:** Remove `GroupedListEnumerator` in https://github.com/ical-org/ical.net/pull/793 (Different solution made it redundant).
* Enable `CA1305` warnings and fix them in https://github.com/ical-org/ical.net/pull/794
* Fix for serialization of property parameters: `CalendarComponent.AddProperty` adds the `CalendarProperty` in https://github.com/ical-org/ical.net/pull/801
* **Breaking:** Feature: Serialize multiple categories and resources to one line in https://github.com/ical-org/ical.net/pull/803 and https://github.com/ical-org/ical.net/pull/804

### 5.0.0-pre.42 - (2025-04-12)

* Fix incorrect handling of UNTIL if falling into DST change and some related improvements in https://github.com/ical-org/ical.net/pull/738
* Fix: Minor NRT warnings with Recurrence in https://github.com/ical-org/ical.net/pull/743
* Fix: Benchmarks in https://github.com/ical-org/ical.net/pull/746
* Replace `DateTime` with `CalDateTime` in `RecurrencePatternEvaluator` and related code in https://github.com/ical-org/ical.net/pull/742
* Evaluation: Make `MaxIncrementCount` configurable in https://github.com/ical-org/ical.net/pull/750
* Fix issue with `BYWEEKNO=1` where `UNTIL` lies in the year prior to the year of the week of the last occurrence. in https://github.com/ical-org/ical.net/pull/752
* Remove `IServiceProvider` in https://github.com/ical-org/ical.net/pull/753
* Update `PRODID` and `VERSION` property handling in https://github.com/ical-org/ical.net/pull/748
* Enhance `RecurrencePatternSerializer` in https://github.com/ical-org/ical.net/pull/758
* Evaluation: Avoid dependency on local culture settings. in https://github.com/ical-org/ical.net/pull/759
* Change `DateTime` method args to `CalDateTime` in https://github.com/ical-org/ical.net/pull/761
* Enable NRT in https://github.com/ical-org/ical.net/pull/762, https://github.com/ical-org/ical.net/pull/763, https://github.com/ical-org/ical.net/pull/764, https://github.com/ical-org/ical.net/pull/765. Note: The current packages are created with `NRT` disabled, The v5 final release will be fully NRT compliant.
* Fix positive/nagative args in `Duration` CTOR in https://github.com/ical-org/ical.net/pull/767

### 5.0.0-pre.41 - (2025-02-20)

  * Make the time zone resolver plugable
  * Make `CalendarEvent.EffectiveDuration` and some conversion functions public.
  * Fix: Incorrect expansion behaviour after `BYWEEKNO`

### 5.0.0-pre.40 - (2025-02-15)

  * Fix: Derive correct file and assembly version from package version in https://github.com/ical-org/ical.net/pull/726
  * Fix inverted limiting behavior of `BYMONTHDAY` by @minichma in https://github.com/ical-org/ical.net/pull/730

### 5.0.0-pre.39 - (2025-02-12)
  * This is the first public pre-release of the next major version of **Ical.Net**. It's an extensive rewrite of the library, with a focus on performance, correctness and usability. All issues reported in prior versions have been addressed, and the library has been thoroughly tested, also using the [libical](https://github.com/libical/libical) test suite.
  * We strongly recommend using the pre-release packages, as they are more stable and feature-complete than the v4.x versions.
  * Feedback is highly appreciated.
  * Breaking changes from v4 are currently listed [here](https://github.com/ical-org/ical.net/wiki/API-Changes-v4-to-v5).
