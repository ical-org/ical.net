using System;

namespace DDay.Collections
{
    public interface IGroupedObject<TGroup>
    {
        event EventHandler<ObjectEventArgs<TGroup, TGroup>> GroupChanged;
        TGroup Group { get; set; }
    }
}
