# Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

## v5

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

## v4
* 4.3.1 - (2024-10-14)
  * Update Ical.Net.csproj to use NodaTime 3.2.0 instead of 3.1.12. NodaTime v3.2.0 which brings some welcome changes
  * Replace Ical.Net.nuspec with Directory.Build.props
  * Remove unnecessary nuget dependencies, so `NodaTime` is again the only one
* 4.3.0 - (2024-10-13)
  * Update the repository from fork `laget.se/ical net`
  * * Remove net5.0 as target framework from all projects
  * Merged a few pull requests  that fixes a few bugs
    * #584, #579, #571, #528, #525, #471, #470, #443
  * Update CI workflows `publish.yml` and `tests.yml`
  * Update ProdId constant and NodaTime package version
  * Add back assembly signing to projects and include strong name key
  * Add class `RegexDefaults` and update all Regex with `RegexDefaults.Timeout` which is set to 200 milliseconds
* 4.2.0 - (2021-04-10) - Many bugbixes from the community
  * Fix infinite loop with MaxDate for GetOccurrences #364
  * Deserializes STANDARD and DAYLIGHT timezone infos #420
  * BYWEEKNO & BYMONTHNO fix for ISO-8601 formatting #463
  * Fixed bug where changing a property value appended the value instead of clearing it. #450
  * Fixed `IsActive` regression #449
  * Fixed bug where ordering of week numbers mattered for equality #513
  * Target `netstandard20` and `net50`, unified build targets
  * Updated to NodaTime 3 and `netcoreapp3.1` for unit tests (later changed to `net50`) #449
* 4.1.11 - (2019-03-21) - Add some conditional debug symbols so VSTS doesn't choke on strong-named assemblies. Thanks, [eriknuds](https://github.com/eriknuds) #442
* 4.1.10 - (2019-01-31) - Strong-named assemblies. Thanks, [josteink](https://github.com/josteink) #159
* 4.1.9 - (2018-07-18) - Associate attachments with their events when VALUE is BINARY. Without the association, parameters such as FMTTYPE would be lost. [PR 411](https://github.com/rianjs/ical.net/pull/411)
* 4.1.8 - (2018-06-12) - `PRODID` and `VERSION` are unconditionally set when serializing a calendar so as not to mislead consumers about where the serialized output came from. Previous behavior would use the values that came from deserialization. #403
* 4.1.6 - (2018-06-01) - Target `net46` instead of `net45`. It seems that [`System.Reflection.TypeExtensions`](https://www.nuget.org/packages/System.Reflection.TypeExtensions/), doesn't support framework versions below 4.6. Bummer.
* 4.1.5 - (2018-05-28)
  * Target `net45` instead of `net46` which opens up the applications that can consume the latest version! [#392](https://github.com/rianjs/ical.net/issues/392)
  * NodaTime doesn't specify an end times for time zones which don't observe DST (such as `America/Phoenix`). We need to handle unbounded end times. [#378](https://github.com/rianjs/ical.net/issues/378)
  * Better `null` handling for `CalDateTime`s. [#372](https://github.com/rianjs/ical.net/issues/372)
* 4.1.4 - (2018-05-24) - [#392](https://github.com/rianjs/ical.net/issues/392). .NET Standard (still) doesn't play nicely with .NET Framework applications, so I have also targeted .NET 4.6 (`net46`). As a consequence of that, I had to downgrade `System.Reflection.TypeExtensions` to 4.1.0, because the original targeted version (4.3.0) was retargeted as `netstandard2.0`, rendering it incompatible with a `netstandard1.3` library.
* 4.1.2 - (2018-05-21) - [#388](https://github.com/rianjs/ical.net/issues/388). Bugfix: Specifying a time zone identifier in the `CalDateTime` constructor should override the backing `DateTime`'s `Kind` property. If no time zone identifier is specified, then the `Kind` property shouldn't be changed.
* 4.1.1 - (2018-05-21) - [#387](https://github.com/rianjs/ical.net/issues/387). Bugfix: Calling `CalDateTime.AsUtc` caches the UTC time, but the cache was not being reset if the `TzId` property changed.
* 4.1.0/4.0.7  - (2018-05-15) - [#383](https://github.com/rianjs/ical.net/issues/383). Add a read-only `AsDateTimeOffset` property to `IDateTime`. Add a few docs to `RecurrencePattern`.
* 4.0.6 - (2017-11-28) - [#344](https://github.com/rianjs/ical.net/issues/344). Fix the VERSION property so it's 2.0 as RFC-5545 requires.
* 4.0.4 [PR 341](https://github.com/rianjs/ical.net/pull/341). Cache the UTC representation for `CalDateTime`s. This results in a 12-16% reduction in unit test runtime.
* 4.0.3 [#337](https://github.com/rianjs/ical.net/issues/337). Fixed a bug in `SimpleDeserializer` where tab characters (`\t`) were excluded from regex match.
* 4.0.1 [#335](https://github.com/rianjs/ical.net/issues/335). Technically this should be 5.0, but given 4.0.0 only has 24 downloads, I've just done a minor version bump.
  * Moved everything from Ical.Net.Collections into the Ical.Net assembly.
  * Updated the `PRODID` stamp to say 4.0.
  * Fixed a bug with a missing nuget dependency
* 4.0.0:
  * `DateTimeKind` is preserved during serialization round trips. `Z` suffixed (i.e. UTC) `DATE-TIME`s produce a `DateTimeKind.Utc`, and everything else produces a `DateTimeKind.Local`. Similarly, when creating `CalDateTime`s with ambiguous timezones, the `DateTimeKind` is examined in an attempt to infer whether it's a UTC time or not. In order of importance, the way of determining `DateTimeKind` is: time zone id, `DateTimeKind` on the incoming `DateTime`, and then a fallback. This should improve interop with other ical libraries like Telerik. [#331](https://github.com/rianjs/ical.net/issues/331)
  * `RRULE`'s `UNTIL` property is now inclusive, and doesn't rely on UTC time comparisons. [#320](https://github.com/rianjs/ical.net/issues/320)
  * `CalDateTime.ToTimeZone` produces the correct local time [#330](https://github.com/rianjs/ical.net/issues/330)
  * `VEVENT` status should be uppercase during serialization. A subtle (but breaking) change, necessitating a major version bump. [#318](https://github.com/rianjs/ical.net/issues/318)
  * `VTIMEZONE`s are once again serialized, this time pulling historic time zone data from NodaTime -- thanks [beriniwlew](https://github.com/beriniwlew)! [PR 304](https://github.com/rianjs/ical.net/pull/304)
  * Entry points are now consolidated into the right places, and make sense:
    * `CalendarCollection.Load()` loads a `CalendarCollection`
    * `Calendar.Load` loads a `Calendar`.
    * `CalendarComponent.Load()` loads whatever type you're looking for.
  * The ANTLR-based parser is gone. [chescock](https://github.com/chescock)'s `SimpleDeserializer` is used everywhere.

### v3
* 3.0.15: .NET Standard version (aka v3 aka `net-core`) is missing System.Reflection.TypeExtensions dependency. [#326](https://github.com/rianjs/ical.net/issues/326)
* 3.0.14: .NET Standard version (aka v3 aka `net-core`) is missing System.Runtime.Serialization.Primitives dependency. [#324](https://github.com/rianjs/ical.net/issues/324)
* 3.0.13: `DTSTART` is not required for `VTODO` components. [PR 322](https://github.com/rianjs/ical.net/pull/322)
* 3.0.12: Several improvements rolled up:
  * `CalendarEvent` now considers Summary and Description for equality and hashing. [PR 309](https://github.com/rianjs/ical.net/pull/309).
  * Protection against `InvalidOperationException`s in some collections usage scenarios [PR 312](https://github.com/rianjs/ical.net/pull/312)
  * Normalized `Journal` implementation. [PR 310](https://github.com/rianjs/ical.net/pull/310)
* 3.0.11-net-core-beta: Targeting netstandard1.3 and net46
* 3.0.10-net-core-beta: Reverts a change made in 3.0.3 which allowed UTC timestamps to specify `TZID=UTC` instead of being suffixed with `Z`. The spec requires `Z` suffixes, and broke many applications, including Outlook. [#263](https://github.com/rianjs/ical.net/issues/263)
* 3.0.9-net-core-beta: Bugfixes: `PeriodList` now fully implements `IList<Period>`. Keep data structures in sync in GroupedList.Remove() [#253](https://github.com/rianjs/ical.net/issues/253). Fix for StackOverflow exception [#257](https://github.com/rianjs/ical.net/issues/257). UnitTests can now be run in VS test runner!
* 3.0.8-net-core-alpha: Bugfix: Better CalDateTime equality and hashing, because time zones matter. [#275](https://github.com/rianjs/ical.net/issues/275)
* 3.0.7-net-core-alpha: Bugfix: Fixed a small ordering bug when evaluating `EXDATE`s and `RDATE`s [#275](https://github.com/rianjs/ical.net/issues/275)
* 3.0.6-net-core-alpha: Bugfix: `CalendarEvent`'s `Equals()` and `GetHashCode()` were buggy in that they did not consider `RecurrenceDates` and `ExceptionDates` properly. Both methods treated these properties as if they were a single collection. Now they are normalized by time zone when before determining whether complete set of `RDATE`s or `EXDATE`s are the same. [#275](https://github.com/rianjs/ical.net/issues/275)
* 3.0.4-net-core-alpha: `CalendarEvent`'s `Equals()` and `GetHashCode()` methods were asymmetric. Both now consider `Attachment`s. In addition, several calendar components did not implement `Equals()` and `GetHashCode()`, namely `Journal` and `Todo`. They both delegate to `RecurringComponent` for the properties owned by that parent object, and extend the methods for the child properties that they respectively own. [#271](https://github.com/rianjs/ical.net/issues/271)
* 3.0.3-net-core-alpha: Bringing in 2.2.34's changes: serializing `EXDATE`s and `RDATE`s now includes time zone information when specified. UTC timestamps are now allowed to be specified either in `Z`-suffixed form (which was mandatory before), OR explicitly with a time zone id (`TZID=UTC`). This allows greater interop with third-party libraries like Telerik's RadSchedule. [#259](https://github.com/rianjs/ical.net/issues/259) `PeriodList` now implements `IList<Period>`.
* 3.0.2-net-core-alpha: Fix nuspec file to declare NodaTime as dependency
* 3.0.1-alpha: Initial publishing of .NET Core release

### v2
* 2.3.6 - (2019-01-30) - Strong-named assemblies. Thanks, [josteink](https://github.com/josteink) #159
* 2.3.5: `VTIMEZONE`s are once again serialized, this time pulling historic time zone data from NodaTime -- thanks [beriniwlew](https://github.com/beriniwlew)! [PR 304](https://github.com/rianjs/ical.net/pull/304)
* 2.3.4: `DTSTART` is not required for `VTODO` components. [PR 322](https://github.com/rianjs/ical.net/pull/322)
* 2.3.3: Several improvements rolled up:
  * `Event` now considers Summary and Description for equality and hashing. [PR 309](https://github.com/rianjs/ical.net/pull/309).
  * Protection against `InvalidOperationException`s in some collections usage scenarios [PR 312](https://github.com/rianjs/ical.net/pull/312)
  * Normalized `Journal` implementation. [PR 310](https://github.com/rianjs/ical.net/pull/310)
* 2.3.2: Reverts a change made in 2.2.34 which allowed UTC timestamps to specify `TZID=UTC` instead of being suffixed with `Z`. The spec requires `Z` suffixes, and broke many applications, including Outlook. [#263](https://github.com/rianjs/ical.net/issues/263)
* 2.3.0: PeriodList now implements `IList` [#280](https://github.com/rianjs/ical.net/issues/280)
* 2.2.39: Bugfix: Better CalDateTime equality and hashing, because time zones matter. [#275](https://github.com/rianjs/ical.net/issues/275)
* 2.2.38: Bugfix: Fixed a small ordering bug when evaluating `EXDATE`s and `RDATE`s [#275](https://github.com/rianjs/ical.net/issues/275)
* 2.2.37: Bugfix: `Event`'s `Equals()` and `GetHashCode()` were buggy in that they did not consider `RecurrenceDates` and `ExceptionDates` properly. Both methods treated these properties as if they were a single collection. Now they are normalized by time zone when before determining whether complete set of `RDATE`s or `EXDATE`s are the same. [#275](https://github.com/rianjs/ical.net/issues/275)
* 2.2.35: Bugfix: `Event`'s `Equals()` and `GetHashCode()` methods were asymmetric. Both now consider `Attachment`s. In addition, several calendar components did not implement `Equals()` and `GetHashCode()`, namely `Journal` and `Todo`. They both delegate to `RecurringComponent` for the properties owned by that parent object, and extend the methods for the child properties that they respectively own. [#271](https://github.com/rianjs/ical.net/issues/271)
* 2.2.34: Serializing `EXDATE`s and `RDATE`s now includes time zone information when specified. UTC timestamps are now allowed to be specified either in `Z`-suffixed form (which was mandatory before), OR explicitly with a time zone id (`TZID=UTC`). This allows greater interop with third-party libraries like Telerik's RadSchedule. [#259](https://github.com/rianjs/ical.net/issues/259)
* 2.2.33: Bugfix for [#235](https://github.com/rianjs/ical.net/issues/235) when years have 53 weeks. Contains a new deserializer that's twice as fast as the default ANTLR implementation, and several other (smaller) performance enhancements. _This will become the default deserializer in a future release._ [PR 246](https://github.com/rianjs/ical.net/pull/246), [PR 247](https://github.com/rianjs/ical.net/pull/247)
* 2.2.31: .NET's UTC offset parsing semantics don't match the RFC (which allows for `hhmmss` UTC offsets), so I extended the offset serializer to account for these differences. ([#102](https://github.com/rianjs/ical.net/issues/102), [#236](https://github.com/rianjs/ical.net/issues/236))
* 2.2.30: `Event.Resources` is an `IList` again. Event.Resources wasn't being deserialized, so I have reverted back to an IList to fix this. Sorry everyone. I'm intentionally violating my own semver rules. In the future, I'll call version n+1 "vNext" so I can more freely rev the major version number.
* 2.2.29: Calling `GetOccurrences()` on a recurrable component should recompute the recurrence set. Specifying `EXDATE` values that don't have a `TimeOfDay` component should "black out" that day from a recurring component's `StartTime`.[#223](https://github.com/rianjs/ical.net/issues/223)
* 2.2.25: Fix for Collection was modified exception, and better handling of recurrence ids when calling `Calendar.GetOccurrences` ([#188](https://github.com/rianjs/ical.net/issues/188)) ([#148](https://github.com/rianjs/ical.net/issues/148))
* 2.2.24: Performance enhancement for solidus-prefixed time zones ([#204](https://github.com/rianjs/ical.net/issues/204)).
* 2.2.23: Bugfix for culture for geographic location serialization. RFC-5545 requires coordinates to use decimal points, not commas, regardless of culture. Correct: `1.2345`. Incorrect: `1,2345`. ([#202](https://github.com/rianjs/ical.net/issues/202))
* 2.2.22: Bugfix for `Event` serialization that always changed the `Duration` to 0. Serialization shouldn't have side effects. ([#199](https://github.com/rianjs/ical.net/issues/199))

Changes weren't systematically tracked before 2.2.22.
