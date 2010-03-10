using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICompositeList<T> :
        IList<T>
    {
        void AddList(IList<T> list);
        void RemoveList(IList<T> list);
        void AddListRange(IEnumerable<IList<T>> lists);
    }
}
