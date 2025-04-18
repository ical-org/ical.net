//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System.Collections.Generic;

namespace Ical.Net.Serialization;
internal interface IParameterProvider
{
    IReadOnlyList<CalendarParameter> GetParameters(object value);
}
