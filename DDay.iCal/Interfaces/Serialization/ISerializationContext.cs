using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();

        object GetService(string name);
        void SetService(string name, object obj);
        void SetService(object obj);
        void RemoveService(Type type);
        void RemoveService(string name);
    }
}
