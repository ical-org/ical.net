using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public interface IFreeBusyEntry :
        IPeriod
    {
        FreeBusyType Type { get; set; }
    }
}
