using System;

namespace Ical.Net
{
    /// <summary> Base Class for all Calendar Objects </summary>
    /// <inheritdoc cref="ICopyable"/>
    /// <inheritdoc cref="ILoadable"/>
    public class CalendarObjectBase : ICopyable, ILoadable
    {
        public CalendarObjectBase()
        {
            IsLoaded = true;
        }

        public virtual void CopyFrom(ICopyable c) {}

        public T Copy<T>()
        {
            var type = GetType();
            var obj = Activator.CreateInstance(type) as ICopyable;

            // Duplicate our values
            if (obj is T obj1)
            {
                obj.CopyFrom(this);
                return obj1;
            }
            return default;
        }

        public bool IsLoaded { get; private set; }

        public event EventHandler? Loaded;

        public void OnLoaded()
        {
            IsLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}