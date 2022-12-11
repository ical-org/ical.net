using System;
using System.Collections.Generic;

namespace Ical.Net.Serialization
{
    public class SerializationContext
    {
        static readonly SerializationContext Default_ = new SerializationContext();

        /// <summary>
        /// Gets the Singleton instance of the SerializationContext class.
        /// </summary>
        public static SerializationContext Default
        {
            get
            {
                // Create a new serialization context that doesn't contain any objects
                // (and is non-static).  That way, if any objects get pushed onto
                // the serialization stack when the Default serialization context is used,
                // and something goes wrong and the objects don't get popped off the stack,
                // we don't need to worry (as much) about a memory leak, because the
                // objects weren't pushed onto a stack referenced by a static variable.
                var ctx = new SerializationContext
                {
                    ServiceProvider_ = Default_.ServiceProvider_
                };
                return ctx;
            }
        }

        readonly Stack<WeakReference> Stack_ = new Stack<WeakReference>();
        ServiceProvider ServiceProvider_ = new ServiceProvider();

        public SerializationContext()
        {
            // Add some services by default
            SetService(new SerializerFactory());
            SetService(new CalendarComponentFactory());
            SetService(new DataTypeMapper());
            SetService(new EncodingStack());
            SetService(new EncodingProvider(this));
        }

        public void Push(object? item)
        {
            if (item != null)
            {
                Stack_.Push(new WeakReference(item));
            }
        }

        public object? Pop()
        {
            if (Stack_.Count <= 0)
            {
                return null;
            }
            var r = Stack_.Pop();
            if (r.IsAlive)
            {
                return r.Target;
            }
            return null;
        }

        public object? Peek()
        {
            if (Stack_.Count <= 0)
            {
                return null;
            }
            var r = Stack_.Peek();
            if (r.IsAlive)
            {
                return r.Target;
            }
            return null;
        }

        public object? GetService(Type serviceType) => ServiceProvider_.GetService(serviceType);

        public object? GetService(string name) => ServiceProvider_.GetService(name);

        public T? GetService<T>() where T : class => ServiceProvider_.GetService<T>();

        public T? GetService<T>(string name) where T : class => ServiceProvider_.GetService<T>(name);

        public void SetService(string name, object obj) => ServiceProvider_.SetService(name, obj);

        public void SetService(object obj) => ServiceProvider_.SetService(obj);

        public void RemoveService(Type type) => ServiceProvider_.RemoveService(type);

        public void RemoveService(string name) => ServiceProvider_.RemoveService(name);
    }
}