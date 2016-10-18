﻿using System;
using System.Collections.Generic;
using ical.net.General;
using ical.net.Interfaces.Components;
using ical.net.Interfaces.General;
using ical.net.Serialization.Factory;
using ical.net.Serialization.iCalendar.Factory;
using ical.net.Serialization.iCalendar.Processors;
using IServiceProvider = ical.net.Interfaces.General.IServiceProvider;

namespace ical.net.Serialization
{
    public class SerializationContext : IServiceProvider// : SerializationContext
    {
        private static SerializationContext _default;

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

        private readonly Stack<WeakReference> _mStack = new Stack<WeakReference>();
        private ServiceProvider _mServiceProvider = new ServiceProvider();

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializationSettings());
            SetService(new SerializerFactory());
            SetService(new ComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new EncodingStack());
            SetService(new EncodingProvider(this));
            SetService(new CompositeProcessor<Calendar>());
            SetService(new CompositeProcessor<ICalendarComponent>());
            SetService(new CompositeProcessor<ICalendarProperty>());
        }

        public virtual void Push(object item)
        {
            if (item != null)
            {
                _mStack.Push(new WeakReference(item));
            }
        }

        public virtual object Pop()
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

        public virtual object Peek()
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

        public virtual object GetService(Type serviceType)
        {
            return _mServiceProvider.GetService(serviceType);
        }

        public virtual object GetService(string name)
        {
            return _mServiceProvider.GetService(name);
        }

        public virtual T GetService<T>()
        {
            return _mServiceProvider.GetService<T>();
        }

        public virtual T GetService<T>(string name)
        {
            return _mServiceProvider.GetService<T>(name);
        }

        public virtual void SetService(string name, object obj)
        {
            _mServiceProvider.SetService(name, obj);
        }

        public virtual void SetService(object obj)
        {
            _mServiceProvider.SetService(obj);
        }

        public virtual void RemoveService(Type type)
        {
            _mServiceProvider.RemoveService(type);
        }

        public virtual void RemoveService(string name)
        {
            _mServiceProvider.RemoveService(name);
        }
    }
}