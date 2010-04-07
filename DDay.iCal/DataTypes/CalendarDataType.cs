using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DDay.iCal
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "CalendarDataType", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public abstract class CalendarDataType :
        ICalendarDataType
    {
        #region Private Fields
        
        ICalendarParameterList _Parameters;

        #endregion

        #region Protected Fields

        protected ICalendarObject _AssociatedObject;

        #endregion

        #region Constructors

        public CalendarDataType()
        {
            Initialize();
        }

        void Initialize()
        {
             _Parameters = new AssociatedCalendarParameterList(null, null);
        }

        #endregion

        #region Internal Methods

        [OnDeserializing]
        internal void DeserializingInternal(StreamingContext context)
        {
            OnDeserializing(context);
        }

        [OnDeserialized]
        internal void DeserializedInternal(StreamingContext context)
        {
            OnDeserialized(context);
        }

        #endregion

        #region Protected Methods

        virtual protected void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        virtual protected void OnDeserialized(StreamingContext context)
        {
        }

        #endregion
    
        #region ICalendarDataType Members

        virtual public Type GetValueType()
        {
            // See RFC 5545 Section 3.2.20.
            if (_Parameters != null && _Parameters.ContainsKey("VALUE"))
            {
                switch (_Parameters.Get("VALUE"))
                {
                    case "BINARY": return typeof(byte[]);
                    case "BOOLEAN": return typeof(bool);
                    case "CAL-ADDRESS": return typeof(Uri);
                    case "DATE": return typeof(IDateTime);
                    case "DATE-TIME": return typeof(IDateTime);
                    case "DURATION": return typeof(TimeSpan);
                    case "FLOAT": return typeof(double);
                    case "INTEGER": return typeof(int);
                    case "PERIOD": return typeof(IPeriod);
                    case "RECUR": return typeof(IRecurrencePattern);
                    case "TEXT": return typeof(string);
                    case "TIME":
                        // FIXME: implement ISO.8601.2004
                        throw new NotImplementedException();
                    case "URI": return typeof(Uri);
                    case "UTC-OFFSET": return typeof(IUTCOffset);
                    default:
                        return null;
                }
            }
            return null;
        }

        virtual public void SetValueType(string type)
        {
            if (_Parameters != null)
                _Parameters.Set("VALUE", type != null ? type : type.ToUpper());
        }

        virtual public ICalendarObject AssociatedObject
        {
            get { return _AssociatedObject; }
            set
            {
                if (!object.Equals(_AssociatedObject, value))
                {
                    _AssociatedObject = value;
                    _Parameters = new AssociatedCalendarParameterList(_Parameters, _AssociatedObject, _AssociatedObject as ICalendarParameterListContainer);
                }
            }
        }

        virtual public IICalendar Calendar
        {
            get
            {
                if (_AssociatedObject != null)
                    return _AssociatedObject.Calendar;
                return null;
            }
        }

        virtual public string Language
        {
            get { return Parameters.Get("LANGUAGE"); }
            set { Parameters.Set("LANGUAGE", value); }
        }

        #endregion

        #region ICopyable Members

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        virtual public void CopyFrom(ICopyable obj)
        {
            if (obj is ICalendarDataType)
            {
                ICalendarDataType dt = (ICalendarDataType)obj;
                AssociatedObject = dt.AssociatedObject;
                _Parameters = new AssociatedCalendarParameterList(dt.Parameters, _AssociatedObject, _AssociatedObject as ICalendarParameterListContainer);
            }
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
        virtual public T Copy<T>()
        {
            ICopyable obj = null;
            Type type = GetType();
            obj = Activator.CreateInstance(type) as ICopyable;

            // Duplicate our values
            if (obj is T)
            {
                obj.CopyFrom(this);
                return (T)obj;
            }
            return default(T);
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return null;
        }

        #endregion

        #region ICalendarParameterListContainer Members

        public ICalendarParameterList Parameters
        {
            get { return _Parameters; }
        }

        #endregion
    }
}
