using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface IDataTypeMapper
    {
        void AddPropertyMapping(string name, Type objectType);
        void AddPropertyMapping(string name, TypeResolverDelegate resolver);
        void RemovePropertyMapping(string name);

        Type GetPropertyMapping(object obj);
    }
}
