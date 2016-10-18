﻿using System.Collections.Generic;
using ical.net.DataTypes;

namespace ical.net.Interfaces.DataTypes
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