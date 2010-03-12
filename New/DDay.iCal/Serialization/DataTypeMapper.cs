using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public delegate Type TypeResolverDelegate(object context);

    public class DataTypeMapper :
        IDataTypeMapper
    {
        private struct Mapping
        {
            public Type ObjectType { get; set; }
            public TypeResolverDelegate Resolver { get; set; }
        }

        #region Private Fields

        IDictionary<string, Mapping> _PropertyMap = new Dictionary<string, Mapping>();

        #endregion

        #region Constructors

        public DataTypeMapper()
        {
            AddPropertyMapping("ATTACH", typeof(IList<IAttachment>));
            AddPropertyMapping("ATTENDEE", typeof(IList<IAttendee>));
            AddPropertyMapping("CATEGORIES", typeof(IList<string>));
            AddPropertyMapping("COMMENT", typeof(IList<string>));
            AddPropertyMapping("COMPLETED", typeof(iCalDateTime));
            AddPropertyMapping("CONTACT", typeof(IList<string>));
            AddPropertyMapping("CREATED", typeof(iCalDateTime));
            AddPropertyMapping("DTEND", typeof(iCalDateTime));
            AddPropertyMapping("DTSTAMP", typeof(iCalDateTime));
            AddPropertyMapping("DTSTART", typeof(iCalDateTime));
            AddPropertyMapping("DUE", typeof(iCalDateTime));
            AddPropertyMapping("DURATION", typeof(TimeSpan));
            AddPropertyMapping("EXDATE", typeof(IList<IRecurrenceDate>));
            AddPropertyMapping("EXRULE", typeof(IList<IRecurrencePattern>));
            AddPropertyMapping("GEO", typeof(IGeographicLocation));
            AddPropertyMapping("LAST-MODIFIED", typeof(iCalDateTime));
            AddPropertyMapping("ORGANIZER", typeof(IOrganizer));
            AddPropertyMapping("PERCENT-COMPLETE", typeof(int));
            AddPropertyMapping("PRIORITY", typeof(int));
            AddPropertyMapping("RDATE", typeof(IList<IRecurrenceDate>));            
            AddPropertyMapping("RECURRENCE-ID", typeof(iCalDateTime));
            AddPropertyMapping("RELATED-TO", typeof(IList<string>));
            AddPropertyMapping("REQUEST-STATUS", typeof(IList<IRequestStatus>));
            AddPropertyMapping("RESOURCES", typeof(IList<string>));
            AddPropertyMapping("RRULE", typeof(IList<IRecurrencePattern>));
            AddPropertyMapping("SEQUENCE", typeof(int));
            AddPropertyMapping("STATUS", ResolveStatusProperty);
            AddPropertyMapping("TRANSP", typeof(ITransparency));
            AddPropertyMapping("TZNAME", typeof(IList<string>));
            AddPropertyMapping("TZOFFSETFROM", typeof(IUTCOffset));
            AddPropertyMapping("TZOFFSETTO", typeof(IUTCOffset));
            AddPropertyMapping("TZURL", typeof(Uri));
            AddPropertyMapping("URL", typeof(Uri));
        }

        #endregion

        #region Event Handlers

        protected Type ResolveStatusProperty(object context)
        {
            ICalendarObject obj = context as ICalendarObject;
            if (obj != null)
            {
                if (obj.Parent is IEvent)
                    return typeof(EventStatus);
                else if (obj.Parent is ITodo)
                    return typeof(TodoStatus);

                // FIXME: return other status types here
            }

            return null;
        }

        #endregion

        #region IDefaultTypeMapper Members

        public void AddPropertyMapping(string name, Type objectType)
        {
            if (name != null && objectType != null)
            {
                Mapping m = new Mapping();
                m.ObjectType = objectType;

                _PropertyMap[name.ToUpper()] = m;
            }
        }

        public void AddPropertyMapping(string name, TypeResolverDelegate resolver)
        {
            if (name != null && resolver != null)
            {
                Mapping m = new Mapping();
                m.Resolver = resolver;

                _PropertyMap[name.ToUpper()] = m;
            }
        }

        public void RemovePropertyMapping(string name)
        {
            if (name != null &&
                _PropertyMap.ContainsKey(name.ToUpper()))
                _PropertyMap.Remove(name.ToUpper());
        }
        
        virtual public Type Get(object obj)
        {
            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null && p.Name != null)
            {
                string name = p.Name.ToUpper();
                if (_PropertyMap.ContainsKey(name))
                {
                    Mapping m = _PropertyMap[name];
                    if (m.Resolver != null)
                        return m.Resolver(p);
                    else
                        return m.ObjectType;
                }
            }
            return null;
        }

        #endregion
    }
}
