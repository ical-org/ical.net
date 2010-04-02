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
            AddPropertyMapping("ACTION", typeof(AlarmAction));
            AddPropertyMapping("ATTACH", typeof(IList<IAttachment>));
            AddPropertyMapping("ATTENDEE", ResolveAttendeeProperty);
            AddPropertyMapping("CATEGORIES", typeof(IList<string>));
            AddPropertyMapping("COMMENT", typeof(IList<string>));
            AddPropertyMapping("COMPLETED", typeof(IDateTime));
            AddPropertyMapping("CONTACT", typeof(IList<string>));
            AddPropertyMapping("CREATED", typeof(IDateTime));
            AddPropertyMapping("DTEND", typeof(IDateTime));
            AddPropertyMapping("DTSTAMP", typeof(IDateTime));
            AddPropertyMapping("DTSTART", typeof(IDateTime));
            AddPropertyMapping("DUE", typeof(IDateTime));
            AddPropertyMapping("DURATION", typeof(TimeSpan));
            AddPropertyMapping("EXDATE", typeof(IList<IPeriodList>));
            AddPropertyMapping("EXRULE", typeof(IList<IRecurrencePattern>));
            AddPropertyMapping("GEO", typeof(IGeographicLocation));
            AddPropertyMapping("LAST-MODIFIED", typeof(IDateTime));
            AddPropertyMapping("ORGANIZER", typeof(IOrganizer));
            AddPropertyMapping("PERCENT-COMPLETE", typeof(int));
            AddPropertyMapping("PRIORITY", typeof(int));
            AddPropertyMapping("RDATE", typeof(IList<IPeriodList>));
            AddPropertyMapping("RECURRENCE-ID", typeof(IDateTime));
            AddPropertyMapping("RELATED-TO", typeof(IList<string>));
            AddPropertyMapping("REQUEST-STATUS", typeof(IList<IRequestStatus>));
            AddPropertyMapping("REPEAT", typeof(int));
            AddPropertyMapping("RESOURCES", typeof(IList<string>));
            AddPropertyMapping("RRULE", typeof(IList<IRecurrencePattern>));
            AddPropertyMapping("SEQUENCE", typeof(int));
            AddPropertyMapping("STATUS", ResolveStatusProperty);
            AddPropertyMapping("TRANSP", typeof(ITransparency));
            AddPropertyMapping("TRIGGER", typeof(ITrigger));
            AddPropertyMapping("TZNAME", typeof(IList<string>));
            AddPropertyMapping("TZOFFSETFROM", typeof(IUTCOffset));
            AddPropertyMapping("TZOFFSETTO", typeof(IUTCOffset));
            AddPropertyMapping("TZURL", typeof(Uri));
            AddPropertyMapping("URL", typeof(Uri));
        }

        #endregion

        #region Event Handlers

        protected Type ResolveAttendeeProperty(object context)
        {
            ICalendarObject obj = context as ICalendarObject;
            if (obj != null)
            {
                if (obj.Parent is IAlarm)
                    return typeof(IAttendee);
                else
                    return typeof(IList<IAttendee>);
            }

            return null;
        }

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
        
        virtual public Type GetPropertyMapping(object obj)
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
