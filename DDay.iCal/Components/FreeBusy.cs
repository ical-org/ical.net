using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                IGetOccurrencesTyped getOccurrences = (IGetOccurrencesTyped)obj;
                IList<Occurrence> occurrences = getOccurrences.GetOccurrences<IEvent>(freeBusyRequest.Start, freeBusyRequest.End);
                List<string> contacts = new List<string>();
                bool isFilteredByAttendees = false;
                
                if (freeBusyRequest.Attendees != null &&
                    freeBusyRequest.Attendees.Count > 0)
                {
                    isFilteredByAttendees = true;
                    foreach (IAttendee attendee in freeBusyRequest.Attendees)
                    {
                        if (attendee.Value != null)
                            contacts.Add(attendee.Value.OriginalString.Trim());                        
                    }
                }

                IFreeBusy fb = freeBusyRequest.Copy<IFreeBusy>();
                fb.UID = new UIDFactory().Build();
                fb.Entries.Clear();
                fb.DTStamp = iCalDateTime.Now;

                foreach (Occurrence o in occurrences)
                {
                    IUniqueComponent uc = o.Source as IUniqueComponent;

                    if (uc != null)
                    {
                        IEvent evt = uc as IEvent;
                        bool accepted = false;
                        FreeBusyType type = FreeBusyType.Busy;
                        
                        // We only accept events, and only "opaque" events.
                        if (evt != null && evt.Transparency != TransparencyType.Transparent)
                            accepted = true;

                        // If the result is filtered by attendees, then
                        // we won't accept it until we find an event
                        // that is being attended by this person.
                        if (accepted && isFilteredByAttendees)
                        {
                            accepted = false;
                            foreach (IAttendee a in uc.Attendees)
                            {
                                if (a.Value != null && contacts.Contains(a.Value.OriginalString.Trim()))
                                {
                                    if (a.ParticipationStatus != null)
                                    {
                                        switch(a.ParticipationStatus.ToUpperInvariant())
                                        {
                                            case ParticipationStatus.Tentative:
                                                accepted = true;
                                                type = FreeBusyType.BusyTentative;
                                                break;
                                            case ParticipationStatus.Accepted:
                                                accepted = true;
                                                type = FreeBusyType.Busy;
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
            FreeBusy fb = new FreeBusy();
            fb.DTStamp = iCalDateTime.Now;
            fb.DTStart = fromInclusive;
            fb.DTEnd = toExclusive;
            if (organizer != null)
                fb.Organizer = organizer.Copy<IOrganizer>();
            if (contacts != null)
            {
                foreach (IAttendee attendee in contacts)
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
            get { return Properties.GetList<IFreeBusyEntry>("FREEBUSY"); }
            set { Properties.SetList("FREEBUSY", value); }
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

        #endregion

        #region IMergeable Members

        virtual public void MergeWith(IMergeable obj)
        {
            IFreeBusy fb = obj as IFreeBusy;
            if (fb != null)
            {
                foreach (IFreeBusyEntry entry in fb.Entries)
                {
                    if (!Entries.Contains(entry))
                        Entries.Add(entry.Copy<IFreeBusyEntry>());
                }
            }
        }

        #endregion
    }
}
