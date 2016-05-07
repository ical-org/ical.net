using System.Collections.Generic;

namespace DDay.iCal
{
    public class FreeBusy :
        UniqueComponent,
        IFreeBusy
    {
        #region Static Public Methods

        static public IFreeBusy Create(ICalendarObject obj, IFreeBusy freeBusyRequest)
        {
            if (obj is IGetOccurrencesTyped)
            {
                var getOccurrences = (IGetOccurrencesTyped)obj;
                var occurrences = getOccurrences.GetOccurrences<IEvent>(freeBusyRequest.Start, freeBusyRequest.End);
                var contacts = new List<string>();
                var isFilteredByAttendees = false;
                
                if (freeBusyRequest.Attendees != null &&
                    freeBusyRequest.Attendees.Count > 0)
                {
                    isFilteredByAttendees = true;
                    foreach (var attendee in freeBusyRequest.Attendees)
                    {
                        if (attendee.Value != null)
                            contacts.Add(attendee.Value.OriginalString.Trim());                        
                    }
                }

                var fb = freeBusyRequest.Copy<IFreeBusy>();
                fb.UID = new UIDFactory().Build();
                fb.Entries.Clear();
                fb.DTStamp = iCalDateTime.Now;

                foreach (var o in occurrences)
                {
                    var uc = o.Source as IUniqueComponent;

                    if (uc != null)
                    {
                        var evt = uc as IEvent;
                        var accepted = false;
                        var type = FreeBusyStatus.Busy;
                        
                        // We only accept events, and only "opaque" events.
                        if (evt != null && evt.Transparency != TransparencyType.Transparent)
                            accepted = true;

                        // If the result is filtered by attendees, then
                        // we won't accept it until we find an event
                        // that is being attended by this person.
                        if (accepted && isFilteredByAttendees)
                        {
                            accepted = false;
                            foreach (var a in uc.Attendees)
                            {
                                if (a.Value != null && contacts.Contains(a.Value.OriginalString.Trim()))
                                {
                                    if (a.ParticipationStatus != null)
                                    {
                                        switch(a.ParticipationStatus.ToUpperInvariant())
                                        {
                                            case ParticipationStatus.Tentative:
                                                accepted = true;
                                                type = FreeBusyStatus.BusyTentative;
                                                break;
                                            case ParticipationStatus.Accepted:
                                                accepted = true;
                                                type = FreeBusyStatus.Busy;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        if (accepted)
                        {
                            // If the entry was accepted, add it to our list!
                            fb.Entries.Add(new FreeBusyEntry(o.Period, type));
                        }
                    }
                }

                return fb;
            }
            return null;
        }

        static public IFreeBusy CreateRequest(IDateTime fromInclusive, IDateTime toExclusive, IOrganizer organizer, IAttendee[] contacts)
        {
            var fb = new FreeBusy();
            fb.DTStamp = iCalDateTime.Now;
            fb.DTStart = fromInclusive;
            fb.DTEnd = toExclusive;
            if (organizer != null)
                fb.Organizer = organizer.Copy<IOrganizer>();
            if (contacts != null)
            {
                foreach (var attendee in contacts)
                    fb.Attendees.Add(attendee.Copy<IAttendee>());
            }

            return fb;
        }

        #endregion

        #region Constructors

        public FreeBusy()
        {
            Name = Components.FREEBUSY;
        }

        #endregion

        #region IFreeBusy Members

        virtual public IList<IFreeBusyEntry> Entries
        {
            get { return Properties.GetMany<IFreeBusyEntry>("FREEBUSY"); }
            set { Properties.Set("FREEBUSY", value); }
        }

        virtual public IDateTime DTStart
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        virtual public IDateTime DTEnd
        {
            get { return Properties.Get<IDateTime>("DTEND"); }
            set { Properties.Set("DTEND", value); }
        }

        virtual public IDateTime Start
        {
            get { return Properties.Get<IDateTime>("DTSTART"); }
            set { Properties.Set("DTSTART", value); }
        }

        virtual public IDateTime End
        {
            get { return Properties.Get<IDateTime>("DTEND"); }
            set { Properties.Set("DTEND", value); }
        }

        virtual public FreeBusyStatus GetFreeBusyStatus(IPeriod period)
        {
            var status = FreeBusyStatus.Free;
            if (period != null)
            {                
                foreach (var fbe in Entries)
                {
                    if (fbe.CollidesWith(period) && status < fbe.Status)
                        status = fbe.Status;
                }
            }
            return status;
        }

        virtual public FreeBusyStatus GetFreeBusyStatus(IDateTime dt)
        {
            var status = FreeBusyStatus.Free;
            if (dt != null)
            {
                foreach (var fbe in Entries)
                {
                    if (fbe.Contains(dt) && status < fbe.Status)
                        status = fbe.Status;
                }
            }
            return status;
        }

        #endregion

        #region IMergeable Members

        virtual public void MergeWith(IMergeable obj)
        {
            var fb = obj as IFreeBusy;
            if (fb != null)
            {
                foreach (var entry in fb.Entries)
                {
                    if (!Entries.Contains(entry))
                        Entries.Add(entry.Copy<IFreeBusyEntry>());
                }
            }
        }

        #endregion
    }
}
