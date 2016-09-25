using System.Collections.Generic;
using ical.net.Interfaces.Evaluation;

namespace ical.net.Interfaces
{
    public interface IICalendarCollection : IGetOccurrencesTyped, IList<ICalendar> {}
}