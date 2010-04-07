using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IKeyedObject<string>
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        string Value { get; set; }
        string[] Values { get; set; }
    }
}
