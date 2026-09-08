//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

namespace Ical.Net.Serialization.Converters;

internal class StringConverter : CalendarPropertyConverter<string>
{
    public override string? Read(CalendarReader reader, IParameterCollection parameters) => reader.GetTextValue();

    public override void Write(CalendarWriter writer, string value) => writer.WriteValue(value);
}
