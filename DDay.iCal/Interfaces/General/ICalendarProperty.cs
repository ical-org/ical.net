using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarProperty :        
        ICalendarParameterListContainer,
        ICalendarObject        
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        object Value { get; set; }
    }    
}
