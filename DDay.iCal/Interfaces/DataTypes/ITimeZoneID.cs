using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZoneID
    {
        bool GloballyUnique { get; set; }
        string ID { get; set; }
    }
}
