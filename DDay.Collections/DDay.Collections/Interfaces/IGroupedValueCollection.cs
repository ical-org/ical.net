using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.Collections
{
    public interface IGroupedValueCollection<TGroup, TInterface, TItem, TValueType> :
        IGroupedCollection<TGroup, TInterface>
        where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
        where TItem : new()
    {
        void Set(TGroup group, TValueType value);
        void Set(TGroup group, IEnumerable<TValueType> values);
        TType Get<TType>(TGroup group);
        IList<TType> GetMany<TType>(TGroup group);
    }
}
