//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class OrganizerSerializer : StringSerializer
{
    public OrganizerSerializer() { }

    public OrganizerSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Organizer);

    public override string? SerializeToString(object? obj)
    {
        try
        {
            var o = obj as Organizer;
            return o?.Value == null
                ? null
                : Encode(o, Escape(o.Value.OriginalString));
        }
        catch
        {
            return null;
        }
    }

    public override object? Deserialize(TextReader? tr)
    {
        if (tr == null) return null;

        var value = tr.ReadToEnd();

        Organizer? organizer = null;
        try
        {
            organizer = CreateAndAssociate() as Organizer;
            if (organizer != null)
            {
                var uriString = Unescape(Decode(organizer, value));
                if (uriString == null)
                {
                    return null;
                }

                // Prepend "mailto:" if necessary
                if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                {
                    uriString = "mailto:" + uriString;
                }

                organizer.Value = new Uri(uriString);
            }
        }
        catch
        {
            // Return null instead of throwing an exception
        }

        return organizer;
    }
}
