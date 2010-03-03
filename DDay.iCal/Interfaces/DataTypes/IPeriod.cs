using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IPeriod :
        ICalendarDataType,
        IComparable
    {
        iCalDateTime StartTime { get; set; }
        iCalDateTime EndTime { get; set; }
        IDuration Duration { get; set; }

        /// <summary>
        /// When true, comparisons between this and other <see cref="Period"/>
        /// objects are matched against the date only, and
        /// not the date-time combination.
        /// </summary>
        bool MatchesDateOnly { get; set; }
    }
}
