//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes;

/// <summary>
/// An iCalendar status code.
/// </summary>
public class StatusCode : EncodableDataType
{
    public int[] Parts { get; private set; }

    public int Primary
    {
        get
        {
            if (Parts.Length > 0)
            {
                return Parts[0];
            }
            return 0;
        }
    }

    public int Secondary => Parts.Length > 1
        ? Parts[1]
        : 0;

    public int Tertiary => Parts.Length > 2
        ? Parts[2]
        : 0;

    public StatusCode()
    {
        Parts = System.Array.Empty<int>();
    }

    public StatusCode(int[] parts)
    {
        Parts = parts;
    }

    [Obsolete("Use TryParse instead.")]
    public StatusCode(string value) : this()
    {
        if (TryParse(value, out var other))
        {
            CopyFrom(other);
        }
    }

    /// <inheritdoc/>
    public override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);
        if (obj is not StatusCode statusCode) return;

        Parts = new int[statusCode.Parts.Length];
        statusCode.Parts.CopyTo(Parts, 0);
    }

    public static bool TryParse(
        string value,
#if NET
        [NotNullWhen(true)]
#endif
        out StatusCode? statusCode)
    {
        statusCode = null;

        // Find the first digit, then any following ".digits" parts
        var start = 0;
        while (start < value.Length && !char.IsDigit(value[start]))
        {
            start++;
        }

        if (start == value.Length)
        {
            return false;
        }

        var parts = new List<int>();
        var end = start + 1;

        while (true)
        {
            if (!int.TryParse(value.Substring(start, end - start), NumberStyles.None, CultureInfo.InvariantCulture, out var part))
            {
                return false;
            }

            parts.Add(part);

            if (end + 1 >= value.Length || value[end] != '.' || !char.IsDigit(value[end + 1]))
            {
                break;
            }

            start = end + 1;
            end = start;
            while (end < value.Length && char.IsDigit(value[end]))
            {
                end++;
            }
        }

        statusCode = new StatusCode(parts.ToArray());
        return true;
    }

    public override string ToString()
    {
        var vals = new string[Parts.Length];
        for (var i = 0; i < Parts.Length; i++)
        {
            vals[i] = Parts[i].ToString(CultureInfo.InvariantCulture);
        }

        return string.Join(".", vals);
    }

    protected bool Equals(StatusCode other) => Parts.SequenceEqual(other.Parts);

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
        return Equals((StatusCode) obj);
    }

    public override int GetHashCode() => CollectionHelpers.GetHashCode(Parts);
}
