using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// A class that is used to specify exactly when an <see cref="Alarm"/> component will trigger.
    /// Usually this date/time is relative to the component to which the Alarm is associated.
    /// </summary>    
    public class Trigger : iCalDataType
    {
        public enum TriggerRelation
        {
            START,
            END
        }

        #region Private Fields

        private Date_Time m_DateTime;
        private Duration m_Duration;
        private TriggerRelation m_Related = TriggerRelation.START;
                
        #endregion

        #region Public Properties

        public Date_Time DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; }
        }

        public Duration Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public TriggerRelation Related
        {
            get
            {       
                if (Parameters.ContainsKey("RELATED"))
                {
                    Parameter p = (Parameter)Parameters["RELATED"];
                    if (p.Values.Count > 0)
                        m_Related = (TriggerRelation)Enum.Parse(typeof(TriggerRelation), p.Values[0].ToString());
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
            : this()
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(object obj)
        {
            if (obj is Trigger)
            {
                Trigger t = (Trigger)obj;
                DateTime = t.DateTime;
                Duration = t.Duration;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref object obj)
        {
            Trigger t = (Trigger)obj;
            
            if (ValueType() == typeof(Date_Time))
            {
                t.DateTime = new Date_Time();
                object dt = t.DateTime;
                return t.DateTime.TryParse(value, ref dt);
            }
            else
            {
                t.Duration = new Duration();
                object d = t.Duration;
                return t.Duration.TryParse(value, ref d);
            }
        }

        #endregion
    }
}
