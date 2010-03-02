using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITZID : 
        ICalendarDataType        
    {
        bool GloballyUnique { get; set; }
        string ID { get; set; }
    }
}
