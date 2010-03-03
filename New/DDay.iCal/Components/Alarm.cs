using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal;
using DDay.iCal;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VALARM component.
    /// </summary>    
#if DATACONTRACT
    [DataContract(Name = "Alarm", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Alarm : 
        CalendarComponent,
        IAlarm
    {
        #region Static Public Methods

        static public Alarm Create(RecurringComponent rc)
        {
            Alarm alarm = rc.Calendar.Create<Alarm>();
            return alarm;
        }

        #endregion

        #region Private Fields

        private List<IAlarmOccurrence> m_Occurrences;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public AlarmAction Action
        {
            get { return Properties.Get<AlarmAction>("ACTION"); }
            set { Properties.Set("ACTION", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public IBinary Attach
        {
            get { return Properties.Get<IBinary>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public IAttendee[] Attendee
        {
            get { return Properties.Get<IAttendee[]>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        virtual public IText Description
        {
            get { return Properties.Get<IText>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        virtual public IDuration Duration
        {
            get { return Properties.Get<IDuration>("DURATION"); }
            set { Properties.Set("DURATION", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        virtual public IInteger Repeat
        {
            get { return Properties.Get<IInteger>("REPEAT"); }
            set { Properties.Set("REPEAT", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 7)]
#endif
        virtual public IText Summary
        {
            get { return Properties.Get<IText>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 8)]
#endif
        virtual public ITrigger Trigger
        {
            get { return Properties.Get<ITrigger>("TRIGGER"); }
            set { Properties.Set("TRIGGER", value); }
        }

        #endregion

        #region Protected Properties

        virtual protected List<IAlarmOccurrence> Occurrences
        {
            get { return m_Occurrences; }
            set { m_Occurrences = value; }
        }

        #endregion

        #region Constructors

        public Alarm()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Name = ComponentFactory.ALARM;
            Occurrences = new List<AlarmOccurrence>();
        }

        #endregion                

        #region Public Methods

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        virtual public IList<IAlarmOccurrence> GetOccurrences(IRecurringComponent rc, iCalDateTime FromDate, iCalDateTime ToDate)
        {
            Occurrences.Clear();

            // If the trigger is relative, it can recur right along with
            // the recurring items, otherwise, it happens once and
            // only once (at a precise time).
            if (Trigger.IsRelative)
            {
                // Ensure that "FromDate" has already been set
                if (FromDate == null)
                    FromDate = rc.Start;

                Duration d = null;
                foreach (IOccurrence o in rc.GetOccurrences(FromDate, ToDate))
                {
                    iCalDateTime dt = o.Period.StartTime;
                    if (Trigger.Related == Trigger.TriggerRelation.End)
                    {
                        if (o.Period.EndTime != null)
                        {
                            dt = o.Period.EndTime;
                            if (d == null)
                                d = o.Period.Duration;
                        }
                        // Use the "last-found" duration as a reference point
                        else if (d != null)
                            dt = o.Period.StartTime + d;
                        else throw new ArgumentException("Alarm trigger is relative to the END of the occurrence; however, the occurence has no discernible end.");
                    }

                    Occurrences.Add(new AlarmOccurrence(this, dt + Trigger.Duration, rc));
                }
            }
            else
            {
                Occurrences.Add(
                    new AlarmOccurrence(
                        this,
                        Trigger.DateTime,
                        rc
                    )
                );
            }

            // If a REPEAT and DURATION value were specified,
            // then handle those repetitions here.
            AddRepeatedItems();

            return Occurrences;
        }

        /// <summary>
        /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="Start"/> date/time.  If <paramref name="Start"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="Start">The earliest date/time to poll trigerred alarms for.</param>
        /// <param name="End">The latest date/time to poll trigerred alarms for.</param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        public IList<IAlarmOccurrence> Poll(iCalDateTime Start, iCalDateTime End)
        {
            List<IAlarmOccurrence> Results = new List<IAlarmOccurrence>();

            // Evaluate the alarms to determine the recurrences
            IRecurringComponent rc = Parent as IRecurringComponent;
            if (rc != null)
            {
                Results.AddRange(GetOccurrences(rc, Start, End));
                Results.Sort();
            }
            return Results;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles the repetitions that occur from the <c>REPEAT</c> and
        /// <c>DURATION</c> properties.  Each recurrence of the alarm will
        /// have its own set of generated repetitions.
        /// </summary>
        virtual protected void AddRepeatedItems()
        {
            if (Repeat != null)
            {
                int len = Occurrences.Count;
                for (int i = 0; i < len; i++)
                {
                    IAlarmOccurrence ao = Occurrences[i];
                    iCalDateTime alarmTime = ao.DateTime;

                    for (int j = 0; j < Repeat; j++)
                    {
                        alarmTime += Duration;
                        Occurrences.Add(new AlarmOccurrence(this, alarmTime, ao.Component));
                    }
                }
            }
        }

        #endregion
    }
}
