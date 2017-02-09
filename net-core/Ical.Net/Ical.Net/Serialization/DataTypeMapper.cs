using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public delegate Type TypeResolverDelegate(object context);

    internal class DataTypeMapper : IDataTypeMapper
    {
        private class PropertyMapping
        {
            public Type ObjectType { get; set; }
            public TypeResolverDelegate Resolver { get; set; }
            public bool AllowsMultipleValuesPerProperty { get; set; }
        }

        private readonly IDictionary<string, PropertyMapping> _propertyMap = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);

        public DataTypeMapper()
        {
            AddPropertyMapping("ACTION", typeof (AlarmAction), false);
            AddPropertyMapping("ATTACH", typeof (Attachment), false);
            AddPropertyMapping("ATTENDEE", typeof (Attendee), false);
            AddPropertyMapping("CATEGORIES", typeof (string), true);
            AddPropertyMapping("COMMENT", typeof (string), false);
            AddPropertyMapping("COMPLETED", typeof (IDateTime), false);
            AddPropertyMapping("CONTACT", typeof (string), false);
            AddPropertyMapping("CREATED", typeof (IDateTime), false);
            AddPropertyMapping("DTEND", typeof (IDateTime), false);
            AddPropertyMapping("DTSTAMP", typeof (IDateTime), false);
            AddPropertyMapping("DTSTART", typeof (IDateTime), false);
            AddPropertyMapping("DUE", typeof (IDateTime), false);
            AddPropertyMapping("DURATION", typeof (TimeSpan), false);
            AddPropertyMapping("EXDATE", typeof (PeriodList), false);
            AddPropertyMapping("EXRULE", typeof (RecurrencePattern), false);
            AddPropertyMapping("FREEBUSY", typeof (FreeBusyEntry), true);
            AddPropertyMapping("GEO", typeof (GeographicLocation), false);
            AddPropertyMapping("LAST-MODIFIED", typeof (IDateTime), false);
            AddPropertyMapping("ORGANIZER", typeof (Organizer), false);
            AddPropertyMapping("PERCENT-COMPLETE", typeof (int), false);
            AddPropertyMapping("PRIORITY", typeof (int), false);
            AddPropertyMapping("RDATE", typeof (PeriodList), false);
            AddPropertyMapping("RECURRENCE-ID", typeof (IDateTime), false);
            AddPropertyMapping("RELATED-TO", typeof (string), false);
            AddPropertyMapping("REQUEST-STATUS", typeof (RequestStatus), false);
            AddPropertyMapping("REPEAT", typeof (int), false);
            AddPropertyMapping("RESOURCES", typeof (string), true);
            AddPropertyMapping("RRULE", typeof (RecurrencePattern), false);
            AddPropertyMapping("SEQUENCE", typeof (int), false);
            AddPropertyMapping("STATUS", ResolveStatusProperty, false);
            AddPropertyMapping("TRANSP", typeof (TransparencyType), false);
            AddPropertyMapping("TRIGGER", typeof (Trigger), false);
            AddPropertyMapping("TZNAME", typeof (string), false);
            AddPropertyMapping("TZOFFSETFROM", typeof (UtcOffset), false);
            AddPropertyMapping("TZOFFSETTO", typeof (UtcOffset), false);
            AddPropertyMapping("TZURL", typeof (Uri), false);
            AddPropertyMapping("URL", typeof (Uri), false);
        }

        protected Type ResolveStatusProperty(object context)
        {
            var obj = context as ICalendarObject;
            if (obj != null)
            {
                if (obj.Parent is CalendarEvent)
                {
                    return typeof (EventStatus);
                }
                if (obj.Parent is Todo)
                {
                    return typeof (TodoStatus);
                }
                if (obj.Parent is Journal)
                {
                    return typeof (JournalStatus);
                }
            }

            return null;
        }

        public void AddPropertyMapping(string name, Type objectType, bool allowsMultipleValues)
        {
            if (name != null && objectType != null)
            {
                var m = new PropertyMapping
                {
                    ObjectType = objectType,
                    AllowsMultipleValuesPerProperty = allowsMultipleValues
                };

                _propertyMap[name] = m;
            }
        }

        public void AddPropertyMapping(string name, TypeResolverDelegate resolver, bool allowsMultipleValues)
        {
            if (name != null && resolver != null)
            {
                var m = new PropertyMapping
                {
                    Resolver = resolver,
                    AllowsMultipleValuesPerProperty = allowsMultipleValues
                };

                _propertyMap[name] = m;
            }
        }

        public void RemovePropertyMapping(string name)
        {
            if (name != null && _propertyMap.ContainsKey(name))
            {
                _propertyMap.Remove(name);
            }
        }

        public virtual bool GetPropertyAllowsMultipleValues(object obj)
        {
            var p = obj as ICalendarProperty;
            PropertyMapping m;
            return p?.Name != null && _propertyMap.TryGetValue(p.Name, out m) && m.AllowsMultipleValuesPerProperty;
        }

        public virtual Type GetPropertyMapping(object obj)
        {
            var p = obj as ICalendarProperty;
            if (p?.Name == null)
            {
                return null;
            }

            PropertyMapping m;
            if (!_propertyMap.TryGetValue(p.Name, out m))
            {
                return null;
            }

            return m.Resolver == null
                ? m.ObjectType
                : m.Resolver(p);
        }
    }
}