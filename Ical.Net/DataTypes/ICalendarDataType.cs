//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net.DataTypes
{
    public interface ICalendarDataType : ICalendarParameterCollectionContainer, ICopyable, IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        Calendar Calendar { get; }

        string Language { get; set; }
    }
}
