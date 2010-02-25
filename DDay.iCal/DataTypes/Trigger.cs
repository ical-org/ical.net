using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that is used to specify exactly when an <see cref="Alarm"/> component will trigger.
    /// Usually this date/time is relative to the component to which the Alarm is associated.
    /// </summary>    
#if DATACONTRACT
    [DataContract(Name = "Trigger", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Trigger : iCalDataType
    {
        public enum TriggerRelation
        {
            Start,
            End
        }

        #region Private Fields

        private iCalDateTime m_DateTime;
        private Duration m_Duration;
        private TriggerRelation m_Related = TriggerRelation.Start;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public iCalDateTime DateTime
        {
            get { return m_DateTime; }
            set
            {
                m_DateTime = value;
                if (m_DateTime != null)
                {
                    // NOTE: this, along with the "Duration" setter, fixes the bug tested in
                    // TODO11(), as well as this thread: https://sourceforge.net/forum/forum.php?thread_id=1926742&forum_id=656447

                    // DateTime and Duration are mutually exclusive
                    Duration = null;

                    // Do not allow timeless date/time values
                    m_DateTime.HasTime = true;
                    AddParameter("VALUE", "DATE-TIME");
                }
                else Parameters.Remove("VALUE");
            }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public Duration Duration
        {
            get { return m_Duration; }
            set
            {
                m_Duration = value;
                if (m_Duration != null)
                {
                    // NOTE: see above.

                    // DateTime and Duration are mutually exclusive
                    DateTime = null;
                }
            }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public TriggerRelation Related
        {
            get
            {
                if (Parameters.ContainsKey("RELATED"))
                {
                    CalendarParameter p = (CalendarParameter)Parameters["RELATED"];
                    if (p.Values.Count > 0)
                        m_Related = (TriggerRelation)Enum.Parse(typeof(TriggerRelation), p.Values[0].ToString(), true);
                }
                return m_Related;
            }
            set
            {
                m_Related = value;
            }
        }

        public bool IsRelative
        {
            get { return m_Duration != null; }
        }

        #endregion

        #region Constructors

        public Trigger() { }
        public Trigger(TimeSpan ts)
        {
            Duration = ts;
        }
        public Trigger(string value)
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is Trigger)
            {
                Trigger t = (Trigger)obj;
                DateTime = t.DateTime;
                Duration = t.Duration;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarObject obj)
        {
            Trigger t = (Trigger)obj;

            if (ValueType() == typeof(iCalDateTime))
            {
                t.DateTime = new iCalDateTime();
                ICalendarObject dt = t.DateTime;
                return t.DateTime.TryParse(value, ref dt);
            }
            else
            {
                t.Duration = new Duration();
                ICalendarObject d = t.Duration;
                return t.Duration.TryParse(value, ref d);
            }
        }

        public override bool Equals(object obj)
        {
            Trigger t = obj as Trigger;
            if (t != null)
            {
                return
                    object.Equals(DateTime, t.DateTime) &&
                    object.Equals(Duration, t.Duration) &&
                    object.Equals(Related, t.Related);
            }
            return base.Equals(obj);
        }

        #endregion
    }
}
