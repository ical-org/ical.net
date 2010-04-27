using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IServiceProvider :
        System.IServiceProvider
    {
        object GetService(string name);
        T GetService<T>();
        T GetService<T>(string name);
        void SetService(string name, object obj);
        void SetService(object obj);
        void RemoveService(Type type);
        void RemoveService(string name);
    }
}
