using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IPeriodList : IEncodableDataType, IEnumerable<Period>
    {
        string TzId { get; }
        Period this[int index] { get; }
        void Add(IDateTime dt);
        void Add(Period item);
        IEnumerator<Period> GetEnumerator();
        int Count { get; }
    }
}