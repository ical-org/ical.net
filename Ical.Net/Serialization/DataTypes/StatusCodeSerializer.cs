//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class StatusCodeSerializer : StringSerializer
{
    public StatusCodeSerializer() { }

    public StatusCodeSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(StatusCode);

    public override string? SerializeToString(object? obj)
    {
        if (obj is not StatusCode sc)
        {
            return null;
        }

        var vals = new string[sc.Parts.Length];
        for (var i = 0; i < sc.Parts.Length; i++)
        {
            vals[i] = sc.Parts[i].ToString(CultureInfo.InvariantCulture);
        }
        return Encode(sc, Escape(string.Join(".", vals)));
    }

    internal static readonly Regex StatusCode = new Regex(@"\d(\.\d+)*", RegexOptions.Compiled | RegexOptions.CultureInvariant, RegexDefaults.Timeout);

    public override object? Deserialize(TextReader? tr)
    {
        if (tr == null) return null;

        var value = tr.ReadToEnd();

        if (CreateAndAssociate() is not StatusCode sc)
        {
            return null;
        }

        // Decode the value as needed
        value = Decode(sc, value);
        if (value == null) return null;

        var match = StatusCode.Match(value);
        if (!match.Success)
        {
            return null;
        }

        var parts = match.Value.Split('.');
        var intParts = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var num))
            {
                return false;
            }
            intParts[i] = num;
        }

        return new StatusCode(intParts);
    }
}
