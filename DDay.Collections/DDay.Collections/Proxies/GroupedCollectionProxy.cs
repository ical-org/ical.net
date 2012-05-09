using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections;

namespace DDay.Collections
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

        IGroupedCollection<TGroup, TOriginal> _RealObject;
        Func<TNew, bool> _Predicate;

        #endregion

        #region Constructors

        public GroupedCollectionProxy(IGroupedCollection<TGroup, TOriginal> realObject, Func<TNew, bool> predicate = null)
        {
            _Predicate = predicate ?? new Func<TNew, bool>(o => true);
            SetProxiedObject(realObject);

            _RealObject.ItemAdded += new EventHandler<ObjectEventArgs<TOriginal, int>>(_RealObject_ItemAdded);
            _RealObject.ItemRemoved += new EventHandler<ObjectEventArgs<TOriginal, int>>(_RealObject_ItemRemoved);
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

        virtual public event EventHandler<ObjectEventArgs<TNew, int>> ItemAdded;
        virtual public event EventHandler<ObjectEventArgs<TNew, int>> ItemRemoved;

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

        virtual public bool Remove(TGroup group)
        {
            return _RealObject.Remove(group);
        }

        virtual public void Clear(TGroup group)
        {
            _RealObject.Clear(group);
        }

        virtual public bool ContainsKey(TGroup group)
        {
            return _RealObject.ContainsKey(group);            
        }

        virtual public int CountOf(TGroup group)
        {
            return _RealObject
                .AllOf(group)
                .OfType<TNew>()
                .Where(_Predicate)
                .Count();            
        }

        virtual public IEnumerable<TNew> AllOf(TGroup group)
        {
            return _RealObject
                .AllOf(group)
                .OfType<TNew>()
                .Where(_Predicate);
        }
        
        virtual public void SortKeys(IComparer<TGroup> comparer = null)
        {
            _RealObject.SortKeys(comparer);
        }

        virtual public void Add(TNew item)
        {
            _RealObject.Add(item);
        }

        virtual public void Clear()
        {
            // Only clear items of this type
            // that match the predicate.

            var items = _RealObject
                .OfType<TNew>()
                .Where(_Predicate)
                .ToArray();

            foreach (TNew item in items)
            {
                _RealObject.Remove(item);
            }
        }

        virtual public bool Contains(TNew item)
        {
            return _RealObject.Contains(item);
        }

        virtual public void CopyTo(TNew[] array, int arrayIndex)
        {
            int i = 0;
            foreach (TNew item in this)
            {
                array[arrayIndex + (i++)] = item;
            }
        }

        virtual public int Count
        {
            get 
            { 
                return _RealObject
                    .OfType<TNew>()
                    .Where(_Predicate)
                    .Count(); 
            }
        }

        virtual public bool IsReadOnly
        {
            get { return false; }
        }

        virtual public bool Remove(TNew item)
        {
            return _RealObject.Remove(item);
        }

        virtual public IEnumerator<TNew> GetEnumerator()
        {
            return _RealObject
                .OfType<TNew>()
                .Where(_Predicate)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _RealObject
                .OfType<TNew>()
                .Where(_Predicate)
                .GetEnumerator();
        }

        #endregion

        #region IGroupedCollectionProxy Members

        public IGroupedCollection<TGroup, TOriginal> RealObject
        {
            get { return _RealObject; }
        }

        virtual public void SetProxiedObject(IGroupedCollection<TGroup, TOriginal> realObject)
        {
            _RealObject = realObject;
        }

        #endregion
    }
}
