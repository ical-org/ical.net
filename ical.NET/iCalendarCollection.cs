using System;
using System.Collections.Generic;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Structs;

namespace Ical.Net
{
    /// <summary>
    /// A list of iCalendars.
    /// </summary>
    [Serializable]
    public class CalendarCollection : List<ICalendar>, IICalendarCollection
    {
        #region IGetOccurrences Members

        public void ClearEvaluation()
        {
            foreach (var iCal in this)
            {
                iCal.ClearEvaluation();
            }
        }

        public HashSet<Occurrence> GetOccurrences(IDateTime dt)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(DateTime dt)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(IDateTime startTime, IDateTime endTime)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences(DateTime startTime, DateTime endTime)
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(IDateTime dt) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(DateTime dt) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(dt));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(IDateTime startTime, IDateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(startTime, endTime));
            }
            return occurrences;
        }

        public HashSet<Occurrence> GetOccurrences<T>(DateTime startTime, DateTime endTime) where T : IRecurringComponent
        {
            var occurrences = new HashSet<Occurrence>();
            foreach (var iCal in this)
            {
                occurrences.UnionWith(iCal.GetOccurrences<T>(startTime, endTime));
            }
            return occurrences;
        }

        #endregion

        #region Private Methods

        IFreeBusy CombineFreeBusy(IFreeBusy main, IFreeBusy current)
        {
            if (main != null)
            {
                main.MergeWith(current);
            }
            return current;
        }

        #endregion

        #region IGetFreeBusy Members

        public IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
            {
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(freeBusyRequest));
            }
            return fb;
        }

        public IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
            {
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(fromInclusive, toExclusive));
            }
            return fb;
        }

        public IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive)
        {
            IFreeBusy fb = null;
            foreach (var iCal in this)
            {
                fb = CombineFreeBusy(fb, iCal.GetFreeBusy(organizer, contacts, fromInclusive, toExclusive));
            }
            return fb;
        }

        #endregion
    }
}