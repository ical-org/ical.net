//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Serialization.Converters;

/// <summary>
/// Same as <see cref="StringConverter"/> but allows any type when
/// writing and uses <see cref="object.ToString"/> to write values.
/// </summary>
internal class ObjectToStringConverter : CalendarPropertyConverter<object>
{
    public override object? Read(CalendarReader reader, IParameterCollection parameters)
        => reader.GetTextValue();

    public override void Write(CalendarWriter writer, object value)
        => writer.WriteValue(value?.ToString() ?? string.Empty);
}
