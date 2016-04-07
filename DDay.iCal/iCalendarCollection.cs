using System;
using System.Collections.Generic;

namespace DDay.iCal
{
    /// <summary>
    /// A list of iCalendars.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class iCalendarCollection :
        List<IICalendar>,
        IICalendarCollection
    {
        #region IGetOccurrences Members

        public void ClearEvaluation()
        {
            foreach (var iCal in this)
                iCal.ClearEvaluation();
        }

        public IList<Occurrence> GetOccurrences(IDateTime dt)
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences(dt));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences(DateTime dt)
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences(dt));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences(startTime, endTime));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences(startTime, endTime));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences<T>(dt));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences<T>(dt));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences<T>(startTime, endTime));
            occurrences.Sort();
            return occurrences;
        }

        public IList<Occurrence> GetOccurrences<T>(DateTime startTime, DateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new List<Occurrence>();
            foreach (var iCal in this)
                occurrences.AddRange(iCal.GetOccurrences<T>(startTime, endTime));
            occurrences.Sort();
            return occurrences;
        }

        #endregion

        #region Private Methods

        IFreeBusy CombineFreeBusy(IFreeBusy main, IFreeBusy current)
        {
            if (main != null)
                main.MergeWith(current);
            return current;
        }

        #endregion

        #region IGetFreeBusy Members

        public IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(freeBusyRequest));
            return fb;
        }

        public IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(fromInclusive, toExclusive));
            return fb;
        }

        public IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(organizer, contacts, fromInclusive, toExclusive));
            return fb;
        }

        #endregion
    }
}
