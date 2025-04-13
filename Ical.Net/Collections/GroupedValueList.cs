//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.Collections.Interfaces;
using Ical.Net.Collections.Proxies;

namespace Ical.Net.Collections;

public class GroupedValueList<TGroup, TInterface, TItem, TValueType> :
    GroupedList<TGroup, TInterface>
    where TGroup : notnull
    where TInterface : class, IGroupedObject<TGroup>, IValueObject<TValueType>
    where TItem : new()
{
    public virtual void Set(TGroup group, TValueType value)
    {
        Set(group, new[] { value });
    }

    public virtual void Set(TGroup group, IEnumerable<TValueType> values)
    {
        if (ContainsKey(group))
        {
            AllOf(group)?.FirstOrDefault()?.SetValue(values);
            return;
        }

        // No matching item was found, add a new item to the list
        if (Activator.CreateInstance(typeof(TItem), true) is TInterface obj)
        {
            obj.Group = group;
            obj.SetValue(values);
            Add(obj);
        }
    }

    public virtual TType? Get<TType>(TGroup group)
    {
        var firstItem = AllOf(group).FirstOrDefault();
        if (firstItem?.Values != null)
        {
            return firstItem
                .Values
                .OfType<TType>()
                .FirstOrDefault();
        }
        return default(TType);
    }

    public virtual IList<TType> GetMany<TType>(TGroup group) where TType : class => new GroupedValueListProxy<TGroup, TInterface, TItem, TValueType, TType>(this, group);
}
