//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Serialization.Converters;

/// <summary>
/// Writes a string value without escaping, for values that are
/// already in the serialized form of the property value type.
/// </summary>
internal class RawStringConverter : CalendarPropertyConverter<string>
{
    public override string? Read(CalendarReader reader, IParameterCollection parameters) => reader.GetTextValue();

    public override void Write(CalendarWriter writer, string value) => writer.WriteValueRaw(value);
}
