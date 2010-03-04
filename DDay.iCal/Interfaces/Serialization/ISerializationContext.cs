using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void Push(object item);
        object Pop();
        object Peek();

        void SetService(object obj);
        void RemoveService(Type type);
    }
}
