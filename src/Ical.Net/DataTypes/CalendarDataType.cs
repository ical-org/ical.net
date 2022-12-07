using System;
using System.Runtime.Serialization;
using Ical.Net.Proxies;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
    public abstract class CalendarDataType : ICalendarDataType
    {
        IParameterCollection _parameters;
        ParameterCollectionProxy _proxy;
        ServiceProvider _serviceProvider;

        protected ICalendarObject _AssociatedObject;

        protected CalendarDataType()
        {
            Initialize();
        }

        void Initialize()
        {
            _parameters = new ParameterList();
            _proxy = new ParameterCollectionProxy(_parameters);
            _serviceProvider = new ServiceProvider();
        }

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

        protected virtual void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        protected virtual void OnDeserialized(StreamingContext context) {}

        /// <summary> See RFC 5545 Section 3.2.20. </summary>
        /// <exception cref="NotImplementedException">for "TIME" only </exception>
        public Type GetValueType()
            => _proxy == null || !_proxy.ContainsKey("VALUE")
                ? null
                : _proxy.Get("VALUE") switch
                {
                    "BINARY" => typeof(byte[]),
                    "BOOLEAN" => typeof(bool),
                    "CAL-ADDRESS" => typeof(Uri),
                    "DATE" => typeof(IDateTime),
                    "DATE-TIME" => typeof(IDateTime),
                    "DURATION" => typeof(TimeSpan),
                    "FLOAT" => typeof(double),
                    "INTEGER" => typeof(int),
                    "PERIOD" => typeof(Period),
                    "RECUR" => typeof(RecurrencePattern),
                    "TEXT" => typeof(string),
                    "TIME" => throw new NotImplementedException(), // FIXME: implement ISO.8601.2004
                    "URI" => typeof(Uri),
                    "UTC-OFFSET" => typeof(UtcOffset),
                    _ => null
                };

        public void SetValueType(string type)
        {
            _proxy?.Set("VALUE", type ?? type.ToUpper());
        }

        public virtual ICalendarObject AssociatedObject
        {
            get => _AssociatedObject;
            set
            {
                if (Equals(_AssociatedObject, value))
                {
                    return;
                }

                _AssociatedObject = value;
                if (_AssociatedObject != null)
                {
                    _proxy.SetParent(_AssociatedObject);
                    if (_AssociatedObject is ICalendarParameterCollectionContainer container)
                    {
                        _proxy.SetProxiedObject(container.Parameters);
                    }
                }
                else
                {
                    _proxy.SetParent(null);
                    _proxy.SetProxiedObject(_parameters);
                }
            }
        }

        public Calendar Calendar => _AssociatedObject?.Calendar;

        public string Language
        {
            get => Parameters.Get("LANGUAGE");
            set => Parameters.Set("LANGUAGE", value);
        }

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        public virtual void CopyFrom(ICopyable obj)
        {
            if (!(obj is ICalendarDataType dt))
            {
                return;
            }

            _AssociatedObject = dt.AssociatedObject;
            _proxy.SetParent(_AssociatedObject);
            _proxy.SetProxiedObject(dt.Parameters);
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
        public T Copy<T>()
        {
            var type = GetType();
            var obj = Activator.CreateInstance(type) as ICopyable;

            // Duplicate our values
            if (obj is T obj1)
            {
                obj.CopyFrom(this);
                return obj1;
            }
            return default;
        }

        public IParameterCollection Parameters => _proxy;

        public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        public object GetService(string name) => _serviceProvider.GetService(name);

        public T GetService<T>() => _serviceProvider.GetService<T>();

        public T GetService<T>(string name) => _serviceProvider.GetService<T>(name);

        public void SetService(string name, object obj) => _serviceProvider.SetService(name, obj);

        public void SetService(object obj) => _serviceProvider.SetService(obj);

        public void RemoveService(Type type) => _serviceProvider.RemoveService(type);

        public void RemoveService(string name) => _serviceProvider.RemoveService(name);
    }
}