//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Ical.Net.DataTypes;

/// <summary>
/// Represents a time offset from UTC (Coordinated Universal Time).
/// </summary>
public class UtcOffset : EncodableDataType
{
    public TimeSpan Offset { get; private set; }

    public bool Positive => Offset >= TimeSpan.Zero;

    public int Hours => Math.Abs(Offset.Hours);

    public int Minutes => Math.Abs(Offset.Minutes);

    public int Seconds => Math.Abs(Offset.Seconds);

    public UtcOffset() { }

    [Obsolete("Use TryParse instead.")]
    public UtcOffset(string value) : this()
    {
        if (TryParse(value, out var utcOffset))
        {
            Offset = utcOffset.Offset;
        }
        else
        {
            throw new FormatException($"{value} is not a valid UTC offset.");
        }
    }

    public UtcOffset(TimeSpan ts)
    {
        Offset = ts;
    }

    public static implicit operator UtcOffset(TimeSpan ts) => new UtcOffset(ts);

    public static explicit operator TimeSpan(UtcOffset o) => o.Offset;

    public virtual DateTime ToUtc(DateTime dt) => DateTime.SpecifyKind(dt.Add(-Offset), DateTimeKind.Utc);

    public virtual DateTime ToLocal(DateTime dt) => DateTime.SpecifyKind(dt.Add(Offset), DateTimeKind.Local);

    protected bool Equals(UtcOffset other) => Offset == other.Offset;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != GetType())
        {
            return false;
        }
        return Equals((UtcOffset) obj);
    }

    public override int GetHashCode() => Offset.GetHashCode();

    public override string ToString() => (Positive ? "+" : "-")
        + Hours.ToString("00", CultureInfo.InvariantCulture)
        + Minutes.ToString("00", CultureInfo.InvariantCulture)
        + (Seconds != 0 ? Seconds.ToString("00", CultureInfo.InvariantCulture) : string.Empty);

    private static readonly string[] _utcOffsetFormats = ["hhmmss", "hhmm", "hh"];

    public static bool TryParse(
        ReadOnlySpan<char> value,
#if NET
        [NotNullWhen(true)]
#endif
        out UtcOffset? utcOffset)
    {
        if (value.Length == 0)
        {
            utcOffset = default;
            return false;
        }

        var isNegative = value[0] == '-';

        // Remove sign if needed
        if (value[0] is '-' or '+')
        {
            value = value.Slice(1);
        }

        var parseResult = TimeSpan.TryParseExact(
#if NET
            value,
#else
            value.ToString(),
#endif
            _utcOffsetFormats, CultureInfo.InvariantCulture, out var ts);

        if (!parseResult)
        {
            utcOffset = default;
            return false;
        }

        utcOffset = new UtcOffset(isNegative ? -ts : ts);
        return true;
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is UtcOffset o)
        {
            Offset = o.Offset;
        }
    }
}
