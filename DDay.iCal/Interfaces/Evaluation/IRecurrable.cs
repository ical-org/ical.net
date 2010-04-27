using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IRecurrable :
        IGetOccurrences,
        IServiceProvider
    {
        [Obsolete("Use the Start property instead.")]
        IDateTime DTStart { get; set; }

        /// <summary>
        /// Gets/sets the start date/time of the component.
        /// </summary>
        IDateTime Start { get; set; }

        IList<IPeriodList> ExceptionDates { get; set; }
        IList<IRecurrencePattern> ExceptionRules { get; set; }
        IList<IPeriodList> RecurrenceDates { get; set; }
        IList<IRecurrencePattern> RecurrenceRules { get; set; }
        IDateTime RecurrenceID { get; set; }        
    }
}
