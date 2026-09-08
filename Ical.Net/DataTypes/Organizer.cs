//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Ical.Net.DataTypes;

/// <summary>
/// A class that represents the organizer of an event/todo/journal.
/// </summary>
[DebuggerDisplay("{Value}")]
public class Organizer : EncodableDataType
{
    public virtual Uri? SentBy
    {
        get
        {
            var sentBy = Parameters.Get("SENT-BY");
            if (!string.IsNullOrWhiteSpace(sentBy))
            {
                return new Uri(sentBy);
            }
            return null;
        }
        set
        {
            if (value != null)
            {
                Parameters.Set("SENT-BY", value.OriginalString);
            }
            else
            {
                Parameters.Remove("SENT-BY");
            }
        }
    }

    public virtual string? CommonName
    {
        get => Parameters.Get("CN");
        set => Parameters.Set("CN", value);
    }

    public virtual Uri? DirectoryEntry
    {
        get
        {
            var dir = Parameters.Get("DIR");
            if (!string.IsNullOrWhiteSpace(dir))
            {
                return new Uri(dir);
            }

            return null;
        }
        set
        {
            if (value != null)
            {
                Parameters.Set("DIR", value.OriginalString);
            }
            else
            {
                Parameters.Remove("DIR");
            }
        }
    }

    public virtual Uri? Value { get; set; }

    public Organizer() { }

    [Obsolete("Use TryParse instead.")]
    public Organizer(string? value) : this()
    {
        if (TryParse(value, out var other))
        {
            Value = other.Value;
        }
    }

    /// <inheritdoc/>
    public sealed override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is Organizer o)
        {
            Value = o.Value;
        }
    }

    #region Text Parsing

    private const string OrganizerScheme = "mailto:";

    public static bool TryParse(
        string? value,
#if NET
        [NotNullWhen(true)]
#endif
        out Organizer? organizer)
    {
        if (string.IsNullOrEmpty(value))
        {
            organizer = default;
            return false;
        }

        // Prepend "mailto:" if necessary
        if (!value.StartsWith(OrganizerScheme, StringComparison.OrdinalIgnoreCase))
        {
            value = "mailto:" + value;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            organizer = new Organizer
            {
                Value = uri
            };
            return true;
        }

        organizer = default;
        return false;
    }

    #endregion
}
