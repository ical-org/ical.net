using System.Collections.Generic;

namespace ical.NET.Collections.Interfaces
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
