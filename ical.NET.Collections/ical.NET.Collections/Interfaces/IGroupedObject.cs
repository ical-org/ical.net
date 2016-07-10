using System;

namespace ical.NET.Collections.Interfaces
{
    public interface IGroupedObject<TGroup>
    {
        TGroup Group { get; set; }
    }
}
