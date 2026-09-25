//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Ical.Net;

public class ServiceProvider
{
    private readonly IDictionary<Type, object> _mTypedServices = new Dictionary<Type, object>();
    private readonly IDictionary<string, object> _mNamedServices = new Dictionary<string, object>();

    public virtual object GetService(Type serviceType)
    {
        if (!_mTypedServices.TryGetValue(serviceType, out var service))
            throw new ArgumentException($"Service of type {serviceType.FullName} not found.", nameof(serviceType));

        return service;
    }

    public virtual object GetService(string name)
    {
        if (!_mNamedServices.TryGetValue(name, out var service))
            throw new ArgumentException($"Service with name {name} not found.", nameof(name));

        return service;
    }

    public virtual T GetService<T>() => (T) GetService(typeof(T));

    public virtual T GetService<T>(string name) => (T) GetService(name);

    public virtual void SetService(string name, object obj)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _mNamedServices[name] = obj;
        }
    }

    /// <summary>
    /// Registers <paramref name="obj"/> under its concrete type and under every interface it implements.
    /// </summary>
    [Obsolete("Use AddService<TService>(TService) instead. This overload reflects over the implemented interfaces and is not trimming- or AOT-safe.")]
    [RequiresUnreferencedCode("Reflects over the interfaces implemented by the argument's runtime type, which may be trimmed away.")]
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

    /// <summary>
    /// Registers <paramref name="impl"/> under exactly <typeparamref name="TService"/>, replacing
    /// any existing registration for that type.
    /// </summary>
    /// <remarks>
    /// Prefer this over <see cref="SetService(object)"/>: stating the service type explicitly,
    /// rather than reflecting over the implemented interfaces, keeps the registration trimming-
    /// and NativeAOT-safe.
    /// <para/>
    /// Deliberately not an overload of <see cref="SetService(object)"/>. As an overload it would
    /// win resolution for most call sites and silently narrow what an existing call registers,
    /// while suppressing the obsolete warning on the member it replaces.
    /// </remarks>
    public virtual void AddService<TService>(TService impl) where TService : notnull
        => _mTypedServices[typeof(TService)] = impl;

    /// <summary>
    /// Removes the service registered under <paramref name="type"/> and under every interface it implements.
    /// </summary>
    [Obsolete("Use RemoveService<TService>() instead. This overload reflects over the implemented interfaces and is not trimming- or AOT-safe.")]
    [RequiresUnreferencedCode("Reflects over the interfaces implemented by the given type, which may be trimmed away.")]
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

    /// <summary>
    /// Removes the service registered under exactly <typeparamref name="TService"/>.
    /// </summary>
    public virtual void RemoveService<TService>() => _mTypedServices.Remove(typeof(TService));
}
