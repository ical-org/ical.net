//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Collections.Interfaces;

namespace Ical.Net.Collections.Proxies;

/// <summary>
/// A proxy for a keyed list.
/// </summary>
public class GroupedValueListProxy<TGroup, TInterface, TItem, TOriginalValue, TNewValue> : IList<TNewValue>
    where TInterface : class, IGroupedObject<TGroup>, IValueObject<TOriginalValue>
    where TGroup : notnull
    where TNewValue : class
    where TItem : new()
{
    private readonly GroupedValueList<TGroup, TInterface, TItem, TOriginalValue> _realObject;
    private readonly TGroup _group;
    private TInterface? _container;

    public GroupedValueListProxy(GroupedValueList<TGroup, TInterface, TItem, TOriginalValue> realObject, TGroup group)
    {
        _realObject = realObject;
        _group = group;
    }

    private TInterface EnsureContainer()
    {
        if (_container != null)
        {
            return _container;
        }

        // Find an item that matches our group
        _container = Items.FirstOrDefault();

        // If no item is found, create a new object and add it to the list
        if (_container != null)
        {
            return _container;
        }
        var container = new TItem();
        if (container is not TInterface)
        {
            throw new Exception("Could not create a container for the value - the container is not of type " + typeof(TInterface).Name);
        }

        _container = (TInterface) (object) container;
        _container.Group = _group;
        _realObject.Add(_container);
        return _container;
    }

    private void IterateValues(Func<IValueObject<TOriginalValue>, int, int, bool> action)
    {
        var i = 0;
        foreach (var obj in _realObject)
        {
            // Get the number of items of the target value i this object
            var count = obj.Values.OfType<TNewValue>().Count();

            // Perform some action on this item
            if (!action(obj, i, count))
                return;

            i += count;
        }
    }
    
    private IEnumerator<TNewValue> GetEnumeratorInternal()
    {
        return Items
            .Where(o => o.ValueCount > 0)
            // Values can't be null if ValueCount > 0  
            .SelectMany(o => o.Values!.OfType<TNewValue>())
            .GetEnumerator();
    }

    public virtual void Add(TNewValue item)
    {
        // Add the value to the object
        if (item is TOriginalValue)
        {
            var value = (TOriginalValue) (object) item;
            EnsureContainer().AddValue(value);
        }
    }

    public virtual void Clear()
    {
        foreach (var original in Items)
        {
            // Clear all values from each matching object
            original.SetValue(default(TOriginalValue)!);
        }
    }

    public virtual bool Contains(TNewValue item) => Items.Any(o => o.ContainsValue((TOriginalValue) (object) item));

    public virtual void CopyTo(TNewValue[] array, int arrayIndex)
    {
        Items
            .SelectMany(o => o.Values!)
            .ToArray()
            .CopyTo(array, arrayIndex);
    }

    public virtual int Count => Items.Sum(o => o.ValueCount);

    public virtual bool IsReadOnly => false;

    public virtual bool Remove(TNewValue item)
    {
        if (!(item is TOriginalValue))
        {
            return false;
        }

        var value = (TOriginalValue) (object) item;
        var container = Items.FirstOrDefault(o => o.ContainsValue(value));

        if (container == null)
        {
            return false;
        }

        container.RemoveValue(value);
        return true;
    }

    public virtual IEnumerator<TNewValue> GetEnumerator() => GetEnumeratorInternal();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    public virtual int IndexOf(TNewValue item)
    {
        var index = -1;

        if (item is not TOriginalValue)
        {
            return index;
        }

        var value = (TOriginalValue) (object) item;
        IterateValues((o, i, count) =>
        {
            if (o.Values.Contains(value))
            {
                var list = o.Values.ToList();
                index = i + list.IndexOf(value);
                return false;
            }
            return true;
        });

        return index;
    }

    public virtual void Insert(int index, TNewValue item)
    {
        IterateValues((o, i, count) =>
        {
            var value = (TOriginalValue) (object) item;

            // Determine if this index is found within this object
            if (index < i || index >= count)
            {
                return true;
            }

            // Convert the items to a list
            var items = o.Values!.ToList();
            // Insert the item at the relative index within the list
            items.Insert(index - i, value);
            // Set the new list
            o.SetValue(items);
            return false;
        });
    }

    public virtual void RemoveAt(int index)
    {
        IterateValues((o, i, count) =>
        {
            // Determine if this index is found within this object
            if (index < i || index >= count) return true;
            // Convert the items to a list
            var items = o.Values!.ToList();
            // Remove the item at the relative index within the list
            items.RemoveAt(index - i);
            // Set the new list
            o.SetValue(items);
            return false;
        });
    }

    /// <summary>
    /// The indexer for the IList interface. This is the virtual, primary implementation of the indexer.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// The value at the specified index, or null if the index is out of range.
    /// </returns>
    public virtual TNewValue? this[int index]
    {
        get
        {
            if (index >= 0 && index < Count)
            {
                return Items
                    .SelectMany(i => i.Values!.OfType<TNewValue>())
                    .Skip(index)
                    .FirstOrDefault();
            }
            return null;
        }
        set
        {
            if (index >= 0 && index < Count)
            {
                if (!Equals(value, null))
                {
                    Insert(index, value);
                    index++;
                }
                RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// This is the explicit non-nullable implementation IList interface IList&lt;TNewValue&gt;?.this[int].
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    TNewValue IList<TNewValue>.this[int index]
    {
        get
        {
            var result = this[index];
            if (result == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return result;
        }
        set => this[index] = value;
    }

    public virtual IEnumerable<TInterface> Items
        => _realObject.AllOf(_group);
}
