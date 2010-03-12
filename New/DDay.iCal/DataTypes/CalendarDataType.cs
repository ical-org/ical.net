using System;
using System.Collections;
using System.Text;
using System.Reflection;
using DDay.iCal;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;
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

        private Stack<ICalendarObject> _Associations = new Stack<ICalendarObject>();

        #endregion        
    
        #region ICalendarDataType Members

        virtual public ICalendarParameterList AssociatedParameters
        {
            get
            {
                ICalendarParameterListContainer c = AssociatedObject as ICalendarParameterListContainer;
                if (c != null)
                    return c.Parameters;
                return null;
            }
        }

        virtual public ICalendarObject AssociatedObject
        {
            get
            {
                if (_Associations != null &&
                    _Associations.Count > 0)
                    return _Associations.Peek();
                return null;
            }
        }

        virtual public IICalendar Calendar
        {
            get
            {
                if (_Associations != null &&
                    _Associations.Count > 0)
                    return _Associations.Peek().Calendar;
                return null;
            }
        }

        virtual public void AssociateWith(ICalendarObject obj)
        {
            if (obj != null)
                _Associations.Push(obj);
        }

        virtual public void Deassociate()
        {
            _Associations.Pop();
        }

        #endregion

        #region ICopyable Members

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        virtual public void CopyFrom(ICopyable obj)
        {
            if (obj is CalendarDataType)
            {
                CalendarDataType dt = (CalendarDataType)obj;
                _Associations = new Stack<ICalendarObject>(dt._Associations);
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

        public object GetService(Type serviceType)
        {
            if (serviceType != null)
            {
                if (typeof(IPeriodEvaluator).IsAssignableFrom(serviceType))
                {

                }
            }
            return null;
        }

        #endregion
    }
}
