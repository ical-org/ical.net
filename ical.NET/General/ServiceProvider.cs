using System;
using System.Collections.Generic;

namespace Ical.Net.General
{
    public class ServiceProvider //: IServiceProvider
    {
        #region Private Fields

        private IDictionary<Type, object> m_TypedServices = new Dictionary<Type, object>();
        private IDictionary<string, object> m_NamedServices = new Dictionary<string, object>();

        #endregion

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            if (m_TypedServices.ContainsKey(serviceType))
                return m_TypedServices[serviceType];
            return null;
        }

        virtual public object GetService(string name)
        {
            if (m_NamedServices.ContainsKey(name))
                return m_NamedServices[name];
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
                m_NamedServices[name] = obj;
        }

        virtual public void SetService(object obj)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                m_TypedServices[type] = obj;

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                    m_TypedServices[iface] = obj;
            }
        }

        virtual public void RemoveService(Type type)
        {
            if (type != null)
            {
                if (m_TypedServices.ContainsKey(type))
                    m_TypedServices.Remove(type);

                // Get interfaces for the given type
                foreach (var iface in type.GetInterfaces())
                {
                    if (m_TypedServices.ContainsKey(iface))
                        m_TypedServices.Remove(iface);
                }
            }
        }

        virtual public void RemoveService(string name)
        {
            if (m_NamedServices.ContainsKey(name))
                m_NamedServices.Remove(name);
        }

        #endregion
    }
}
