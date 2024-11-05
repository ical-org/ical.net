//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using Ical.Net.Collections.Interfaces;
using Ical.Net.DataTypes;

namespace Ical.Net
{
    public interface ICalendarProperty : ICalendarParameterCollectionContainer, ICalendarObject, IValueObject<object>
    {
        object Value { get; set; }
    }
}
