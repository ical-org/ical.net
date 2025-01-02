//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;

namespace Ical.Net;

// This class should be declared as abstract
public class CalendarObjectBase : ICopyable, ILoadable
{
    private bool _mIsLoaded = true;

    /// <summary>
    /// Makes a deep copy of the <see cref="ICopyable"/> source
    /// to the current object. This method must be overridden in a derived class.
    /// </summary>
    public virtual void CopyFrom(ICopyable obj)
    {
        throw new NotImplementedException("Must be implemented in a derived class.");
    }

    /// <summary>
    /// Creates a deep copy of the <see cref="T"/> object.
    /// </summary>
    /// <returns>The copy of the <see cref="T"/> object.</returns>
    public virtual T? Copy<T>()
    {
        if (Activator.CreateInstance(GetType(), true) is not T objOfT) return default;

        (objOfT as ICopyable)?.CopyFrom(this);
        return objOfT;
    }

    public virtual bool IsLoaded => _mIsLoaded;

    public event EventHandler? Loaded;

    public virtual void OnLoaded()
    {
        _mIsLoaded = true;
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
