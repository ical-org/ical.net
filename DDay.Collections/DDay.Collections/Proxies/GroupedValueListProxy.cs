using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections;

namespace DDay.Collections
{
    /// <summary>
    /// A proxy for a keyed list.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class GroupedValueListProxy<TGroup, TInterface, TItem, TOriginalValue, TNewValue> :
        IGroupedValueListProxy<TInterface, TNewValue>
        where TInterface : class, IGroupedObject<TGroup>, IValueObject<TOriginalValue>
        where TItem : new()        
    {
        #region Private Fields

        IGroupedValueList<TGroup, TInterface, TItem, TOriginalValue> _RealObject;
        TGroup _Group;
        TInterface _Container;

        #endregion

        #region Constructors

        public GroupedValueListProxy(IGroupedValueList<TGroup, TInterface, TItem, TOriginalValue> realObject, TGroup group)
        {
            _RealObject = realObject;
            _Group = group;
        }

        #endregion

        #region Private Methods

        TInterface EnsureContainer()
        {
            if (_Container == null)
            {
                // Find an item that matches our group
                _Container = Items.FirstOrDefault();

                // If no item is found, create a new object and add it to the list
                if (object.Equals(_Container, default(TInterface)))
                {
                    var container = new TItem();
                    if (!(container is TInterface))
                        throw new Exception("Could not create a container for the value - the container is not of type " + typeof(TInterface).GetType().Name);
                    _Container = (TInterface)(object)container;
                    _Container.Group = _Group;
                    _RealObject.Add(_Container);
                }
            }
            return _Container;
        }

        void IterateValues(Func<IValueObject<TOriginalValue>, int, int, bool> action)
        {
            int i = 0;
            foreach (var obj in _RealObject)
            {
                // Get the number of items of the target value i this object
                var count = obj.Values != null ? obj.Values.OfType<TNewValue>().Count() : 0;

                // Perform some action on this item
                if (!action(obj, i, count))
                    return;

                i += count;
            }
        }

        IValueObject<TOriginalValue> ObjectForIndex(int index, ref int relativeIndex)
        {
            IValueObject<TOriginalValue> obj = null;
            int retVal = -1;

            IterateValues((o, i, count) =>
                {
                    // Determine if this index is found within this object
                    if (index >= i && index < count)
                    {
                        retVal = index - i;
                        obj = o;
                        return false;
                    }
                    return true;
                });

            relativeIndex = retVal;
            return obj;            
        }

        IEnumerator<TNewValue> GetEnumeratorInternal()
        {
            return Items
                .Where(o => o.ValueCount > 0)
                .SelectMany(o => o.Values.OfType<TNewValue>())
                .GetEnumerator();
        }

        #endregion

        #region IList<TNewValue> Members

        virtual public void Add(TNewValue item)
        {
            // Add the value to the object
            if (item is TOriginalValue)
            {
                var value = (TOriginalValue)(object)item;
                EnsureContainer().AddValue(value);
            }
        }

        virtual public void Clear()
        {
            var items = Items.Where(o => o.Values != null);

            foreach (TInterface original in items)
            {
                // Clear all values from each matching object
                original.SetValue(default(TOriginalValue));
            }
        }

        virtual public bool Contains(TNewValue item)
        {
            if (item is TOriginalValue)
            {
                return Items
                    .Where(o => o.ContainsValue((TOriginalValue)(object)item))
                    .Any();
            }
            return false;
        }

        virtual public void CopyTo(TNewValue[] array, int arrayIndex)
        {
            Items                
                .Where(o => o.Values != null)
                .SelectMany(o => o.Values)
                .ToArray()
                .CopyTo(array, arrayIndex);
        }
        
        virtual public int Count
        {
            get
            {
                return Items.Sum(o => o.ValueCount);
            }
        }

        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        virtual public bool Remove(TNewValue item)
        {
            if (item is TOriginalValue)
            {
                var value = (TOriginalValue)(object)item;

                var container = Items
                    .Where(o => o.ContainsValue(value))
                    .FirstOrDefault();

                if (container != null)
                {
                    container.RemoveValue(value);
                    return true;
                }
            }
            return false;
        }

        virtual public IEnumerator<TNewValue> GetEnumerator()
        {
            return GetEnumeratorInternal();        
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        virtual public int IndexOf(TNewValue item)
        {
            int index = -1;

            if (item is TOriginalValue)
            {
                var value = (TOriginalValue)(object)item;
                IterateValues((o, i, count) =>
                    {
                        if (o.Values != null && o.Values.Contains(value))
                        {
                            var list = o.Values.ToList();
                            index = i + list.IndexOf(value);
                            return false;
                        }
                        return true;
                    });
            }

            return index;
        }

        virtual public void Insert(int index, TNewValue item)
        {
            IterateValues((o, i, count) =>
                {
                    var value = (TOriginalValue)(object)item;

                    // Determine if this index is found within this object
                    if (index >= i && index < count)
                    {
                        // Convert the items to a list
                        var items = o.Values.ToList();
                        // Insert the item at the relative index within the list
                        items.Insert(index - i, value);
                        // Set the new list
                        o.SetValue(items);
                        return false;
                    }
                    return true;
                });
        }

        virtual public void RemoveAt(int index)
        {
            IterateValues((o, i, count) =>
            {
                // Determine if this index is found within this object
                if (index >= i && index < count)
                {
                    // Convert the items to a list
                    var items = o.Values.ToList();
                    // Remove the item at the relative index within the list
                    items.RemoveAt(index - i);
                    // Set the new list
                    o.SetValue(items);
                    return false;
                }
                return true;
            });
        }

        virtual public TNewValue this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                {
                    return this
                        .Skip(index)
                        .FirstOrDefault();
                }
                return default(TNewValue);
            }
            set
            {
                if (index >= 0 && index < Count)
                {   
                    if (!object.Equals(value, default(TNewValue)))
                    {
                        Insert(index, value);
                        index++;
                    }
                    RemoveAt(index);
                }
            }
        }

        #endregion

        #region IGroupedValueListProxy Members

        virtual public IEnumerable<TInterface> Items
        {
            get
            {
                if (_Group != null)
                    return _RealObject.AllOf(_Group);
                else
                    return _RealObject;
            }
        }

        #endregion 
    }
}
