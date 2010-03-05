using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurrencePattern
    {
        int Count { get; set; }
        FrequencyType Frequency { get; set; }
        int Interval { get; set; }

        IList<iCalDateTime> StaticOccurrences { get; set; }
        iCalDateTime Until { get; set; }

        IList<iCalDateTime> Evaluate(iCalDateTime StartDate, iCalDateTime FromDate, iCalDateTime ToDate);
        bool CheckValidDate(iCalDateTime dt);
        bool IsValidDate(iCalDateTime dt);

        void IncrementDate(ref iCalDateTime dt, int amount);
    }
}
