using System.Collections.Generic;

namespace Ical.Net.Collections
{
    public class MultiLinkedList<TType> :
        List<TType>
    {
        public virtual int StartIndex => 0;
        public virtual int ExclusiveEnd => Count > 0
            ? StartIndex + Count
            : StartIndex;
    }
}
