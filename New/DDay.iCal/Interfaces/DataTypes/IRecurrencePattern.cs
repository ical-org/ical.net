using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurrencePattern
    {
        IList<iCalDateTime> StaticOccurrences { get; set; }
        iCalDateTime Until { get; set; }

        IList<iCalDateTime> Evaluate(iCalDateTime StartDate, iCalDateTime FromDate, iCalDateTime ToDate);
        bool CheckValidDate(iCalDateTime dt);
        bool IsValidDate(iCalDateTime dt);
    }    
}
