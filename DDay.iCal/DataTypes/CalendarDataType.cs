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
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class CalendarDataType :
        ICalendarDataType
    {
        #region Private Fields
        
        ICalendarParameterList _Parameters;
        ServiceProvider _ServiceProvider;

        #endregion

        #region Protected Fields

        /// <summary>
        /// NOTE: we use a weak reference here to ensure this doesn't cause a memory leak.
        /// As this class merely provides a service to calendar properties, we shouldn't
        /// be holding on to memory references via this object anyhow.
        /// </summary>
        protected WeakReference _AssociatedObject;

        #endregion

        #region Constructors

        public CalendarDataType()
        {
            Initialize();
        }

        void Initialize()
        {
             _Parameters = new AssociatedCalendarParameterList(null, null);
             _ServiceProvider = new ServiceProvider();
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
            get { return _AssociatedObject != null ? _AssociatedObject.Target as ICalendarObject : null; }
            set
            {
                if ((value != null && _AssociatedObject == null) ||
                    (value == null) ||
                    !object.Equals(_AssociatedObject.Target, value))
                {
                    if (value != null)
                        _AssociatedObject = new WeakReference(value);
                    else
                        _AssociatedObject = null;

                    ICalendarObject associated = AssociatedObject;
                    _Parameters = new AssociatedCalendarParameterList(_Parameters, associated, associated as ICalendarParameterListContainer);
                }
            }
        }

        virtual public IICalendar Calendar
        {
            get
            {
                if (_AssociatedObject != null)
                    return AssociatedObject.Calendar;
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
                ICalendarObject associated = dt.AssociatedObject;
                AssociatedObject = associated;
                _Parameters = new AssociatedCalendarParameterList(dt.Parameters, associated, associated as ICalendarParameterListContainer);
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

        #region ICalendarParameterListContainer Members

        public ICalendarParameterList Parameters
        {
            get { return _Parameters; }
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return _ServiceProvider.GetService(serviceType);
        }

        public object GetService(string name)
        {
            return _ServiceProvider.GetService(name);
        }

        public T GetService<T>()
        {
            return _ServiceProvider.GetService<T>();
        }

        public T GetService<T>(string name)
        {
            return _ServiceProvider.GetService<T>(name);
        }

        public void SetService(string name, object obj)
        {
            _ServiceProvider.SetService(name, obj);
        }

        public void SetService(object obj)
        {
            _ServiceProvider.SetService(obj);
        }

        public void RemoveService(Type type)
        {
            _ServiceProvider.RemoveService(type);
        }

        public void RemoveService(string name)
        {
            _ServiceProvider.RemoveService(name);
        }

        #endregion
    }
}
