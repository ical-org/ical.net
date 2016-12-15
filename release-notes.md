## Release notes

A listing of what each [Nuget package](https://www.nuget.org/packages/Ical.Net) version represents.

### v2

* 2.2.25: Fix for Collection was modified exception, and better handling of recurrence ids when calling `Calendar.GetOccurrences` ([#188](https://github.com/rianjs/ical.net/issues/188)) ([#148](https://github.com/rianjs/ical.net/issues/148))
* 2.2.24: Performance enhancement for solidus-prefixed time zones ([#204](https://github.com/rianjs/ical.net/issues/204)).
* 2.2.23: Bugfix for culture for geographic location serialization. RFC-5545 requires coordinates to use decimal points, not commas, regardless of culture. Correct: `1.2345`. Incorrect: `1,2345`. ([#202](https://github.com/rianjs/ical.net/issues/202))
* 2.2.22: Bugfix for `Event` serialization that always changed the `Duration` to 0. Serialization shouldn't have side effects. ([#199](https://github.com/rianjs/ical.net/issues/199))

Changes weren't systematically tracked before 2.2.22.