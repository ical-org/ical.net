//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
namespace Ical.Net.DataTypes;

public interface IEncodableDataType
{
    string? Encoding { get; set; }
}
