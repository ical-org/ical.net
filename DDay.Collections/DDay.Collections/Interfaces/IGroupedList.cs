using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.Collections
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
        int IndexOf(TItem obj);

        /// <summary>
        /// Gets the object at the specified index.
        /// </summary>
        TItem this[int index] { get; }
    }
}
