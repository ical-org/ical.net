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
            ["SKIP"] = new(@"^[ \r\n\t]+|^\.$"),
            ["number"] = new(@"^[1-9][0-9]*"),
            ["numberAsText"] = new(@"^(one|two|three)", RegexOptions.IgnoreCase),
            ["every"] = new(@"^Every", RegexOptions.IgnoreCase),
            ["day(s)"] = new(@"^days?", RegexOptions.IgnoreCase),
            ["weekday(s)"] = new(@"^weekdays?", RegexOptions.IgnoreCase),
            ["week(s)"] = new(@"^weeks?", RegexOptions.IgnoreCase),
            ["hour(s)"] = new(@"^hours?", RegexOptions.IgnoreCase),
            ["minute(s)"] = new(@"^minutes?", RegexOptions.IgnoreCase),
            ["month(s)"] = new(@"^months?", RegexOptions.IgnoreCase),
            ["year(s)"] = new(@"^years?", RegexOptions.IgnoreCase),
            ["on"] = new(@"^(on|in)", RegexOptions.IgnoreCase),
            ["at"] = new(@"^(at)", RegexOptions.IgnoreCase),
            ["the"] = new(@"^the", RegexOptions.IgnoreCase),
            ["first"] = new(@"^first", RegexOptions.IgnoreCase),
            ["second"] = new(@"^second", RegexOptions.IgnoreCase),
            ["third"] = new(@"^third", RegexOptions.IgnoreCase),
            ["nth"] = new(@"^([1-9][0-9]*)(\.|th|nd|rd|st)", RegexOptions.IgnoreCase),
            ["last"] = new(@"^last", RegexOptions.IgnoreCase),
            ["for"] = new(@"^for", RegexOptions.IgnoreCase),
            ["time(s)"] = new(@"^times?", RegexOptions.IgnoreCase),
            ["until"] = new(@"^(un)?til", RegexOptions.IgnoreCase),
            ["monday"] = new(@"^mo(n(day)?)?", RegexOptions.IgnoreCase),
            ["tuesday"] = new(@"^tu(e(s(day)?)?)?", RegexOptions.IgnoreCase),
            ["wednesday"] = new(@"^we(d(n(esday)?)?)?", RegexOptions.IgnoreCase),
            ["thursday"] = new(@"^th(u(r(sday)?)?)?", RegexOptions.IgnoreCase),
            ["friday"] = new(@"^fr(i(day)?)?", RegexOptions.IgnoreCase),
            ["saturday"] = new(@"^sa(t(urday)?)?", RegexOptions.IgnoreCase),
            ["sunday"] = new(@"^su(n(day)?)?", RegexOptions.IgnoreCase),
            ["january"] = new(@"^jan(uary)?", RegexOptions.IgnoreCase),
            ["february"] = new(@"^feb(ruary)?", RegexOptions.IgnoreCase),
            ["march"] = new(@"^mar(ch)?", RegexOptions.IgnoreCase),
            ["april"] = new(@"^apr(il)?", RegexOptions.IgnoreCase),
            ["may"] = new(@"^may", RegexOptions.IgnoreCase),
            ["june"] = new(@"^june?", RegexOptions.IgnoreCase),
            ["july"] = new(@"^july?", RegexOptions.IgnoreCase),
            ["august"] = new(@"^aug(ust)?", RegexOptions.IgnoreCase),
            ["september"] = new(@"^sep(t(ember)?)?", RegexOptions.IgnoreCase),
            ["october"] = new(@"^oct(ober)?", RegexOptions.IgnoreCase),
            ["november"] = new(@"^nov(ember)?", RegexOptions.IgnoreCase),
            ["december"] = new(@"^dec(ember)?", RegexOptions.IgnoreCase),
            ["comma"] = new(@"^(,\s*|(and|or)\s*)+", RegexOptions.IgnoreCase),
        },
    };

    public Dictionary<DayOfWeek, string> DayNames { get; init; } = null!;
    public List<DayOfWeek> WeekDays { get; init; } = null!;
    public List<string> MonthNames { get; init; } = null!;
    public Dictionary<string, Regex> Tokens { get; init; } = null!;
}
