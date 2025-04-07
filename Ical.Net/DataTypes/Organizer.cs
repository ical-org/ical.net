//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using Ical.Net.Serialization.DataTypes;

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

    public virtual string CommonName
    {
        get => Parameters.Get("CN");
        set => Parameters.Set("CN", value);
    }

    public virtual Uri? DirectoryEntry
    {
        get
        {
            string dir = Parameters.Get("DIR");
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

    public Organizer(string value) : this()
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var serializer = new OrganizerSerializer();
        if (serializer.Deserialize(new StringReader(value)) is ICopyable deserialized)
            CopyFrom(deserialized);
    }

    protected bool Equals(Organizer other) => Equals(Value, other.Value);

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
        return Equals((Organizer) obj);
    }

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <inheritdoc/>
    public sealed override void CopyFrom(ICopyable obj)
    {
        base.CopyFrom(obj);

        if (obj is Organizer o)
        {
            Value = o.Value;
        }
    }
}
