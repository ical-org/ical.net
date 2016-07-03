using System;
using System.Collections.Generic;

namespace ical.NET.Collections.Interfaces
{
    public interface IGroupedCollection<TGroup, TItem> :
        ICollection<TItem>
        where TItem : class, IGroupedObject<TGroup>
    {
        /// <summary>
        /// Fired after an item is added to the collection.
        /// </summary>
        event EventHandler<ObjectEventArgs<TItem, int>> ItemAdded;

        /// <summary>
        /// Fired after an item is removed from the collection.
        /// </summary>
        event EventHandler<ObjectEventArgs<TItem, int>> ItemRemoved;

        /// <summary>
        /// Removes all items with the matching group from the collection.
        /// </summary>        
        /// <returns>True if the object was removed, false otherwise.</returns>
        bool Remove(TGroup group);

        /// <summary>
        /// Clears all items matching the specified group.
        /// </summary>
        void Clear(TGroup group);
        
        /// <summary>
        /// Returns true if the list contains at least one 
        /// object with a matching group, false otherwise.
        /// </summary>
        bool ContainsKey(TGroup group);

        /// <summary>
        /// Returns the number of objects in the list
        /// with a matching group.
        /// </summary>
        int CountOf(TGroup group);
        
        /// <summary>
        /// Returns a list of objects that
        /// match the specified group.
        /// </summary>
        IEnumerable<TItem> AllOf(TGroup group);

        /// <summary>
        /// Sort the keys of the list.
        /// </summary>
        void SortKeys(IComparer<TGroup> comparer = null);
    }
}
