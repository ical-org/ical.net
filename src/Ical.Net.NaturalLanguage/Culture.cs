using System.Text.RegularExpressions;

namespace Ical.Net.NaturalLanguage;

public class Culture {
    public static readonly Culture English = new() {
        DayNames = new() {
            [DayOfWeek.Sunday] = "Sunday",
            [DayOfWeek.Monday] = "Monday",
            [DayOfWeek.Tuesday] = "Tuesday",
            [DayOfWeek.Wednesday] = "Wednesday",
            [DayOfWeek.Thursday] = "Thursday",
            [DayOfWeek.Friday] = "Friday",
            [DayOfWeek.Saturday] = "Saturday",
        },
        WeekDays = new() {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
        },
        MonthNames = new() {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December",
        },
        Tokens = new() {
            ["SKIP"] = new(@"^[ \r\n\t]+|^\.$", RegexOptions.None, TimeSpan.FromMilliseconds(100)),
            ["number"] = new(@"^[1-9][0-9]*", RegexOptions.None, TimeSpan.FromMilliseconds(100)),
            ["numberAsText"] = new(@"^(one|two|three)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["every"] = new(@"^Every", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["day(s)"] = new(@"^days?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["weekday(s)"] = new(@"^weekdays?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["week(s)"] = new(@"^weeks?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["hour(s)"] = new(@"^hours?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["minute(s)"] = new(@"^minutes?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["month(s)"] = new(@"^months?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["year(s)"] = new(@"^years?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["on"] = new(@"^(on|in)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["at"] = new(@"^(at)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["the"] = new(@"^the", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["first"] = new(@"^first", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["second"] = new(@"^second", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["third"] = new(@"^third", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["nth"] = new(@"^([1-9][0-9]*)(\.|th|nd|rd|st)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["last"] = new(@"^last", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["for"] = new(@"^for", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["time(s)"] = new(@"^times?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["until"] = new(@"^(un)?til", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["monday"] = new(@"^mo(n(day)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["tuesday"] = new(@"^tu(e(s(day)?)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["wednesday"] = new(@"^we(d(n(esday)?)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["thursday"] = new(@"^th(u(r(sday)?)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["friday"] = new(@"^fr(i(day)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["saturday"] = new(@"^sa(t(urday)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["sunday"] = new(@"^su(n(day)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["january"] = new(@"^jan(uary)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["february"] = new(@"^feb(ruary)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["march"] = new(@"^mar(ch)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["april"] = new(@"^apr(il)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["may"] = new(@"^may", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["june"] = new(@"^june?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["july"] = new(@"^july?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["august"] = new(@"^aug(ust)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["september"] = new(@"^sep(t(ember)?)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["october"] = new(@"^oct(ober)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["november"] = new(@"^nov(ember)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["december"] = new(@"^dec(ember)?", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            ["comma"] = new(@"^(,\s*|(and|or)\s*)+", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
        },
    };

    public Dictionary<DayOfWeek, string> DayNames { get; init; } = null!;
    public List<DayOfWeek> WeekDays { get; init; } = null!;
    public List<string> MonthNames { get; init; } = null!;
    public Dictionary<string, Regex> Tokens { get; init; } = null!;
}
