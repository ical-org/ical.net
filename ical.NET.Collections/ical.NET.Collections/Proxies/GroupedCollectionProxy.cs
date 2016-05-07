using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ical.NET.Collections.Interfaces;
using ical.NET.Collections.Interfaces.Proxies;

namespace ical.NET.Collections.Proxies
{
    /// <summary>
    /// A proxy for a keyed list.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class GroupedCollectionProxy<TGroup, TOriginal, TNew> :
        IGroupedCollectionProxy<TGroup, TOriginal, TNew>
        where TOriginal : class, IGroupedObject<TGroup>
        where TNew : class, TOriginal
    {
        #region Private Fields

        IGroupedCollection<TGroup, TOriginal> _realObject;
        Func<TNew, bool> _predicate;

        #endregion

        #region Constructors

        public GroupedCollectionProxy(IGroupedCollection<TGroup, TOriginal> realObject, Func<TNew, bool> predicate = null)
        {
            _predicate = predicate ?? new Func<TNew, bool>(o => true);
            SetProxiedObject(realObject);

            _realObject.ItemAdded += new EventHandler<ObjectEventArgs<TOriginal, int>>(_RealObject_ItemAdded);
            _realObject.ItemRemoved += new EventHandler<ObjectEventArgs<TOriginal, int>>(_RealObject_ItemRemoved);
        }

        #endregion

        #region Event Handlers

        void _RealObject_ItemRemoved(object sender, ObjectEventArgs<TOriginal, int> e)
        {
            if (e.First is TNew)
                OnItemRemoved((TNew)e.First, e.Second);
        }

        void _RealObject_ItemAdded(object sender, ObjectEventArgs<TOriginal, int> e)
        {
            if (e.First is TNew)
                OnItemAdded((TNew)e.First, e.Second);
        }

        #endregion

        #region IGroupedCollection Members

        public virtual event EventHandler<ObjectEventArgs<TNew, int>> ItemAdded;
        public virtual event EventHandler<ObjectEventArgs<TNew, int>> ItemRemoved;

        protected void OnItemAdded(TNew item, int index)
        {
            if (ItemAdded != null)
                ItemAdded(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        protected void OnItemRemoved(TNew item, int index)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ObjectEventArgs<TNew, int>(item, index));
        }

        public virtual bool Remove(TGroup group)
        {
            return _realObject.Remove(group);
        }

        public virtual void Clear(TGroup group)
        {
            _realObject.Clear(group);
        }

        public virtual bool ContainsKey(TGroup group)
        {
            return _realObject.ContainsKey(group);            
        }

        public virtual int CountOf(TGroup group)
        {
            return _realObject.Count(g => g.Group.GetType() == typeof (TGroup));
        }

        public virtual IEnumerable<TNew> AllOf(TGroup group)
        {
            return _realObject
                .AllOf(group)
                .OfType<TNew>()
                .Where(_predicate);
        }
        
        public virtual void SortKeys(IComparer<TGroup> comparer = null)
        {
            _realObject.SortKeys(comparer);
        }

        public virtual void Add(TNew item)
        {
            _realObject.Add(item);
        }

        public virtual void Clear()
        {
            // Only clear items of this type
            // that match the predicate.

            var items = _realObject
                .OfType<TNew>()
                .ToArray();

            foreach (var item in items)
            {
                _realObject.Remove(item);
            }
        }

        public virtual bool Contains(TNew item)
        {
            return _realObject.Contains(item);
        }

        public virtual void CopyTo(TNew[] array, int arrayIndex)
        {
            var i = 0;
            foreach (var item in this)
            {
                array[arrayIndex + (i++)] = item;
            }
        }

        public virtual int Count
        {
            get 
            { 
                return _realObject
                    .OfType<TNew>()
                    .Count(); 
            }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(TNew item)
        {
            return _realObject.Remove(item);
        }

        public virtual IEnumerator<TNew> GetEnumerator()
        {
            return _realObject
                .OfType<TNew>()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _realObject
                .OfType<TNew>()
                .GetEnumerator();
        }

        #endregion

        #region IGroupedCollectionProxy Members

        public IGroupedCollection<TGroup, TOriginal> RealObject
        {
            get { return _realObject; }
        }

        public virtual void SetProxiedObject(IGroupedCollection<TGroup, TOriginal> realObject)
        {
            _realObject = realObject;
        }

        #endregion
    }
}
