using System.Collections.Generic;

namespace DDay.iCal
{
    public interface IPeriodList :
        IEncodableDataType,
        IList<IPeriod>
    {
        string TZID { get; set; }

        IPeriod this[int index] { get; set; }
        void Add(IDateTime dt);
        void Remove(IDateTime dt);
    }
}
