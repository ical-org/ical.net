using Ical.Net.DataTypes;

namespace Ical.Net.NaturalLanguage;

public class ToText {
    private readonly List<int>? _byMonthDay;
    private readonly ByWeekDayRec? _byWeekDay;
    private readonly Func<DateTime, string> _dateFormatter;
    private readonly Culture _culture;
    private readonly RecurrencePattern _recurrencePattern;
    private readonly List<string> _text = new();

    public ToText(RecurrencePattern recurrencePattern, Func<DateTime, string>? dateFormatter = null,
        Culture? culture = null) {
        culture ??= Culture.English;
        _culture = culture;

        _recurrencePattern = recurrencePattern;
        dateFormatter ??= DefaultDateFormatter;
        _dateFormatter = dateFormatter;

        var weekdays = culture.WeekDays;
        var weekend = culture.DayNames.Keys.Except(weekdays).ToList();

        bool Contains(DayOfWeek d) => _recurrencePattern.ByDay.Any(z => z.DayOfWeek == d);
        bool NContains(DayOfWeek d) => _recurrencePattern.ByDay.All(z => z.DayOfWeek != d);

        if (_recurrencePattern.ByDay.Count > 0) {
            var allWeeks = _recurrencePattern.ByDay.Where(r => r.Offset == 0).ToList();
            var someWeeks = _recurrencePattern.ByDay.Where(r => r.Offset != 0).ToList();
            var isWeekDays = weekdays.All(Contains) && weekend.All(NContains);
            var isEveryDay = weekdays.All(Contains) && weekend.All(Contains);
            _byWeekDay = new ByWeekDayRec(allWeeks, someWeeks, isWeekDays, isEveryDay);
        }

        if (_recurrencePattern.ByMonthDay.Count > 0) {
            var positive = _recurrencePattern.ByMonthDay.Where(r => r > 0).Order().ToList();
            var negative = _recurrencePattern.ByMonthDay.Where(r => r < 0).Order().ToList();
            _byMonthDay = positive.Concat(negative).ToList();
        }
    }

    public static string ToFriendlyText(RecurrencePattern rp, Culture? culture = null) => new ToText(rp, culture: culture).ToFriendlyText();

    private string ToFriendlyText() {
        _text.Clear();
        _text.Add("Every");

        switch (_recurrencePattern.Frequency) {
            case FrequencyType.Minutely:
                Minutely();
                break;
            case FrequencyType.Hourly:
                Hourly();
                break;
            case FrequencyType.Daily:
                Daily();
                break;
            case FrequencyType.Weekly:
                Weekly();
                break;
            case FrequencyType.Monthly:
                Monthly();
                break;
            case FrequencyType.Yearly:
                Yearly();
                break;
            case FrequencyType.Secondly:
            case FrequencyType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_recurrencePattern.Until != DateTime.MinValue) {
            _text.Add("until");
            var until = _recurrencePattern.Until;
            _text.Add(_dateFormatter(until));
        }
        else if (_recurrencePattern.Count > 0) {
            _text.Add("for");
            _text.Add(_recurrencePattern.Count.ToString());
            _text.Add(Plural(_recurrencePattern.Count) ? "times" : "time");
        }

        if (!IsFullyConvertible()) {
            _text.Add("(~ approximate)");
        }

        return string.Join(' ', _text);
    }

    private static string List<T>(IEnumerable<T> arr, Func<T, string> getText, string? finalDelim = null,
        string delim = ",") {
        static string DelimJoin(IReadOnlyList<string> array, string delimiter, string finalDelimiter) {
            var list = "";
            for (var i = 0; i < array.Count; i++) {
                if (i != 0) {
                    if (i == array.Count - 1) {
                        list += $" {finalDelimiter} ";
                    }
                    else {
                        list += $"{delimiter} ";
                    }
                }

                list += array[i];
            }

            return list;
        }

        var strarr = arr.Select(getText).DefaultIfEmpty("").ToList();
        if (finalDelim is not null) {
            return DelimJoin(strarr, delim, finalDelim);
        }

        return strarr.Aggregate((x, y) => $"{x}{delim} {y}");
    }

    private static string Nth(int n) {
        if (n == -1) {
            return "last";
        }

        var npos = Math.Abs(n);
        var nth = npos switch {
            1 => npos + "st",
            2 => npos + "nd",
            3 => npos + "rd",
            21 => npos + "st",
            22 => npos + "nd",
            23 => npos + "rd",
            31 => npos + "st",
            _ => npos + "th",
        };

        return n < 0 ? nth + " " + "last" : nth;
    }

    private static bool Plural(int n) => n % 100 != 1;

    private void ByHour() {
        _text.Add("at");
        _text.Add(List(_recurrencePattern.ByHour, x => x.ToString(), "and"));
    }

    private void ByMonth() {
        _text.Add(List(_recurrencePattern.ByMonth, MonthText, "and"));
    }

    private void ByMonthDay() {
        if (_byWeekDay is { AllWeeks.Count: > 0 }) {
            _text.Add("on");
            _text.Add(List(_byWeekDay.AllWeeks, WeekDayText, "or"));
            _text.Add("the");
            _text.Add(List(_byMonthDay!, Nth, "or"));
        }
        else {
            _text.Add("on the");
            _text.Add(List(_byMonthDay!, Nth, "and"));
        }
    }

    private void ByWeekDay() {
        if (_byWeekDay is { AllWeeks.Count: > 0, IsWeekDays: false }) {
            _text.Add("on");
            _text.Add(List(_byWeekDay.AllWeeks, WeekDayText));
        }

        if (_byWeekDay is { SomeWeeks.Count: > 0 }) {
            if (_byWeekDay is { AllWeeks.Count: > 0 }) {
                _text.Add("and");
            }

            _text.Add("on");
            _text.Add(List(_byWeekDay.SomeWeeks, WeekDayText, "and"));
        }
    }

    private void Daily() {
        if (_recurrencePattern.Interval != 1) {
            _text.Add(_recurrencePattern.Interval.ToString());
        }

        if (_byWeekDay is { IsWeekDays: true }) {
            _text.Add(Plural(_recurrencePattern.Interval) ? "weekdays" : "weekday");
        }
        else {
            _text.Add(Plural(_recurrencePattern.Interval) ? "days" : "day");
        }

        if (_recurrencePattern.ByMonth.Count > 0) {
            _text.Add("in");
            ByMonth();
        }

        if (_byMonthDay is not null) {
            ByMonthDay();
        }
        else if (_byWeekDay is not null) {
            ByWeekDay();
        }
        else if (_recurrencePattern.ByHour.Count > 0) {
            ByHour();
        }
    }

    private string DefaultDateFormatter(DateTime d) {
        var utc = d.ToUniversalTime();
        var year = utc.Year;
        var month = _culture.MonthNames[utc.Month];
        var day = utc.Day;

        return $"{month} ${day}, ${year}";
    }

    private void Hourly() {
        if (_recurrencePattern.Interval != 1) {
            _text.Add(_recurrencePattern.Interval.ToString());
        }

        _text.Add(Plural(_recurrencePattern.Interval) ? "hours" : "hour");
    }

    private bool IsFullyConvertible() {
        if (_recurrencePattern.Frequency switch {
                FrequencyType.None => true,
                FrequencyType.Secondly => true,
                _ => false,
            }) {
            return false;
        }

        if (_recurrencePattern.Until != DateTime.MinValue && _recurrencePattern.Count > 0) {
            return false;
        }

        return true;
    }

    private void Minutely() {
        if (_recurrencePattern.Interval != 1) {
            _text.Add(_recurrencePattern.Interval.ToString());
        }

        _text.Add(Plural(_recurrencePattern.Interval) ? "minutes" : "minute");
    }

    private void Monthly() {
        if (_recurrencePattern.ByMonth.Count > 0) {
            if (_recurrencePattern.Interval != 1) {
                _text.Add(_recurrencePattern.Interval.ToString());
                _text.Add("months");
                if (Plural(_recurrencePattern.Interval)) {
                    _text.Add("in");
                }
            }

            ByMonth();
        }
        else {
            if (_recurrencePattern.Interval != 1) {
                _text.Add(_recurrencePattern.Interval.ToString());
            }

            _text.Add(Plural(_recurrencePattern.Interval) ? "months" : "month");
        }

        if (_byMonthDay is not null) {
            ByMonthDay();
        }
        else if (_byWeekDay is { IsWeekDays: true }) {
            _text.Add("on");
            _text.Add("weekdays");
        }
        else {
            ByWeekDay();
        }


        if (_recurrencePattern.ByHour.Count > 0) {
            ByHour();
        }
    }

    private string MonthText(int m) {
        return _culture.MonthNames[m - 1];
    }

    private string WeekDayText(WeekDay r) {
        var dayName = _culture.DayNames[r.DayOfWeek];

        return r.Offset != int.MinValue
            ? $"the {Nth(r.Offset)} {dayName}"
            : dayName;
    }

    private void Weekly() {
        if (_recurrencePattern.Interval != 1) {
            _text.Add(_recurrencePattern.Interval.ToString());
            _text.Add(Plural(_recurrencePattern.Interval) ? "weeks" : "week");
        }

        if (_byWeekDay is { IsWeekDays: true }) {
            if (_recurrencePattern.Interval == 1) {
                _text.Add(Plural(_recurrencePattern.Interval) ? "weekdays" : "weekday");
            }
            else {
                _text.Add("on");
                _text.Add("weekdays");
            }
        }
        else if (_byWeekDay is { IsEveryDay: true }) {
            _text.Add(Plural(_recurrencePattern.Interval) ? "days" : "day");
        }
        else {
            if (_recurrencePattern.Interval == 1) {
                _text.Add("week");
            }

            if (_recurrencePattern.ByMonth.Count > 0) {
                _text.Add("in");
                ByMonth();
            }

            if (_byMonthDay is not null) {
                ByMonthDay();
            }
            else if (_byWeekDay is not null) {
                ByWeekDay();
            }
        }
    }

    private void Yearly() {
        if (_recurrencePattern.ByMonth.Count > 0) {
            if (_recurrencePattern.Interval != 1) {
                _text.Add(_recurrencePattern.Interval.ToString());
                _text.Add("years");
            }

            ByMonth();
        }
        else {
            if (_recurrencePattern.Interval != 1) {
                _text.Add(_recurrencePattern.Interval.ToString());
            }

            _text.Add(Plural(_recurrencePattern.Interval) ? "years" : "year");
        }

        if (_byMonthDay is not null) {
            ByMonthDay();
        }
        else if (_byWeekDay is not null) {
            ByWeekDay();
        }

        if (_recurrencePattern.ByYearDay.Count > 0) {
            _text.Add("on the");
            _text.Add(List(_recurrencePattern.ByYearDay, Nth, "and"));
            _text.Add("day");
        }

        if (_recurrencePattern.ByWeekNo.Count > 0) {
            _text.Add("in");
            _text.Add(Plural(_recurrencePattern.ByWeekNo.Count) ? "weeks" : "week");
            _text.Add(List(_recurrencePattern.ByWeekNo, x => x.ToString(), "and"));
        }

        if (_recurrencePattern.ByHour.Count > 0) {
            ByHour();
        }
    }

    private record ByWeekDayRec(List<WeekDay> AllWeeks, List<WeekDay> SomeWeeks, bool IsWeekDays, bool IsEveryDay);
}
