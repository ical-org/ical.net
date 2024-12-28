﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class DurationSerializer : SerializerBase
{
    public DurationSerializer() { }

    public DurationSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Duration);

    public override string SerializeToString(object obj)
        => (obj is not Duration duration) ? null : SerializeToString(duration);

    private static string SerializeToString(Duration ts)
    {
        if (ts.IsEmpty)
            return "P0D";

        var sb = new StringBuilder();

        var sign = ts.Sign;
        if (sign < 0)
            sb.Append('-');

        sb.Append('P');
        if (ts.Weeks != null)
            sb.Append($"{sign * ts.Weeks}W");
        if (ts.Days != null)
            sb.Append($"{sign * ts.Days}D");

        if (ts.Hours != null || ts.Minutes != null || ts.Seconds != null)
        {
            sb.Append('T');
            if (ts.Hours != null)
                sb.Append($"{sign * ts.Hours}H");
            if (ts.Minutes != null)
                sb.Append($"{sign * ts.Minutes}M");
            if (ts.Seconds != null)
                sb.Append($"{sign * ts.Seconds}S");
        }

        return sb.ToString();
    }

    internal static readonly Regex TimespanMatch =
        new Regex(@"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexDefaults.Timeout);

    public override object Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        try
        {
            var match = TimespanMatch.Match(value);

            if (!match.Success)
                return value;

            var sign = 1;
            int? weeks = null;
            int? days = null;
            int? hours = null;
            int? minutes = null;
            int? seconds = null;

            if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                sign = -1;

            int? GetGroupInt(string key)
                => match.Groups[key].Success ? Convert.ToInt32(match.Groups[key].Value) : null;

            weeks = GetGroupInt("week");
            if (match.Groups["main"].Success)
            {
                days = GetGroupInt("day");
                if (match.Groups["time"].Success)
                {
                    hours  = GetGroupInt("hour");
                    minutes = GetGroupInt("minute");
                    seconds = GetGroupInt("second");
                }
            }

            return new Duration(sign * weeks, sign * days, sign * hours, sign * minutes, sign * seconds);
        }
        catch
        {
            throw new FormatException();
        }
    }
}
