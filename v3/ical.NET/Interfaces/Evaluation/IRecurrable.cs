using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using IServiceProvider = Ical.Net.Interfaces.General.IServiceProvider;

namespace Ical.Net.Interfaces.Evaluation
{
    public interface IRecurrable : IGetOccurrences, IServiceProvider
    {
        /// <summary>
        /// Gets/sets the start date/time of the component.
        /// </summary>
        IDateTime Start { get; set; }

        IList<PeriodList> ExceptionDates { get; set; }
        IList<RecurrencePattern> ExceptionRules { get; set; }
        IList<PeriodList> RecurrenceDates { get; set; }
        IList<RecurrencePattern> RecurrenceRules { get; set; }
        IDateTime RecurrenceId { get; set; }
    }
}