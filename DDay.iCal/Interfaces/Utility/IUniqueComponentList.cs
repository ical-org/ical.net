using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    

    public interface IUniqueComponentListReadonly<T> :
        IEnumerable<T>
        where T : IUniqueComponent
    {
        bool ContainsKey(string UID);

        int Count { get; }

        // Indexers
        T this[int index] { get; set; }
        T this[string uid] { get; set; }
    }

    public interface IUniqueComponentList<T> :
        IList<T>,
        IUniqueComponentListReadonly<T>
        where T : IUniqueComponent
    {        
    }
}
