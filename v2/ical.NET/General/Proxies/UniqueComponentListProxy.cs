using System;
using System.Collections.Generic;
using System.Linq;
using ical.net.collections.Interfaces;
using ical.net.Interfaces.Components;
using ical.net.Interfaces.General;

namespace ical.net.General.Proxies
{
    public class UniqueComponentListProxy<TComponentType> : CalendarObjectListProxy<TComponentType>, IUniqueComponentList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        private readonly Dictionary<string, TComponentType> _lookup;

        public UniqueComponentListProxy(IGroupedCollection<string, ICalendarObject> children) : base(children)
        {
            _lookup = new Dictionary<string, TComponentType>();
        }

        private TComponentType Search(string uid)
        {
            TComponentType componentType;
            if (_lookup.TryGetValue(uid, out componentType))
            {
                return componentType;
            }

            var item = this.FirstOrDefault(c => string.Equals(c.Uid, uid, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                return default(TComponentType);
            }

            _lookup[uid] = item;
            return item;
        }

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
    }
}