using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface ISerializationContext : 
        IServiceProvider
    {
        void AddService(object obj);
        void RemoveService(Type type);
    }
}
