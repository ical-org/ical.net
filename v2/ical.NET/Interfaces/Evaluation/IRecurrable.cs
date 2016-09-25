using System;
using System.Collections.Generic;
using ical.net.Interfaces.DataTypes;
using IServiceProvider = ical.net.Interfaces.General.IServiceProvider;

namespace ical.net.Interfaces.Evaluation
{
    public interface IRecurrable : IGetOccurrences, IServiceProvider
    {
        [Obsolete("Use the Start property instead.")]
        IDateTime DtStart { get; set; }

        /// <summary>
        /// Gets/sets the start date/time of the component.
        /// </summary>
        IDateTime Start { get; set; }

        IList<IPeriodList> ExceptionDates { get; set; }
        IList<IRecurrencePattern> ExceptionRules { get; set; }
        IList<IPeriodList> RecurrenceDates { get; set; }
        IList<IRecurrencePattern> RecurrenceRules { get; set; }
        IDateTime RecurrenceId { get; set; }
    }
}