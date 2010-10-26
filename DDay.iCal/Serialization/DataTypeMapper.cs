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
            AddPropertyMapping("ATTACH", typeof(IList<IAttachment>), false);
            AddPropertyMapping("ATTENDEE", ResolveAttendeeProperty, false);
            AddPropertyMapping("CATEGORIES", typeof(IList<string>), true);
            AddPropertyMapping("COMMENT", typeof(IList<string>), false);
            AddPropertyMapping("COMPLETED", typeof(IDateTime), false);
            AddPropertyMapping("CONTACT", typeof(IList<string>), false);
            AddPropertyMapping("CREATED", typeof(IDateTime), false);
            AddPropertyMapping("DTEND", typeof(IDateTime), false);
            AddPropertyMapping("DTSTAMP", typeof(IDateTime), false);
            AddPropertyMapping("DTSTART", typeof(IDateTime), false);
            AddPropertyMapping("DUE", typeof(IDateTime), false);
            AddPropertyMapping("DURATION", typeof(TimeSpan), false);
            AddPropertyMapping("EXDATE", typeof(IList<IPeriodList>), false);
            AddPropertyMapping("EXRULE", typeof(IList<IRecurrencePattern>), false);
            AddPropertyMapping("FREEBUSY", typeof(IList<IFreeBusyEntry>), true);
            AddPropertyMapping("GEO", typeof(IGeographicLocation), false);
            AddPropertyMapping("LAST-MODIFIED", typeof(IDateTime), false);
            AddPropertyMapping("ORGANIZER", typeof(IOrganizer), false);
            AddPropertyMapping("PERCENT-COMPLETE", typeof(int), false);
            AddPropertyMapping("PRIORITY", typeof(int), false);
            AddPropertyMapping("RDATE", typeof(IList<IPeriodList>), false);
            AddPropertyMapping("RECURRENCE-ID", typeof(IDateTime), false);
            AddPropertyMapping("RELATED-TO", typeof(IList<string>), false);
            AddPropertyMapping("REQUEST-STATUS", typeof(IList<IRequestStatus>), false);
            AddPropertyMapping("REPEAT", typeof(int), false);
            AddPropertyMapping("RESOURCES", typeof(IList<string>), true);
            AddPropertyMapping("RRULE", typeof(IList<IRecurrencePattern>), false);
            AddPropertyMapping("SEQUENCE", typeof(int), false);
            AddPropertyMapping("STATUS", ResolveStatusProperty, false);
            AddPropertyMapping("TRANSP", typeof(TransparencyType), false);
            AddPropertyMapping("TRIGGER", typeof(ITrigger), false);
            AddPropertyMapping("TZNAME", typeof(IList<string>), false);
            AddPropertyMapping("TZOFFSETFROM", typeof(IUTCOffset), false);
            AddPropertyMapping("TZOFFSETTO", typeof(IUTCOffset), false);
            AddPropertyMapping("TZURL", typeof(Uri), false);
            AddPropertyMapping("URL", typeof(Uri), false);
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
