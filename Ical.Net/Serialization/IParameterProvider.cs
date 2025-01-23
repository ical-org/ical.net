//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;

namespace Ical.Net.Serialization;
internal interface IParameterProvider
{
    IReadOnlyList<CalendarParameter> GetParameters(object value);
}
