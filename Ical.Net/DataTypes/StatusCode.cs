//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

    internal static readonly Regex StatusCodeRegex = new Regex(@"\d(\.\d+)*", RegexOptions.Compiled | RegexOptions.CultureInvariant, RegexDefaults.Timeout);

    public static bool TryParse(
        string value,
#if NET
        [NotNullWhen(true)]
#endif
        out StatusCode? statusCode)
    {
        var match = StatusCodeRegex.Match(value);
        if (!match.Success)
        {
            statusCode = null;
            return false;
        }

        var parts = match.Value.Split('.');
        var intParts = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var num))
            {
                statusCode = null;
                return false;
            }
            intParts[i] = num;
        }

        statusCode = new StatusCode(intParts);
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
