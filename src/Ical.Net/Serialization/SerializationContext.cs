using System;
using System.Collections.Generic;

namespace Ical.Net.Serialization
{
    public class SerializationContext
    {
        static SerializationContext _default;

        /// <summary>
        /// Gets the Singleton instance of the SerializationContext class.
        /// </summary>
        public static SerializationContext Default
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
                var ctx = new SerializationContext
                {
                    _mServiceProvider = _default._mServiceProvider
                };
                return ctx;
            }
        }

        readonly Stack<WeakReference> _mStack = new Stack<WeakReference>();
        ServiceProvider _mServiceProvider = new ServiceProvider();

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializerFactory());
            SetService(new CalendarComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new EncodingStack());
            SetService(new EncodingProvider(this));
        }

        public void Push(object item)
        {
            if (item != null)
            {
                _mStack.Push(new WeakReference(item));
            }
        }

        public object Pop()
        {
            if (_mStack.Count > 0)
            {
                var r = _mStack.Pop();
                if (r.IsAlive)
                {
                    return r.Target;
                }
            }
            return null;
        }

        public object Peek()
        {
            if (_mStack.Count > 0)
            {
                var r = _mStack.Peek();
                if (r.IsAlive)
                {
                    return r.Target;
                }
            }
            return null;
        }

        public object GetService(Type serviceType) => _mServiceProvider.GetService(serviceType);

        public object GetService(string name) => _mServiceProvider.GetService(name);

        public T GetService<T>() => _mServiceProvider.GetService<T>();

        public T GetService<T>(string name) => _mServiceProvider.GetService<T>(name);

        public void SetService(string name, object obj)
        {
            _mServiceProvider.SetService(name, obj);
        }

        public void SetService(object obj)
        {
            _mServiceProvider.SetService(obj);
        }

        public void RemoveService(Type type)
        {
            _mServiceProvider.RemoveService(type);
        }

        public void RemoveService(string name)
        {
            _mServiceProvider.RemoveService(name);
        }
    }
}