using System.Collections.Generic;

namespace ical.net.collections
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
