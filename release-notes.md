## Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

### v3
* 3.0.4-net-core-alpha: All calendar components now explicitly implement IEquatable<T>. [PR 262](https://github.com/rianjs/ical.net/pull/262)
* 3.0.3-net-core-alpha: Bringing in 2.2.34's changes: serializing `EXDATE`s and `RDATE`s now includes time zone information when specified. UTC timestamps are now allowed to be specified either in `Z`-suffixed form (which was mandatory before), OR explicitly with a time zone id (`TZID=UTC`). This allows greater interop with third-party libraries like Telerik's RadSchedule. [#259](https://github.com/rianjs/ical.net/issues/259) `PeriodList` now implements `IList<Period>`.
* 3.0.2-net-core-alpha: Fix nuspec file to declare NodaTime as dependency
* 3.0.1-alpha: Initial publishing of .NET Core release

### v2
* 2.2.35: All calendar components now explicitly implement IEquatable<T>. [PR 262](https://github.com/rianjs/ical.net/pull/262)
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
