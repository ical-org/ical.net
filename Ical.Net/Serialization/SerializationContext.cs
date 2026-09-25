//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ical.Net.Serialization;

public class SerializationContext
{
    private static SerializationContext? _default;

    /// <summary>
    /// Gets the Singleton instance of the SerializationContext class.
    /// </summary>
    public static SerializationContext Default
    {
        get
        {
            _default ??= new SerializationContext();

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
        // Registered directly against the provider rather than through the overridable
        // AddService, so a derived type cannot intercept these before its own constructor runs.
        _mServiceProvider.AddService<ISerializerFactory>(new SerializerFactory());
        _mServiceProvider.AddService(new CalendarComponentFactory());
        _mServiceProvider.AddService(new DataTypeMapper());
        _mServiceProvider.AddService(new EncodingStack());
        _mServiceProvider.AddService<IEncodingProvider>(new EncodingProvider(this));
    }

    public virtual void Push(object? item)
    {
        if (item != null)
        {
            _mStack.Push(new WeakReference(item));
        }
    }

    public virtual object? Pop()
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

    public virtual object? Peek()
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

    public virtual object GetService(Type serviceType) => _mServiceProvider.GetService(serviceType);

    public virtual object GetService(string name) => _mServiceProvider.GetService(name);

    public virtual T GetService<T>() => _mServiceProvider.GetService<T>();

    public virtual T GetService<T>(string name) => _mServiceProvider.GetService<T>(name);

    public virtual void SetService(string name, object obj) => _mServiceProvider.SetService(name, obj);

    [Obsolete("Use AddService<TService>(TService) instead. This overload reflects over the implemented interfaces and is not trimming- or AOT-safe.")]
    [RequiresUnreferencedCode("Reflects over the interfaces implemented by the argument's runtime type, which may be trimmed away.")]
    public virtual void SetService(object obj) => _mServiceProvider.SetService(obj);

    /// <summary>
    /// Registers <paramref name="impl"/> under exactly <typeparamref name="TService"/>, replacing
    /// any existing registration for that type.
    /// </summary>
    public virtual void AddService<TService>(TService impl) where TService : notnull => _mServiceProvider.AddService(impl);

    [Obsolete("Use RemoveService<TService>() instead. This overload reflects over the implemented interfaces and is not trimming- or AOT-safe.")]
    [RequiresUnreferencedCode("Reflects over the interfaces implemented by the given type, which may be trimmed away.")]
    public virtual void RemoveService(Type type) => _mServiceProvider.RemoveService(type);

    public virtual void RemoveService(string name) => _mServiceProvider.RemoveService(name);

    /// <summary>
    /// Removes the service registered under exactly <typeparamref name="TService"/>.
    /// </summary>
    public virtual void RemoveService<TService>() => _mServiceProvider.RemoveService<TService>();
}
