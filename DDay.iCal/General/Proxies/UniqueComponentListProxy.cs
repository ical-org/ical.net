using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.Collections;

namespace DDay.iCal
{
    public class UniqueComponentListProxy<TComponentType> :
        CalendarObjectListProxy<TComponentType>,
        IUniqueComponentList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        Dictionary<string, TComponentType> _Lookup;

        #region Constructors

        public UniqueComponentListProxy(IGroupedCollection<string, ICalendarObject> children) : base(children)
        {
            _Lookup = new Dictionary<string, TComponentType>();

            children.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarObject, int>>(children_ItemAdded);
            children.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarObject,int>>(children_ItemRemoved);
        }
        
        #endregion

        #region Private Methods

        TComponentType Search(string uid)
        {
            if (_Lookup.ContainsKey(uid))
            {
                return _Lookup[uid];
            }

            var item = this
                .OfType<TComponentType>()
                .Where(c => string.Equals(c.UID, uid))
                .FirstOrDefault();

            if (item != null)
            {
                _Lookup[uid] = item;
                return item;
            }
            return default(TComponentType);
        }

        #endregion
       
        #region UniqueComponentListProxy Members

        virtual public TComponentType this[string uid]
        {
            get
            {
                return Search(uid);
            }
            set
            {
                // Find the item matching the UID
                var item = Search(uid);

                if (item != null)
                    Remove(item);
                
                if (value != null)
                    Add(value);
            }
        }

        #endregion

        #region Event Handlers

        void children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            if (e.First is TComponentType)
            {
                TComponentType component = (TComponentType)e.First;
                component.UIDChanged += UIDChanged;

                if (!string.IsNullOrEmpty(component.UID))
                    _Lookup[component.UID] = component;
            }
        }

        void children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            if (e.First is TComponentType)
            {
                TComponentType component = (TComponentType)e.First;
                component.UIDChanged -= UIDChanged;

                if (!string.IsNullOrEmpty(component.UID) &&
                    _Lookup.ContainsKey(component.UID))
                {
                    _Lookup.Remove(component.UID);
                }
            }   
        }

        void UIDChanged(object sender, ObjectEventArgs<string, string> e)
        {
            if (e.First != null &&
                _Lookup.ContainsKey(e.First))
            {
                _Lookup.Remove(e.First);
            }

            if (e.Second != null)
            {
                _Lookup[e.Second] = (TComponentType)sender;
            }
        }

        #endregion
    }
}
