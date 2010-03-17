using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface INextRecurrable
    {
        /// <summary>
        /// Returns the next period of occurrence based on the
        /// date/time of the last occurrence, or null if no
        /// occurrence could be determined.
        /// </summary>
        IPeriod GetNextOccurrence(IDateTime lastOccurrence);
    }
}
