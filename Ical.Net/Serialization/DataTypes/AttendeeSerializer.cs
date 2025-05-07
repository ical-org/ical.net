//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class AttendeeSerializer : StringSerializer
{
    public AttendeeSerializer() { }

    public AttendeeSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Attendee);

    public override string? SerializeToString(object? obj)
    {
        var a = obj as Attendee;
        return a?.Value == null
            ? null
            : Encode(a, a.Value.OriginalString);
    }

    public Attendee? Deserialize(string attendee)
    {
        try
        {
            if (CreateAndAssociate() is not Attendee a) return null;

            // Note: Returned value can't be null here when args are not null.
            var uriString = Unescape(Decode(a, attendee))!;

            // Prepend "mailto:" if necessary
            if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                uriString = "mailto:" + uriString;
            }

            a.Value = new Uri(uriString);

            return a;
        }
        catch
        {
            // Return null instead of throwing an exception
        }

        return null;
    }

    public override object? Deserialize(TextReader? tr) => tr != null ? Deserialize(tr.ReadToEnd()) : null;
}
