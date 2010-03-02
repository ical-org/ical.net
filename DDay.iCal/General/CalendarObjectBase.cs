using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class CalendarObjectBase :
        ICopyable,
        IMergeable,
        ILoadable
    {
        #region Private Fields

        private bool m_IsLoaded = false;

        #endregion

        #region ICopyable Members

        /// <summary>
        /// Copies values from the target object to the
        /// current object.
        /// </summary>
        virtual public void CopyFrom(ICopyable c)
        {            
        }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns>The copy of the object.</returns>
        virtual public T Copy<T>()
        {
            ICopyable obj = null;
            Type type = GetType();
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

        #region IMergeable Members

        virtual public void MergeWith(IMergeable obj)
        {
        }

        #endregion

        #region ILoadable Members

        virtual public bool IsLoaded
        {
            get { return m_IsLoaded; }
        }
        
        [field:NonSerialized]
        public event EventHandler Loaded;

        virtual public void OnLoaded()
        {
            m_IsLoaded = true;
            if (Loaded != null)
                Loaded(this, EventArgs.Empty);
        }

        #endregion
    }
}
