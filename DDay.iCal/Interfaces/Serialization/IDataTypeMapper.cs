using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface IDataTypeMapper
    {
        void AddPropertyMapping(string name, Type objectType, bool allowsMultipleValuesPerProperty);
        void AddPropertyMapping(string name, TypeResolverDelegate resolver, bool allowsMultipleValuesPerProperty);
        void RemovePropertyMapping(string name);

        bool GetPropertyAllowsMultipleValues(object obj);
        Type GetPropertyMapping(object obj);
    }
}
