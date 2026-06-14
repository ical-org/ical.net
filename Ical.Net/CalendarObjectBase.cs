//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;

namespace Ical.Net;

public abstract class CalendarObjectBase : ICopyable
{
    /// <summary>
    /// Makes a deep copy of the <see cref="ICopyable"/> source
    /// to the current object. This method must be overridden in a derived class.
    /// </summary>
    public virtual void CopyFrom(ICopyable obj)
        => throw new NotImplementedException("Must be implemented in a derived class.");

    /// <summary>
    /// Creates a deep copy of the <see cref="T"/> object.
    /// </summary>
    /// <returns>The copy of the <see cref="T"/> object.</returns>
    public virtual T? Copy<T>()
    {
        var type = GetType();
        var obj = Activator.CreateInstance(type) as ICopyable;

        if (obj is not T objOfT) return default(T?);

        obj.CopyFrom(this);
        return objOfT;
    }
}
