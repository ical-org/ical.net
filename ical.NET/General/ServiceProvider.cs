using System;
using System.Collections.Generic;

namespace Ical.Net.General
{
    public class ServiceProvider //: IServiceProvider
    {
        #region Private Fields

        private IDictionary<Type, object> _mTypedServices = new Dictionary<Type, object>();
        private IDictionary<string, object> _mNamedServices = new Dictionary<string, object>();

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            if (_mTypedServices.ContainsKey(serviceType))
                return _mTypedServices[serviceType];
            return null;
        }

        virtual public object GetService(string name)
        {
            if (_mNamedServices.ContainsKey(name))
                return _mNamedServices[name];
            return null;
        }

        virtual public T GetService<T>()
        {
            var service = GetService(typeof(T));
            if (service is T)
                return (T)service;
            return default(T);
        }

        virtual public T GetService<T>(string name)
        {
            var service = GetService(name);
            if (service is T)
                return (T)service;
            return default(T);
        }

        virtual public void SetService(string name, object obj)
        {
            if (!string.IsNullOrEmpty(name) && obj != null)
                _mNamedServices[name] = obj;
        }

        virtual public void SetService(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                _mTypedServices[type] = obj;

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                    _mTypedServices[iface] = obj;
            }
        }

        virtual public void RemoveService(Type type)
        {
            if (type != null)
            {
                if (_mTypedServices.ContainsKey(type))
                    _mTypedServices.Remove(type);

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                {
                    if (_mTypedServices.ContainsKey(iface))
                        _mTypedServices.Remove(iface);
                }
            }
        }

        virtual public void RemoveService(string name)
        {
            if (_mNamedServices.ContainsKey(name))
                _mNamedServices.Remove(name);
        }

        #endregion
    }
}
