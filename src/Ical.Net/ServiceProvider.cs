using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net
{
    public class ServiceProvider
    {
        readonly IDictionary<Type, object> TypedServices_ = new Dictionary<Type, object>();
        readonly IDictionary<string, object> NamedServices_ = new Dictionary<string, object>();

        public object GetService(Type serviceType) => TypedServices_[serviceType];
        public object GetService(string name) => NamedServices_[name];

        public T GetService<T>() => (T)GetService(typeof(T));
        public T GetService<T>(string name) => (T)GetService(name);

        public void SetService(string name, object? obj)
        {
            if (!string.IsNullOrEmpty(name) && obj != null)
            {
                NamedServices_[name] = obj;
            }
        }

        public void SetService(object? obj)
        {
            if (obj == null)
            {
                return;
            }
            var type = obj.GetType();
            TypedServices_[type] = obj;

            // Get interfaces for the given type
            foreach (var iFace in type.GetInterfaces())
            {
                TypedServices_[iFace] = obj;
            }
        }

        public void RemoveService(Type? type)
        {
            if (type == null)
            {
                return;
            }
            if (TypedServices_.ContainsKey(type))
            {
                TypedServices_.Remove(type);
            }

            // Get interfaces for the given type
            foreach (var iFace in type.GetInterfaces().Where(iface => TypedServices_.ContainsKey(iface)))
            {
                TypedServices_.Remove(iFace);
            }
        }

        public void RemoveService(string name)
        {
            if (NamedServices_.ContainsKey(name))
            {
                NamedServices_.Remove(name);
            }
        }
    }
}