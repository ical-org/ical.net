using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VALARM component.
    /// </summary>
#if SILVERLIGHT
    [DataContract(Name = "Alarm", Namespace="http://www.ddaysoftware.com/dday.ical/components/2009/07/")]
#else
    [Serializable]
#endif
    public class Alarm : ComponentBase
    {
        #region Static Public Methods

        static public Alarm Create(RecurringComponent rc)
        {
            Alarm alarm = rc.iCalendar.Create<Alarm>();
            return alarm;
        }

        #endregion

        #region Private Fields

        private List<AlarmOccurrence> m_Occurrences;

        private AlarmAction m_Action;        
        private Binary m_Attach;        
        private Cal_Address[] m_Attendee;        
        private Text m_Description;        
        private Duration m_Duration;        
        private Integer m_Repeat;        
        private Text m_Summary;        
        private Trigger m_Trigger;        

        #endregion

        #region Public Properties

        [Serialized]
        virtual public AlarmAction Action
        {
            get { return m_Action; }
            set { m_Action = value; }
        }

        [Serialized]
        virtual public Binary Attach
        {
            get { return m_Attach; }
            set { m_Attach = value; }
        }

        [Serialized]
        virtual public Cal_Address[] Attendee
        {
            get { return m_Attendee; }
            set { m_Attendee = value; }
        }

        [Serialized]
        virtual public Text Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [Serialized, DefaultValue("P")]
        virtual public Duration Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        [Serialized]
        virtual public Integer Repeat
        {
            get { return m_Repeat; }
            set { m_Repeat = value; }
        }

        [Serialized]
        virtual public Text Summary
        {
            get { return m_Summary; }
            set { m_Summary = value; }
        }

        [Serialized]
        virtual public Trigger Trigger
        {
            get { return m_Trigger; }
            set { m_Trigger = value; }
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
            Occurrences = new List<AlarmOccurrence>();
        }
        public Alarm(iCalObject parent)
            : base(parent)
        {
            this.Name = ComponentBase.ALARM;
            Occurrences = new List<AlarmOccurrence>();
        }

        #endregion                

        #region Public Methods

        /// <summary>
        /// Gets a list of alarm occurrences for the given recurring component, <paramref name="rc"/>
        /// that occur between <paramref name="FromDate"/> and <paramref name="ToDate"/>.
        /// </summary>
        virtual public List<AlarmOccurrence> GetOccurrences(RecurringComponent rc, iCalDateTime FromDate, iCalDateTime ToDate)
        {
            Occurrences.Clear();

            // If the trigger is relative, it can recur right along with
            // the recurring items, otherwise, it happens once and
            // only once (at a precise time).
            if (Trigger.IsRelative)
            {
                // Ensure that "FromDate" has already been set
                if (FromDate == null)
                    FromDate = rc.Start.Copy();

                Duration d = null;
                foreach (Occurrence o in rc.GetOccurrences(FromDate, ToDate))
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
            else Occurrences.Add(new AlarmOccurrence(this, Trigger.DateTime.Copy(), rc));

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
        /// <returns>A list of <see cref="AlarmOccurrence"/> objects, each containing a triggered alarm.</returns>
        public List<AlarmOccurrence> Poll(iCalDateTime Start, iCalDateTime End)
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

        /// <summary>
        /// Returns a typed copy of the Alarm.
        /// </summary>
        /// <returns>A typed copy of the Alarm object.</returns>
        public new Alarm Copy()
        {
            return (Alarm)base.Copy();
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
                    iCalDateTime alarmTime = ao.DateTime.Copy();

                    for (int j = 0; j < Repeat; j++)
                    {
                        alarmTime += Duration;
                        Occurrences.Add(new AlarmOccurrence(this, alarmTime.Copy(), ao.Component));
                    }
                }
            }
        }

        #endregion
    }
}
