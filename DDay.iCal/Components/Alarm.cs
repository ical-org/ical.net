using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents an RFC 2445 VALARM component.
    /// </summary>
    public class Alarm : ComponentBase
    {
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

        public List<AlarmOccurrence> Occurrences
        {
            get { return m_Occurrences; }
            set { m_Occurrences = value; }
        }

        [Serialized]
        public AlarmAction Action
        {
            get { return m_Action; }
            set { m_Action = value; }
        }

        [Serialized]
        public Binary Attach
        {
            get { return m_Attach; }
            set { m_Attach = value; }
        }

        [Serialized]
        public Cal_Address[] Attendee
        {
            get { return m_Attendee; }
            set { m_Attendee = value; }
        }

        [Serialized]
        public Text Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [Serialized, DefaultValue("P")]
        public Duration Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        [Serialized]
        public Integer Repeat
        {
            get { return m_Repeat; }
            set { m_Repeat = value; }
        }

        [Serialized]
        public Text Summary
        {
            get { return m_Summary; }
            set { m_Summary = value; }
        }

        [Serialized]
        public Trigger Trigger
        {
            get { return m_Trigger; }
            set { m_Trigger = value; }
        }

        #endregion

        #region Static Public Methods

        static public Alarm Create(RecurringComponent rc)
        {
            Alarm alarm = (Alarm)rc.iCalendar.Create(rc, "VALARM");
            return alarm;
        }

        #endregion

        #region Constructors

        public Alarm(iCalObject parent)
            : base(parent)
        {            
            this.Name = "VALARM";
            Occurrences = new List<AlarmOccurrence>();
        }

        #endregion                

        #region Public Methods

        /// <summary>
        /// Evaluates <see cref="Alarm"/>s for the given recurring component, <paramref name="rc"/>.
        /// This evaluation is based on the evaluation period for the <see cref="RecurringComponent"/>.        
        /// </summary>
        /// <param name="rc">The </param>
        /// <returns></returns>
        virtual public List<AlarmOccurrence> Evaluate(RecurringComponent rc)
        {
            Occurrences.Clear();

            // If the trigger is relative, it can recur right along with
            // the recurring items, otherwise, it happens once and
            // only once (at a precise time).            
            if (Trigger.IsRelative)
            {
                Duration d = null;
                foreach (Period p in rc.Periods)
                {
                    Date_Time dt = p.StartTime;
                    if (Trigger.Related == Trigger.TriggerRelation.END)
                    {
                        if (p.EndTime != null)
                        {
                            dt = p.EndTime;
                            if (d == null)
                                d = p.Duration;
                        }
                        // Use the "last-found" duration as a reference point
                        else if (d != null)
                            dt = p.StartTime + d;
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
        public List<AlarmOccurrence> Poll(Date_Time Start)
        {
            List<AlarmOccurrence> Results = new List<AlarmOccurrence>();
            foreach (AlarmOccurrence ao in Occurrences)
            {
                if ((Start == null || ao.DateTime >= Start) && ao.DateTime <= DateTime.Now)
                    Results.Add(ao.Copy());
            }
            return Results;
        }

        /// <summary>
        /// Returns a typed copy of the Alarm.
        /// </summary>
        /// <returns>A typed copy of the Alarm object.</returns>
        public Alarm Copy()
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
                    Date_Time AlarmTime = ao.DateTime.Copy();

                    for (int j = 0; j < Repeat; j++)
                    {
                        AlarmTime += Duration;
                        Occurrences.Add(new AlarmOccurrence(this, AlarmTime.Copy(), ao.Component));
                    }
                }
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class that represents a specific occurrence of an <see cref="Alarm"/>.        
        /// </summary>
        /// <remarks>
        /// The <see cref="AlarmOccurrence"/> contains the <see cref="Date_Time"/> when
        /// the alarm occurs, the <see cref="Alarm"/> that fired, and the 
        /// component on which the alarm fired.
        /// </remarks>
        public class AlarmOccurrence
        {
            #region Private Fields

            private Alarm m_Alarm;            
            private Date_Time m_DateTime;            
            private RecurringComponent m_Component;            

            #endregion

            #region Public Properties

            public Alarm Alarm
            {
                get { return m_Alarm; }
                set { m_Alarm = value; }
            }

            public Date_Time DateTime
            {
                get { return m_DateTime; }
                set { m_DateTime = value; }
            }

            public RecurringComponent Component
            {
                get { return m_Component; }
                set { m_Component = value; }
            }

            #endregion

            #region Constructors

            public AlarmOccurrence(AlarmOccurrence ao)
            {
                this.Alarm = ao.Alarm;
                this.DateTime = ao.DateTime.Copy();
                this.Component = ao.Component;
            }
            public AlarmOccurrence(Alarm a, Date_Time dt, RecurringComponent rc)
            {
                this.Alarm = a;
                this.DateTime = dt;
                this.Component = rc;
            }

            #endregion

            #region Public Methods

            public AlarmOccurrence Copy()
            {
                AlarmOccurrence ao = new AlarmOccurrence(this);
                return ao;
            }

            #endregion
        }
        
        #endregion
    }
}
