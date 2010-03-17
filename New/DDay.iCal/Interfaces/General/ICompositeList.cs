using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public interface ICompositeList :
        IEnumerable
    {
        event EventHandler<ObjectEventArgs<object>> ItemAdded;
        event EventHandler<ObjectEventArgs<object>> ItemRemoved;                
    }

    public interface ICompositeList<T> :
        IList<T>,
        ICompositeList
    {
        void AddList(IEnumerable list);
        void RemoveList(IList<T> list);
        void AddListRange(IEnumerable<IEnumerable> lists);
    }
}
