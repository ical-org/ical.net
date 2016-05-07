using System;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{

    [Serializable]

    public class CalendarObjectBase :
        ICopyable,        
        ILoadable
    {
        #region Private Fields

        private bool _mIsLoaded;

        #endregion

        #region Constructors

        public CalendarObjectBase()
        {
            // Objects that are loaded using a normal constructor
            // are "Loaded" by default.  Objects that are being
            // deserialized do not use the constructor.
            _mIsLoaded = true;
        }

        #endregion

        #region ICopyable Members

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        public virtual void CopyFrom(ICopyable c)
        {
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
        public virtual T Copy<T>()
        {
            ICopyable obj = null;
            var type = GetType();
            obj = Activator.CreateInstance(type) as ICopyable;

            // Duplicate our values
            if (obj is T)
            {
                obj.CopyFrom(this);
                return (T)obj;
            }
            return default(T);
        }

        #endregion        

        #region ILoadable Members

        public virtual bool IsLoaded => _mIsLoaded;

        [field:NonSerialized]
        public event EventHandler Loaded;

        public virtual void OnLoaded()
        {
            _mIsLoaded = true;
            if (Loaded != null)
                Loaded(this, EventArgs.Empty);
        }

        #endregion
    }
}
