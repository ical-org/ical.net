using System;
using System.IO;
using System.Text;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.DataTypes
{
    public class TimeSpanSerializer : SerializerBase
    {
        public TimeSpanSerializer() { }

        public TimeSpanSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (TimeSpan);

        public override string SerializeToString(object obj)
        {
            if (!(obj is TimeSpan))
            {
                return null;
            }

            var ts = (TimeSpan) obj;

            if (ts == TimeSpan.Zero)
            {
                return "P0D";
            }

            var sb = new StringBuilder();

            if (ts < TimeSpan.Zero)
            {
                sb.Append("-");
            }

            sb.Append("P");
            if (ts.Days > 7 && ts.Days % 7 == 0 && ts.Hours == 0 && ts.Minutes == 0 && ts.Seconds == 0)
            {
                sb.Append(Math.Round(Math.Abs((double) ts.Days) / 7) + "W");
            }
            else
            {
                if (ts.Days != 0)
                {
                    sb.Append(Math.Abs(ts.Days) + "D");
                }
                if (ts.Hours != 0 || ts.Minutes != 0 || ts.Seconds != 0)
                {
                    sb.Append("T");
                    if (ts.Hours != 0)
                    {
                        sb.Append(Math.Abs(ts.Hours) + "H");
                    }
                    if (ts.Minutes != 0)
                    {
                        sb.Append(Math.Abs(ts.Minutes) + "M");
                    }
                    if (ts.Seconds != 0)
                    {
                        sb.Append(Math.Abs(ts.Seconds) + "S");
                    }
                }
            }

            return sb.ToString();
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            try
            {
                var match = CompiledRegularExpressions.Timespan.Match(value);
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