//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
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

    public override string? SerializeToString(object? obj)
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
        // NOSONAR: netstandard2.x does not support string.Create(CultureInfo.InvariantCulture, $"{...}");
        if (ts.Weeks != null)
            sb.Append(FormattableString.Invariant($"{sign * ts.Weeks}W")); // NOSONAR 
        if (ts.Days != null)
            sb.Append(FormattableString.Invariant($"{sign * ts.Days}D")); // NOSONAR

        if (ts.Hours != null || ts.Minutes != null || ts.Seconds != null)
        {
            sb.Append('T');

            if (ts.Hours != null)
                sb.Append(FormattableString.Invariant($"{sign * ts.Hours}H")); // NOSONAR
            if (ts.Minutes != null)
                sb.Append(FormattableString.Invariant($"{sign * ts.Minutes}M")); // NOSONAR
            if (ts.Seconds != null)
                sb.Append(FormattableString.Invariant($"{sign * ts.Seconds}S")); // NOSONAR
        }

        return sb.ToString();
    }

    internal static readonly Regex DurationMatch =
        new Regex(@"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexDefaults.Timeout);

    /// <summary>
    /// Deserializes a string into a <see cref="Duration"/> object.
    /// </summary>
    /// <param name="tr"></param>
    /// <returns>A <see cref="Duration"/> for a valid input pattern, or <see langword="null"/> otherwise.</returns>
    /// <exception cref="FormatException">Cannot create a <see cref="Duration"/> from the input pattern values.</exception>
    public override object? Deserialize(TextReader tr)
    {
        var value = tr.ReadToEnd();

        try
        {
            var match = DurationMatch.Match(value);

            if (!match.Success)
            {
                // This happens for
                // * EXDATE values, which never have a duration,
                // * RDATE values, where the optional duration is missing
                // * TRIGGER values that are invalid
                return null;
            }

            var sign = 1;
            int? weeks = null;
            int? days = null;
            int? hours = null;
            int? minutes = null;
            int? seconds = null;

            if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                sign = -1;

            int? GetGroupInt(string key)
                => match.Groups[key].Success ? Convert.ToInt32(match.Groups[key].Value, CultureInfo.InvariantCulture) : null;

            weeks = GetGroupInt("week");
            if (match.Groups["main"].Success)
            {
                days = GetGroupInt("day");
                if (match.Groups["time"].Success)
                {
                    hours = GetGroupInt("hour");
                    minutes = GetGroupInt("minute");
                    seconds = GetGroupInt("second");
                }
            }

            return new Duration(sign * weeks, sign * days, sign * hours, sign * minutes, sign * seconds);
        }
        catch (Exception ex)
        {
            throw new FormatException(ex.Message, ex);
        }
    }
}
