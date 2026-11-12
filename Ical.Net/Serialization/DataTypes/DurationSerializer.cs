//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes;

public class DurationSerializer : SerializerBase
{
    public DurationSerializer() { }

    public DurationSerializer(SerializationContext ctx) : base(ctx) { }

    public override Type TargetType => typeof(Duration);

    public override string? SerializeToString(object? obj)
        => (obj is not Duration duration) ? null : duration.ToBasicIso();

    /// <summary>
    /// Deserializes a string into a <see cref="Duration"/> object.
    /// </summary>
    /// <param name="tr"></param>
    /// <returns>A <see cref="Duration"/> for a valid input pattern, or <see langword="null"/> otherwise.</returns>
    /// <exception cref="FormatException">Cannot create a <see cref="Duration"/> from the input pattern values.</exception>
    public override object? Deserialize(TextReader tr)
        => Duration.Parse(tr.ReadToEnd());
}
