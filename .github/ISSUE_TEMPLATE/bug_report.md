---
name: Bug report
about: Create a report to help us improve
title: ''
labels: bug
assignees: ''

---

**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:

1. iCalendar Data
If possible, provide the iCalendar data that reproduces the issue:
```
BEGIN:VCALENDAR
...
END:VCALENDAR
```

2. Include a unit test that shows actual and expected behavior:
```cs
var calendar = Calendar.Load("...");```
---
Assert.That(...)
```

**Expected behavior**
Add a clear and concise description for the reason of the expected behavior.

**Environment (please complete the following information):**
- OS: [e.g. Windows 10, macOS 11.2]
- .NET version: [e.g. .NET 6, .NET Framework 4.8]
- ical.net version: [e.g. 4.1.0]

**Additional context**
Add any other context about the problem here, such as relevant code snippets, logs, or error messages.
