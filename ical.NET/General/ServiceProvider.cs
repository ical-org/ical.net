﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.General
{
    public class ServiceProvider //: IServiceProvider
    {
        private readonly IDictionary<Type, object> _mTypedServices = new Dictionary<Type, object>();
        private readonly IDictionary<string, object> _mNamedServices = new Dictionary<string, object>();

        public virtual object GetService(Type serviceType)
        {
            object service;
            _mTypedServices.TryGetValue(serviceType, out service);
            return service;
        }

        public virtual object GetService(string name)
        {
            object service;
            _mNamedServices.TryGetValue(name, out service);
            return service;
        }

        public virtual T GetService<T>()
        {
            var service = GetService(typeof (T));
            if (service is T)
            {
                return (T) service;
            }
            return default(T);
        }

        public virtual T GetService<T>(string name)
        {
            var service = GetService(name);
            if (service is T)
            {
                return (T) service;
            }
            return default(T);
        }

        public virtual void SetService(string name, object obj)
        {
            if (!string.IsNullOrEmpty(name) && obj != null)
            {
                _mNamedServices[name] = obj;
            }
        }

        public virtual void SetService(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                _mTypedServices[type] = obj;

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                {
                    _mTypedServices[iface] = obj;
                }
            }
        }

        public virtual void RemoveService(Type type)
        {
            if (type != null)
            {
                _mTypedServices.Remove(type);

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                {
                    _mTypedServices.Remove(iface);
                }
            }
        }

        public virtual void RemoveService(string name)
        {
            _mNamedServices.Remove(name);
        }
    }
}