using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 2445 VALARM component.
    /// FIXME: move GetOccurrences() logic into an AlarmEvaluator.
    /// </summary>    
#if DATACONTRACT
    [DataContract(Name = "Alarm", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
    /*[KnownType(typeof(Binary))]
    [KnownType(typeof(Cal_Address))]
    [KnownType(typeof(Cal_Address[]))]
    [KnownType(typeof(Text))]
    [KnownType(typeof(Duration))]
    [KnownType(typeof(Integer))]
    [KnownType(typeof(Trigger))]    */
#else
    [Serializable]
#endif
    public class Alarm :
        CalendarComponent,
        IAlarm
    {
        #region Private Fields

        private List<AlarmOccurrence> m_Occurrences;

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
        virtual public IAttachment Attachment
        {
            get { return Properties.Get<IAttachment>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public IList<IAttendee> Attendees
        {
            get { return Properties.GetList<IAttendee>("ATTENDEE"); }
            set { Properties.SetList("ATTENDEE", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        virtual public string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        virtual public TimeSpan Duration
        {
            get { return Properties.Get<TimeSpan>("DURATION"); }
            set { Properties.Set("DURATION", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        virtual public int Repeat
        {
            get { return Properties.Get<int>("REPEAT"); }
            set { Properties.Set("REPEAT", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 7)]
#endif
        virtual public string Summary
        {
            get { return Properties.Get<string>("SUMMARY"); }
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

        virtual protected List<AlarmOccurrence> Occurrences
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

        void Initialize()
        {
            Name = Components.ALARM;
            Occurrences = new List<AlarmOccurrence>();
        }

        #endregion                

        #region Public Methods

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        virtual public IList<AlarmOccurrence> GetOccurrences(IRecurringComponent rc, IDateTime FromDate, IDateTime ToDate)
        {
            Occurrences.Clear();

            if (Trigger != null)
            {
                // If the trigger is relative, it can recur right along with
                // the recurring items, otherwise, it happens once and
                // only once (at a precise time).
                if (Trigger.IsRelative)
                {
                    // Ensure that "FromDate" has already been set
                    if (FromDate == null)
                        FromDate = rc.Start.Copy<IDateTime>();

                    TimeSpan d = default(TimeSpan);
                    foreach (Occurrence o in rc.GetOccurrences(FromDate, ToDate))
                    {
                        IDateTime dt = o.Period.StartTime;
                        if (Trigger.Related == TriggerRelation.End)
                        {
                            if (o.Period.EndTime != null)
                            {
                                dt = o.Period.EndTime;
                                if (d == default(TimeSpan))
                                    d = o.Period.Duration;
                            }
                            // Use the "last-found" duration as a reference point
                            else if (d != default(TimeSpan))
                                dt = o.Period.StartTime.Add(d);
                            else throw new ArgumentException("Alarm trigger is relative to the END of the occurrence; however, the occurence has no discernible end.");
                        }

                        Occurrences.Add(new AlarmOccurrence(this, dt.Add(Trigger.Duration.Value), rc));
                    }
                }
                else
                {
                    IDateTime dt = Trigger.DateTime.Copy<IDateTime>();
                    dt.AssociatedObject = this;
                    Occurrences.Add(new AlarmOccurrence(this, dt, rc));
                }

                // If a REPEAT and DURATION value were specified,
                // then handle those repetitions here.
                AddRepeatedItems();
            }

            return Occurrences;
        }

        /// <summary>
        /// Polls the <see cref="Alarm"/> component for alarms that have been triggered
        /// since the provided <paramref name="Start"/> date/time.  If <paramref name="Start"/>
        /// is null, all triggered alarms will be returned.
        /// </summary>
        /// <param name="Start">The earliest date/time to poll trigerred alarms for.</param>
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        virtual public IList<AlarmOccurrence> Poll(IDateTime Start, IDateTime End)
        {
            List<AlarmOccurrence> Results = new List<AlarmOccurrence>();

            // Evaluate the alarms to determine the recurrences
            RecurringComponent rc = Parent as RecurringComponent;
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
                    AlarmOccurrence ao = Occurrences[i];
                    IDateTime alarmTime = ao.DateTime.Copy<IDateTime>();

                    for (int j = 0; j < Repeat; j++)
                    {
                        alarmTime = alarmTime.Add(Duration);
                        Occurrences.Add(new AlarmOccurrence(this, alarmTime.Copy<IDateTime>(), ao.Component));
                    }
                }
            }
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion
    }
}
