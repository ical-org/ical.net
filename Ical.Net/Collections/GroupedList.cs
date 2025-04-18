//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Collections;

/// <summary>
/// A list of objects that are keyed.
/// </summary>
public class GroupedList<TGroup, TItem> :
    IGroupedList<TGroup, TItem>
    where TGroup : notnull
    where TItem : class, IGroupedObject<TGroup>
{
    private readonly List<IMultiLinkedList<TItem>> _lists = new();
    private readonly Dictionary<TGroup, IMultiLinkedList<TItem>> _dictionary = new();

    private IMultiLinkedList<TItem> EnsureList(TGroup group)
    {
        if (_dictionary.TryGetValue(group, out var existingList))
        {
            return existingList;
        }

        var list = new MultiLinkedList<TItem>();
        _dictionary[group] = list;
        _lists.Add(list);
        return list;
    }

    private IMultiLinkedList<TItem>? ListForIndex(int index, out int relativeIndex)
    {
        var list = _lists.FirstOrDefault(l => l.StartIndex <= index && l.ExclusiveEnd > index);
        if (list != null)
        {
            relativeIndex = index - list.StartIndex;
            return list!;
        }

        relativeIndex = -1;
        return null;
    }

    public event EventHandler<ObjectEventArgs<TItem, int>>? ItemAdded;

    protected void OnItemAdded(TItem obj, int index)
        => ItemAdded?.Invoke(this, new ObjectEventArgs<TItem, int>(obj, index));

    public virtual void Add(TItem? item)
    {
        if (item == null)
        {
            return;
        }

        var group = item.Group;
        var list = EnsureList(group);

        var index = list.Count;
        list.Add(item);
        OnItemAdded(item, list.StartIndex + index);
    }

    public virtual int IndexOf(TItem? obj)
    {
        if (obj == null)
        {
            return -1;
        }
        var group = obj.Group;
        if (!_dictionary.TryGetValue(group, out var list))
        {
            return -1;
        }

        var index = list.IndexOf(obj);
        return index >= 0 ? list.StartIndex + index : -1;
    }

    public virtual void Clear(TGroup group)
    {
        if (_dictionary.TryGetValue(group, out var list))
        {
            list.Clear();
        }
    }

    public virtual void Clear()
    {
        _dictionary.Clear();
        _lists.Clear();
    }

    public virtual bool ContainsKey(TGroup group) => _dictionary.ContainsKey(group);

    public virtual int Count => _lists.Sum(list => list.Count);

    public virtual int CountOf(TGroup group) =>
        _dictionary.TryGetValue(group, out var list) ? list.Count : 0;

    public virtual IEnumerable<TItem> Values() => _dictionary.Values.SelectMany(i => i);

    public virtual IEnumerable<TItem> AllOf(TGroup group) =>
        _dictionary.TryGetValue(group, out var list) ? list : Array.Empty<TItem>();

    public virtual bool Remove(TItem? item)
    {
        if (item == null)
        {
            return false;
        }

        var group = item.Group;

        if (!_dictionary.TryGetValue(group, out var list))
        {
            return false;
        }

        var index = list.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        list.RemoveAt(index);
        return true;
    }

    public virtual bool Remove(TGroup group)
    {
        if (!_dictionary.TryGetValue(group, out var list))
        {
            return false;
        }

        for (var i = list.Count - 1; i >= 0; i--)
        {
            list.RemoveAt(i);
        }

        return true;
    }

    public virtual bool Contains(TItem? item)
    {
        if (item == null)
        {
            return false;
        }

        var group = item.Group;
        return _dictionary.TryGetValue(group, out var list) && list.Contains(item);
    }

    public virtual void CopyTo(TItem?[] array, int arrayIndex)
        => _dictionary.SelectMany(kvp => kvp.Value).ToArray().CopyTo(array, arrayIndex);

    public virtual bool IsReadOnly => false;

    public virtual void Insert(int index, TItem? item)
    {
        var list = ListForIndex(index, out var relativeIndex);
        if (list == null || item == null)
        {
            return;
        }

        list.Insert(relativeIndex, item);
        OnItemAdded(item, index);
    }

    public virtual void RemoveAt(int index)
    {
        var list = ListForIndex(index, out var relativeIndex);

        list?.RemoveAt(relativeIndex);
    }

    /// <summary>
    /// The indexer for the IList interface. This is the virtual, primary implementation of the indexer.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>
    /// Returns the item at the specified index, or null if the index is invalid.
    /// </returns>
    public virtual TItem? this[int index]
    {
        get
        {
            var list = ListForIndex(index, out var relativeIndex);
            return list?[relativeIndex];
        }
        set
        {
            var list = ListForIndex(index, out var relativeIndex);
            if (list == null || value == null) return;
            list[relativeIndex] = value;
            OnItemAdded(value, index);
        }
    }

    /// <summary>
    /// This is an explicit non-nullable implementation of the indexer for the IList interface.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    TItem IList<TItem>.this[int index]
    {
        get => this[index] ?? throw new ArgumentOutOfRangeException(nameof(index));
        set => this[index] = value;
    }

    public IEnumerator<TItem> GetEnumerator() => new GroupedListEnumerator<TItem>(_lists);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
