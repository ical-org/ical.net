using System;

namespace Ical.Net
{
    public class CalendarObjectBase : ICopyable, ILoadable
    {
        public CalendarObjectBase()
        {
            IsLoaded = true;
        }

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        public virtual void CopyFrom(ICopyable c) {}

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
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

        public event EventHandler Loaded;

        public void OnLoaded()
        {
            IsLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}