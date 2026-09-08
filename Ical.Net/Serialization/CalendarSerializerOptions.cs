//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;

namespace Ical.Net.Serialization;

public sealed class CalendarSerializerOptions
{
    private Dictionary<string, CalendarPropertyConverter>? _converters;

    public Dictionary<string, CalendarPropertyConverter> Converters => _converters ??= [];

    /// <summary>
    /// Deserializing with strict parsing enabled will throw
    /// exceptions when invalid values or formatting is encountered.
    ///
    /// When disabled, invalid properties will be ignored.
    ///
    /// <para/>
    /// Default value is <see cref="true"/>.
    /// </summary>
    public bool StrictParsing { get; set; }

    /// <summary>
    /// When true, component properties are serialized alphabetically by name.
    /// </summary>
    public bool OrderComponentProperties { get; set; } = true;

    public CalendarPropertyConverter GetConverter(string name)
    {
        if (_converters is { } converterDictionary
            && converterDictionary.TryGetValue(name, out var converter))
        {
            return converter;
        }

        return ConverterMap.GetConverter(name);
    }
}
