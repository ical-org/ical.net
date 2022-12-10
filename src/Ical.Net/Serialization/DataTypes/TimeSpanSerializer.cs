using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Ical.Net.Serialization.DataTypes
{
    public class TimeSpanSerializer : SerializerBase
    {
        public TimeSpanSerializer() { }

        public TimeSpanSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (TimeSpan);

        public override string? SerializeToString(object? obj) => obj is TimeSpan ts ? SerializeToString(ts) : null;

        /// <summary> Converts the <paramref name="timeSpan"/> to a Standard Period "P..." String </summary>
        public static string SerializeToString(TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return "P0D";
            }

            var sb = new StringBuilder();

            if (timeSpan < TimeSpan.Zero)
            {
                sb.Append("-");
            }

            sb.Append("P");
            if (timeSpan.Days > 7 && timeSpan.Days % 7 == 0 && timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
            {
                sb.Append(Math.Round(Math.Abs((double) timeSpan.Days) / 7) + "W");
            }
            else
            {
                if (timeSpan.Days != 0)
                {
                    sb.Append(Math.Abs(timeSpan.Days) + "D");
                }
                if (timeSpan.Hours != 0 || timeSpan.Minutes != 0 || timeSpan.Seconds != 0)
                {
                    sb.Append("T");
                    if (timeSpan.Hours != 0)
                    {
                        sb.Append(Math.Abs(timeSpan.Hours) + "H");
                    }
                    if (timeSpan.Minutes != 0)
                    {
                        sb.Append(Math.Abs(timeSpan.Minutes) + "M");
                    }
                    if (timeSpan.Seconds != 0)
                    {
                        sb.Append(Math.Abs(timeSpan.Seconds) + "S");
                    }
                }
            }

            return sb.ToString();
        }

        internal static readonly Regex TimespanMatch =
            new Regex(@"^(?<sign>\+|-)?P(((?<week>\d+)W)|(?<main>((?<day>\d+)D)?(?<time>T((?<hour>\d+)H)?((?<minute>\d+)M)?((?<second>\d+)S)?)?))$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            try
            {
                var match = TimespanMatch.Match(value);
                var days = 0;
                var hours = 0;
                var minutes = 0;
                var seconds = 0;

                if (match.Success)
                {
                    var mult = 1;
                    if (match.Groups["sign"].Success && match.Groups["sign"].Value == "-")
                    {
                        mult = -1;
                    }

                    if (match.Groups["week"].Success)
                    {
                        days = Convert.ToInt32(match.Groups["week"].Value) * 7;
                    }
                    else if (match.Groups["main"].Success)
                    {
                        if (match.Groups["day"].Success)
                        {
                            days = Convert.ToInt32(match.Groups["day"].Value);
                        }
                        if (match.Groups["time"].Success)
                        {
                            if (match.Groups["hour"].Success)
                            {
                                hours = Convert.ToInt32(match.Groups["hour"].Value);
                            }
                            if (match.Groups["minute"].Success)
                            {
                                minutes = Convert.ToInt32(match.Groups["minute"].Value);
                            }
                            if (match.Groups["second"].Success)
                            {
                                seconds = Convert.ToInt32(match.Groups["second"].Value);
                            }
                        }
                    }

                    return new TimeSpan(days * mult, hours * mult, minutes * mult, seconds * mult);
                }
            }
            catch {}

            return value;
        }
    }
}