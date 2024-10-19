using System;

namespace Ical.Net
{
    // This class should be declared as abstract
    public class CalendarObjectBase : ICopyable, ILoadable
    {
        private bool _mIsLoaded = true;

        /// <summary>
        /// Makes a deep copy of the <see cref="ICopyable"/> source
        /// to the current object. This method must be overridden in a derived class.
        /// </summary>
        public virtual void CopyFrom(ICopyable c)
        {
            throw new NotImplementedException("Must be implemented in a derived class.");
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="T"/> object.
        /// </summary>
        /// <returns>The copy of the <see cref="T"/> object.</returns>
        public virtual T Copy<T>()
        {
            var type = GetType();
            var obj = Activator.CreateInstance(type) as ICopyable;
            
            if (obj is not T objOfT) return default(T);

            obj.CopyFrom(this);
            return objOfT;
        }

        public virtual bool IsLoaded => _mIsLoaded;

        public event EventHandler Loaded;

        public virtual void OnLoaded()
        {
            _mIsLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}