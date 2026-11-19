//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;

namespace Ical.Net.Serialization;

[Obsolete("This will be removed with no public replacement.")]
internal interface IParameterProvider
{
    IReadOnlyList<CalendarParameter> GetParameters(object? value);
}
