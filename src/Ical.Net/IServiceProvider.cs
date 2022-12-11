using System;

namespace Ical.Net
{
    public interface IServiceProvider
    {
        object? GetService(string name);
        object? GetService(Type type);
        T? GetService<T>() where T : class;
        T? GetService<T>(string name) where T : class;
        void SetService(string name, object obj);
        void SetService(object obj);
        void RemoveService(Type type);
        void RemoveService(string name);
    }
}