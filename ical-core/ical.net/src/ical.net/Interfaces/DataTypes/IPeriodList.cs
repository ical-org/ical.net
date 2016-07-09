using System.Collections.Generic;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IPeriodList : IEncodableDataType, IList<IPeriod>
    {
        string TzId { get; set; }

        new IPeriod this[int index] { get; set; }
        void Add(IDateTime dt);
        void Remove(IDateTime dt);
    }
}