using System.Text.RegularExpressions;
using Ical.Net.DataTypes;
using OneOf;

namespace Ical.Net.NaturalLanguage;

public class Parser {
    private readonly Dictionary<string, Regex> _rules;
    private string _text = null!;
    private string? _symbol;
    private MatchCollection? _value;
    private bool _done = true;

    private Parser(Dictionary<string, Regex> rules) {
        _rules = rules;
    }

    private bool Start(string text) {
        _text = text;
        _done = false;
        return NextSymbol();
    }

    private bool IsDone() {
        return _done && _symbol is null;
    }

    private bool NextSymbol() {
        MatchCollection? best;
        string? bestSymbol = null;

        _symbol = null;
        _value = null;
        do {
            if (_done) return false;

            best = null;
            foreach (var name in _rules.Keys) {
                var rule = _rules[name];
                var match = rule.Matches(_text);
                if (match.Count > 0 && (best == null || match[0].Length > best[0].Length)) {
                    best = match;
                    bestSymbol = name;
                }
            }

            if (best != null) {
                _text = _text[best[0].Length..];

                if (_text == "") _done = true;
            }

            if (best == null) {
                _done = true;
                _symbol = null;
                _value = null;
                return false;
            }
        } while (bestSymbol == "SKIP");

        _symbol = bestSymbol;
        _value = best;
        return true;
    }

    private OneOf<bool, MatchCollection> Accept(string name) {
        if (_symbol != name) {
            return false;
        }

        if (_value is { Count: > 0 }) {
            var v = _value;
            NextSymbol();
            return v;
        }

        NextSymbol();
        return true;
    }

    private OneOf<bool, MatchCollection> AcceptNumber() {
        return Accept("number");
    }

    private void Expect(string name) {
        if (!Accept(name).T()) {
            throw new Exception("expected " + name + " but found " + _symbol);
        }
    }

    private static int ParseInt(string s, int @base) {
        return int.Parse(s);
    }

    public static RecurrencePattern? ParseText(string text, Culture? culture = null) {
        culture ??= Culture.English;
        RecurrencePattern options = new();
        var ttr = new Parser(culture.Tokens);

        var weekdays = culture.WeekDays;
        var weekend = culture.DayNames.Keys.Except(weekdays).ToList();
        var weekdayAbbrevs = weekdays.Select(r => r.ToString()[..2].ToUpper());

        if (!ttr.Start(text)) return null;

        S();
        //options.PostProcess();
        return options;

        void S() {
            ttr.Expect("every");
            var n = ttr.AcceptNumber();
            if (n.T()) options.Interval = ParseInt(n.AsT1[0].Value, 10);
            if (ttr.IsDone()) throw new Exception("Unexpected end");

            switch (ttr._symbol) {
                case "day(s)":
                    options.Frequency = FrequencyType.Daily;
                    if (ttr.NextSymbol()) {
                        At();
                        F();
                    }
                    break;

                // FIXME Note: every 2 weekdays != every two weeks on weekdays.
                // DAILY on weekdays is not a valid rule
                case "weekday(s)":
                    options.Frequency = FrequencyType.Weekly;
                    options.ByDay.AddRange(weekdayAbbrevs.Select(z=>MkWeekDay(z)));
                    ttr.NextSymbol();
                    F();
                    break;

                case "week(s)":
                    options.Frequency = FrequencyType.Weekly;
                    if (ttr.NextSymbol()) {
                        On();
                        F();
                    }

                    break;

                case "hour(s)":
                    options.Frequency = FrequencyType.Hourly;
                    if (ttr.NextSymbol()) {
                        On();
                        F();
                    }

                    break;

                case "minute(s)":
                    options.Frequency = FrequencyType.Minutely;
                    if (ttr.NextSymbol()) {
                        On();
                        F();
                    }

                    break;

                case "month(s)":
                    options.Frequency = FrequencyType.Monthly;
                    if (ttr.NextSymbol()) {
                        On();
                        F();
                    }

                    break;

                case "year(s)":
                    options.Frequency = FrequencyType.Yearly;
                    if (ttr.NextSymbol()) {
                        On();
                        F();
                    }

                    break;

                case "monday":
                case "tuesday":
                case "wednesday":
                case "thursday":
                case "friday":
                case "saturday":
                case "sunday":
                    options.Frequency = FrequencyType.Weekly;
                    var key = ttr._symbol[..2].ToUpper();
                    options.ByDay.Add(MkWeekDay(key));

                    if (!ttr.NextSymbol()) return;

                    // TODO check for duplicates
                    while (ttr.Accept("comma").T()) {
                        if (ttr.IsDone()) throw new Exception("Unexpected end");

                        var wkd = DecodeWkd();
                        if (wkd is null) {
                            throw new Exception(
                                "Unexpected symbol " + ttr._symbol + ", expected weekday"
                            );
                        }

                        options.ByDay.Add(MkWeekDay(wkd));
                        ttr.NextSymbol();
                    }

                    MdaYs();
                    F();
                    break;

                case "january":
                case "february":
                case "march":
                case "april":
                case "may":
                case "june":
                case "july":
                case "august":
                case "september":
                case "october":
                case "november":
                case "december":
                    options.Frequency = FrequencyType.Yearly;
                    options.ByMonth.Add(DecodeM()!.Value);

                    if (!ttr.NextSymbol()) return;

                    // TODO check for duplicates
                    while (ttr.Accept("comma").T()) {
                        if (ttr.IsDone()) throw new Exception("Unexpected end");

                        var m = DecodeM();
                        if (m is null) {
                            throw new Exception(
                                "Unexpected symbol " + ttr._symbol + ", expected month"
                            );
                        }

                        options.ByMonth.Add(m.Value);
                        ttr.NextSymbol();
                    }

                    On();
                    F();
                    break;

                default:
                    throw new Exception("Unknown symbol");
            }
        }

        void MdaYs() {
            ttr.Accept("on");
            ttr.Accept("the");

            var nth = DecodeNth();
            if (nth is null)
                return;

            options.ByMonthDay.Add(nth.Value);
            ttr.NextSymbol();

            while (ttr.Accept("comma").T()) {
                nth = DecodeNth();
                if (nth is null) {
                    throw new Exception(
                        "Unexpected symbol " + ttr._symbol + "; expected monthday"
                    );
                }

                options.ByMonthDay.Add(nth.Value);
                ttr.NextSymbol();
            }
        }

        int? DecodeNth() {
            switch (ttr._symbol) {
                case "last":
                    ttr.NextSymbol();
                    return -1;
                case "first":
                    ttr.NextSymbol();
                    return 1;
                case "second":
                    ttr.NextSymbol();
                    return ttr.Accept("last").T() ? -2 : 2;
                case "third":
                    ttr.NextSymbol();
                    return ttr.Accept("last").T() ? -3 : 3;
                case "nth":
                    var v = ParseInt(ttr._value![0].Groups[1].Value, 10);
                    if (v is < -366 or > 366) throw new Exception("Nth out of range: " + v);

                    ttr.NextSymbol();
                    return ttr.Accept("last").T() ? -v : v;

                default:
                    return null;
            }
        }

        string? DecodeWkd() {
            switch (ttr._symbol) {
                case "monday":
                case "tuesday":
                case "wednesday":
                case "thursday":
                case "friday":
                case "saturday":
                case "sunday":
                    return ttr._symbol[..2].ToUpper();
                default:
                    return null;
            }
        }

        int? DecodeM() {
            return ttr._symbol switch {
                "january" => 1,
                "february" => 2,
                "march" => 3,
                "april" => 4,
                "may" => 5,
                "june" => 6,
                "july" => 7,
                "august" => 8,
                "september" => 9,
                "october" => 10,
                "november" => 11,
                "december" => 12,
                _ => null,
            };
        }

        void On() {
            var on = ttr.Accept("on");
            var the = ttr.Accept("the");
            if (!(on.T() || the.T())) return;

            do {
                var nth = DecodeNth();
                var wkd = DecodeWkd();
                var m = DecodeM();

                // nth <weekday> | <weekday>
                if (nth is not null) {
                    // ttr.nextSymbol()

                    if (wkd is not null) {
                        ttr.NextSymbol();
                        options.ByDay.Add(MkWeekDay(wkd, nth));
                    }
                    else {
                        options.ByMonthDay.Add(nth.Value);
                        ttr.Accept("day(s)");
                    }
                    // <weekday>
                }
                else if (wkd is not null) {
                    ttr.NextSymbol();
                    options.ByDay.Add(MkWeekDay(wkd));
                }
                else if (ttr._symbol == "weekday(s)") {
                    ttr.NextSymbol();
                    options.ByDay.AddRange(new[] {
                        MkWeekDay("SU"),
                        MkWeekDay("MO"),
                        MkWeekDay("TU"),
                        MkWeekDay("WE"),
                        MkWeekDay("TH"),
                    });
                }
                else if (ttr._symbol == "week(s)") {
                    ttr.NextSymbol();
                    var n = ttr.AcceptNumber();
                    if (n.F()) {
                        throw new Exception(
                            "Unexpected symbol " + ttr._symbol + ", expected week number"
                        );
                    }

                    options.ByWeekNo.Add(ParseInt(n.AsT1[0].Value, 10));
                    while (ttr.Accept("comma").T()) {
                        n = ttr.AcceptNumber();
                        if (n.F()) {
                            throw new Exception(
                                "Unexpected symbol " + ttr._symbol + "; expected monthday"
                            );
                        }

                        options.ByWeekNo.Add(ParseInt(n.AsT1[0].Value, 10));
                    }
                }
                else if (m is not null) {
                    ttr.NextSymbol();
                    options.ByMonth.Add(m.Value);
                }
                else {
                    return;
                }
            } while (ttr.Accept("comma").T() || ttr.Accept("the").T() || ttr.Accept("on").T());
        }

        void At() {
            var at = ttr.Accept("at");
            if (at.F()) return;

            do {
                var n = ttr.AcceptNumber();
                if (n.F()) {
                    throw new Exception("Unexpected symbol " + ttr._symbol + ", expected hour");
                }

                options.ByHour.Add(ParseInt(n.AsT1[0].Value, 10));
                while (ttr.Accept("comma").T()) {
                    n = ttr.AcceptNumber();
                    if (n.F()) {
                        throw new Exception("Unexpected symbol " + ttr._symbol + "; expected hour");
                    }

                    options.ByHour.Add(ParseInt(n.AsT1[0].Value, 10));
                }
            } while (ttr.Accept("comma").T() || ttr.Accept("at").T());
        }

        void F() {
            if (ttr._symbol == "until") {
                var success = DateTime.TryParse(ttr._text, out var date);

                if (!success) throw new Exception("Cannot parse until date:" + ttr._text);
                options.Until = date;
            }
            else if (ttr.Accept("for").T()) {
                options.Count = ParseInt(ttr._value![0].Groups[0].Value, 10);
                ttr.Expect("number");
                // ttr.expect('times')
            }
        }
    }

    private static readonly Dictionary<string, DayOfWeek> WeekDayDic = new() {
        ["MO"] = DayOfWeek.Monday,
        ["TU"] = DayOfWeek.Tuesday,
        ["WE"] = DayOfWeek.Wednesday,
        ["TH"] = DayOfWeek.Thursday,
        ["FR"] = DayOfWeek.Friday,
        ["SA"] = DayOfWeek.Saturday,
        ["SU"] = DayOfWeek.Sunday,
    };

    private static WeekDay MkWeekDay(string wkd, int? num = null) => new(WeekDayDic[wkd], num ?? int.MinValue);
}

internal static class OneOfExtensions {
    public static bool T(this OneOf<bool, MatchCollection> o) => o.IsTruthy();
    public static bool F(this OneOf<bool, MatchCollection> o) => o.IsFalsy();

    private static bool IsTruthy(this OneOf<bool, MatchCollection> r) => r is { IsT0: true, AsT0: true } or { IsT1: true, AsT1: [_, ..], };
    private static bool IsFalsy(this OneOf<bool, MatchCollection> r) => r is { IsT0: true, AsT0: false } or { IsT1: true, AsT1: [] };
}
