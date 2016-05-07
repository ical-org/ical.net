using System;

namespace ical.NET.Collections.Interfaces
{
    public interface IGroupedObject<TGroup>
    {
        event EventHandler<ObjectEventArgs<TGroup, TGroup>> GroupChanged;
        TGroup Group { get; set; }
    }
}
