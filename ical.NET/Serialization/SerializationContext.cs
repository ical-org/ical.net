using System;
using System.Collections.Generic;
using Ical.Net.General;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Factory;
using Ical.Net.Serialization.iCalendar.Processors;

namespace Ical.Net.Serialization
{
    public class SerializationContext :        
        ISerializationContext
    {
        #region Static Private Fields

        static private SerializationContext _default;

        #endregion

        #region Static Public Properties

        /// <summary>
        /// Gets the Singleton instance of the SerializationContext class.
        /// </summary>
        static public ISerializationContext Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new SerializationContext();
                }

                // Create a new serialization context that doesn't contain any objects
                // (and is non-static).  That way, if any objects get pushed onto
                // the serialization stack when the Default serialization context is used,
                // and something goes wrong and the objects don't get popped off the stack,
                // we don't need to worry (as much) about a memory leak, because the
                // objects weren't pushed onto a stack referenced by a static variable.
                var ctx = new SerializationContext();
                ctx._mServiceProvider = _default._mServiceProvider;
                return ctx;
            }
        }

        #endregion

        #region Private Fields

        private Stack<WeakReference> _mStack = new Stack<WeakReference>();
        private ServiceProvider _mServiceProvider = new ServiceProvider();

        #endregion

        #region Constructors

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializationSettings());
            SetService(new SerializerFactory());
            SetService(new ComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new EncodingStack());
            SetService(new EncodingProvider(this));            
            SetService(new CompositeProcessor<IICalendar>());
            SetService(new CompositeProcessor<ICalendarComponent>());
            SetService(new CompositeProcessor<ICalendarProperty>());            
        }

        #endregion

        #region ISerializationContext Members

        virtual public void Push(object item)
        {
            if (item != null)
                _mStack.Push(new WeakReference(item));
        }

        virtual public object Pop()
        {
            if (_mStack.Count > 0)
            {
                var r = _mStack.Pop();
                if (r.IsAlive)
                    return r.Target;
            }
            return null;
        }

        virtual public object Peek()
        {
            if (_mStack.Count > 0)
            {
                var r = _mStack.Peek();
                if (r.IsAlive)
                    return r.Target;
            }
            return null;
        }

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return _mServiceProvider.GetService(serviceType);
        }

        virtual public object GetService(string name)
        {
            return _mServiceProvider.GetService(name);
        }

        virtual public T GetService<T>()
        {
            return _mServiceProvider.GetService<T>();
        }

        virtual public T GetService<T>(string name)
        {
            return _mServiceProvider.GetService<T>(name);
        }

        virtual public void SetService(string name, object obj)
        {
            _mServiceProvider.SetService(name, obj);
        }

        virtual public void SetService(object obj)
        {
            _mServiceProvider.SetService(obj);
        }

        virtual public void RemoveService(Type type)
        {
            _mServiceProvider.RemoveService(type);
        }

        virtual public void RemoveService(string name)
        {
            _mServiceProvider.RemoveService(name);
        }

        #endregion
    }
}
