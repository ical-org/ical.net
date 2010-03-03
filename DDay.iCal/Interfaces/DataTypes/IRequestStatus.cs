using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRequestStatus :
        ICalendarDataType
    {
        Text StatusDesc { get; set; }
        Text ExtData { get; set; }
        StatusCode StatusCode { get; set; }
        string Language { get; set; }
    }
}
