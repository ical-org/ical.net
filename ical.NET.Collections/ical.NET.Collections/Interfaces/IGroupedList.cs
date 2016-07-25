using System.Collections.Generic;

namespace ical.Net.Collections.Interfaces
{
    public interface IGroupedList<TGroup, TItem> :
        IGroupedCollection<TGroup, TItem>,
        IList<TItem>
        where TItem : class, IGroupedObject<TGroup>
    {
        /// <summary>
        /// Returns the index of the given item
        /// within the list, or -1 if the item
        /// is not found in the list.
        /// </summary>
        new int IndexOf(TItem obj);

        /// <summary>
        /// Gets the object at the specified index.
        /// </summary>
        new TItem this[int index] { get; }
    }
}
