namespace ical.NET.Collections.Interfaces
{
    public interface IGroupedValueList<TGroup, TInterface, TItem, TValueType> :
        IGroupedValueCollection<TGroup, TInterface, TItem, TValueType>,
        IGroupedList<TGroup, TInterface>
        where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
        where TItem : new()
    {        
    }
}
