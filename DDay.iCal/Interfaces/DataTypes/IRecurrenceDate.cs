using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurrenceDate
    {
        IList<Period> Evaluate(iCalDateTime StartDate, iCalDateTime FromDate, iCalDateTime ToDate);
    }
}
