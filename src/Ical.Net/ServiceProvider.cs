using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net
{
    public class ServiceProvider
    {
        readonly IDictionary<Type, object> _mTypedServices = new Dictionary<Type, object>();
        readonly IDictionary<string, object> _mNamedServices = new Dictionary<string, object>();

        public object GetService(Type serviceType)
        {
            object service;
            _mTypedServices.TryGetValue(serviceType, out service);
            return service;
        }

        public object GetService(string name)
        {
            object service;
            _mNamedServices.TryGetValue(name, out service);
            return service;
        }

        public T GetService<T>()
        {
            var service = GetService(typeof (T));
            if (service is T service1)
            {
                return service1;
            }
            return default;
        }

        public T GetService<T>(string name)
        {
            var service = GetService(name);
            if (service is T service1)
            {
                return service1;
            }
            return default;
        }

        public void SetService(string name, object obj)
        {
            if (!string.IsNullOrEmpty(name) && obj != null)
            {
                _mNamedServices[name] = obj;
            }
        }

        public void SetService(object obj)
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

        public void RemoveService(Type type)
        {
            if (type != null)
            {
                if (_mTypedServices.ContainsKey(type))
                {
                    _mTypedServices.Remove(type);
                }

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces().Where(iface => _mTypedServices.ContainsKey(iface)))
                {
                    _mTypedServices.Remove(iface);
                }
            }
        }

        public void RemoveService(string name)
        {
            if (_mNamedServices.ContainsKey(name))
            {
                _mNamedServices.Remove(name);
            }
        }
    }
}