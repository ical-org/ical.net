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

        ICalendarParameterList _Parameters = new AssociatedCalendarParameterList(null, null);

        #endregion

        #region Protected Fields

        protected ICalendarObject _AssociatedObject;

        #endregion        
    
        #region ICalendarDataType Members

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
