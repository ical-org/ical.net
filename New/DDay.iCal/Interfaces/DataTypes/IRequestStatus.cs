using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRequestStatus :
        ICalendarDataType
    {
        IText StatusDesc { get; set; }
        IText ExtData { get; set; }
        IStatusCode StatusCode { get; set; }
        string Language { get; set; }
    }
}
