using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public delegate Type TypeResolverDelegate(object context);

    public class DataTypeMapper :
        IDataTypeMapper
    {
        private struct PropertyMapping
        {
            public Type ObjectType { get; set; }
            public TypeResolverDelegate Resolver { get; set; }
            public bool AllowsMultipleValuesPerProperty { get; set; }
        }

        #region Private Fields

        IDictionary<string, PropertyMapping> _PropertyMap = new Dictionary<string, PropertyMapping>();

        #endregion

        #region Constructors

        public DataTypeMapper()
        {
            AddPropertyMapping("ACTION", typeof(AlarmAction), false);
            AddPropertyMapping("ATTACH", typeof(IAttachment), false);
            AddPropertyMapping("ATTENDEE", typeof(IAttendee), false);
            AddPropertyMapping("CATEGORIES", typeof(string), true);
            AddPropertyMapping("COMMENT", typeof(string), false);
            AddPropertyMapping("COMPLETED", typeof(IDateTime), false);
            AddPropertyMapping("CONTACT", typeof(string), false);
            AddPropertyMapping("CREATED", typeof(IDateTime), false);
            AddPropertyMapping("DTEND", typeof(IDateTime), false);
            AddPropertyMapping("DTSTAMP", typeof(IDateTime), false);
            AddPropertyMapping("DTSTART", typeof(IDateTime), false);
            AddPropertyMapping("DUE", typeof(IDateTime), false);
            AddPropertyMapping("DURATION", typeof(TimeSpan), false);
            AddPropertyMapping("EXDATE", typeof(IPeriodList), false);
            AddPropertyMapping("EXRULE", typeof(IRecurrencePattern), false);
            AddPropertyMapping("FREEBUSY", typeof(IFreeBusyEntry), true);
            AddPropertyMapping("GEO", typeof(IGeographicLocation), false);
            AddPropertyMapping("LAST-MODIFIED", typeof(IDateTime), false);
            AddPropertyMapping("ORGANIZER", typeof(IOrganizer), false);
            AddPropertyMapping("PERCENT-COMPLETE", typeof(int), false);
            AddPropertyMapping("PRIORITY", typeof(int), false);
            AddPropertyMapping("RDATE", typeof(IPeriodList), false);
            AddPropertyMapping("RECURRENCE-ID", typeof(IDateTime), false);
            AddPropertyMapping("RELATED-TO", typeof(string), false);
            AddPropertyMapping("REQUEST-STATUS", typeof(IRequestStatus), false);
            AddPropertyMapping("REPEAT", typeof(int), false);
            AddPropertyMapping("RESOURCES", typeof(string), true);
            AddPropertyMapping("RRULE", typeof(IRecurrencePattern), false);
            AddPropertyMapping("SEQUENCE", typeof(int), false);
            AddPropertyMapping("STATUS", ResolveStatusProperty, false);
            AddPropertyMapping("TRANSP", typeof(TransparencyType), false);
            AddPropertyMapping("TRIGGER", typeof(ITrigger), false);
            AddPropertyMapping("TZNAME", typeof(string), false);
            AddPropertyMapping("TZOFFSETFROM", typeof(IUTCOffset), false);
            AddPropertyMapping("TZOFFSETTO", typeof(IUTCOffset), false);
            AddPropertyMapping("TZURL", typeof(Uri), false);
            AddPropertyMapping("URL", typeof(Uri), false);
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
                else if (obj.Parent is IJournal)
                    return typeof(JournalStatus);
            }

            return null;
        }

        #endregion

        #region IDefaultTypeMapper Members

        public void AddPropertyMapping(string name, Type objectType, bool allowsMultipleValues)
        {
            if (name != null && objectType != null)
            {
                PropertyMapping m = new PropertyMapping();
                m.ObjectType = objectType;
                m.AllowsMultipleValuesPerProperty = allowsMultipleValues;

                _PropertyMap[name.ToUpper()] = m;
            }
        }

        public void AddPropertyMapping(string name, TypeResolverDelegate resolver, bool allowsMultipleValues)
        {
            if (name != null && resolver != null)
            {
                PropertyMapping m = new PropertyMapping();
                m.Resolver = resolver;
                m.AllowsMultipleValuesPerProperty = allowsMultipleValues;

                _PropertyMap[name.ToUpper()] = m;
            }
        }

        public void RemovePropertyMapping(string name)
        {
            if (name != null &&
                _PropertyMap.ContainsKey(name.ToUpper()))
                _PropertyMap.Remove(name.ToUpper());
        }

        virtual public bool GetPropertyAllowsMultipleValues(object obj)
        {
            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null && p.Name != null)
            {
                string name = p.Name.ToUpper();
                if (_PropertyMap.ContainsKey(name))
                {
                    PropertyMapping m = _PropertyMap[name];
                    return m.AllowsMultipleValuesPerProperty;
                }
            }
            return false;
        }
        
        virtual public Type GetPropertyMapping(object obj)
        {
            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null && p.Name != null)
            {
                string name = p.Name.ToUpper();
                if (_PropertyMap.ContainsKey(name))
                {
                    PropertyMapping m = _PropertyMap[name];
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
