using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IPeriodList :
        IEncodableDataType,
        ICollection<IPeriod>
    {
        string TZID { get; set; }

        IPeriod this[int index] { get; set; }
        void Add(IDateTime dt);
        void Remove(IDateTime dt);
    }
}
