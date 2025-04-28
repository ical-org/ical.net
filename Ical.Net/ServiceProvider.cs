//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net;

public class ServiceProvider
{
    private readonly IDictionary<Type, object> _mTypedServices = new Dictionary<Type, object>();
    private readonly IDictionary<string, object> _mNamedServices = new Dictionary<string, object>();

    public virtual object? GetService(Type serviceType)
    {
        _mTypedServices.TryGetValue(serviceType, out var service);
        return service;
    }

    public virtual object? GetService(string name)
    {
        _mNamedServices.TryGetValue(name, out var service);
        return service;
    }

    public virtual T? GetService<T>()
    {
        var service = GetService(typeof(T));
        if (service is T svc)
        {
            return svc;
        }
        return default(T);
    }

    public virtual T? GetService<T>(string name)
    {
        var service = GetService(name);
        if (service is T svc)
        {
            return svc;
        }
        return default(T);
    }

    public virtual void SetService(string name, object obj)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _mNamedServices[name] = obj;
        }
    }

    public virtual void SetService(object obj)
    {
        var type = obj.GetType();
        _mTypedServices[type] = obj;

        // Get interfaces for the given type
        foreach (var interfaceType in type.GetInterfaces())
        {
            _mTypedServices[interfaceType] = obj;
        }
    }

    public virtual void RemoveService(Type type)
    {
        if (_mTypedServices.ContainsKey(type))
        {
            _mTypedServices.Remove(type);
        }

        foreach (var interfaceType in type.GetInterfaces().Where(interfaceType => _mTypedServices.ContainsKey(interfaceType)))
        {
            _mTypedServices.Remove(interfaceType);
        }
    }

    public virtual void RemoveService(string name)
    {
        if (_mNamedServices.ContainsKey(name))
        {
            _mNamedServices.Remove(name);
        }
    }
}
