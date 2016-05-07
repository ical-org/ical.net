using System.Collections.Generic;
using System.Linq;
using ical.NET.Collections;
using ical.NET.Collections.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General.Proxies
{
    public class UniqueComponentListProxy<TComponentType> : CalendarObjectListProxy<TComponentType>, IUniqueComponentList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        Dictionary<string, TComponentType> _lookup;

        #region Constructors

        public UniqueComponentListProxy(IGroupedCollection<string, ICalendarObject> children) : base(children)
        {
            _lookup = new Dictionary<string, TComponentType>();

            children.ItemAdded += children_ItemAdded;
            children.ItemRemoved += children_ItemRemoved;
        }

        #endregion

        #region Private Methods

        TComponentType Search(string uid)
        {
            if (_lookup.ContainsKey(uid))
            {
                return _lookup[uid];
            }

            var item = this.OfType<TComponentType>().Where(c => string.Equals(c.Uid, uid)).FirstOrDefault();

            if (item != null)
            {
                _lookup[uid] = item;
                return item;
            }
            return default(TComponentType);
        }

        #endregion

        #region UniqueComponentListProxy Members

        public virtual TComponentType this[string uid]
        {
            get { return Search(uid); }
            set
            {
                // Find the item matching the UID
                var item = Search(uid);

                if (item != null)
                {
                    Remove(item);
                }

                if (value != null)
                {
                    Add(value);
                }
            }
        }

        #endregion

        #region Event Handlers

        void children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            if (e.First is TComponentType)
            {
                var component = (TComponentType) e.First;
                component.UidChanged += UidChanged;

                if (!string.IsNullOrEmpty(component.Uid))
                {
                    _lookup[component.Uid] = component;
                }
            }
        }

        void children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            if (e.First is TComponentType)
            {
                var component = (TComponentType) e.First;
                component.UidChanged -= UidChanged;

                if (!string.IsNullOrEmpty(component.Uid) && _lookup.ContainsKey(component.Uid))
                {
                    _lookup.Remove(component.Uid);
                }
            }
        }

        void UidChanged(object sender, ObjectEventArgs<string, string> e)
        {
            if (e.First != null && _lookup.ContainsKey(e.First))
            {
                _lookup.Remove(e.First);
            }

            if (e.Second != null)
            {
                _lookup[e.Second] = (TComponentType) sender;
            }
        }

        #endregion
    }
}