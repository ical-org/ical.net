using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization.iCalendar.DataTypes;
using DDay.iCal.Serialization.iCalendar.Components;

namespace DDay.iCal.Serialization.iCalendar
{
    /// <summary>
    /// A class that generates serialization objects for iCalendar components.
    /// </summary>
    public class SerializerFactory
    {
        static public ISerializable Create(object obj, ISerializationContext ctx)
        {
            if (obj != null)
            {
                ISerializable s = null;

                Type type = obj.GetType();
                if (type.IsArray)
                    s = new ArraySerializer(obj as Array);
                else if (type.IsEnum)
                    s = new EnumSerializer(obj as Enum);
                else if (typeof(DDay.iCal.iCalendar).IsAssignableFrom(type))
                    s = new iCalendarSerializer(obj as DDay.iCal.iCalendar);
                else if (typeof(Event).IsAssignableFrom(type))
                    s = new EventSerializer(obj as Event);
                else if (typeof(RecurringComponent).IsAssignableFrom(type))
                    s = new RecurringComponentSerializer(obj as RecurringComponent);
                else if (typeof(UniqueComponent).IsAssignableFrom(type))
                    s = new UniqueComponentSerializer(obj as UniqueComponent);
                else if (typeof(ComponentBase).IsAssignableFrom(type))
                    s = new ComponentBaseSerializer(obj as ComponentBase);
                else if (typeof(iCalDataType).IsAssignableFrom(type))
                    s = new DataTypeSerializer(obj as iCalDataType);
                else if (typeof(Parameter).IsAssignableFrom(type))
                    s = new ParameterSerializer(obj as Parameter);
                else if (typeof(Property).IsAssignableFrom(type))
                    s = new PropertySerializer(obj as Property);
                // We don't allow ContentLines to directly serialize, as
                // they're likely a byproduct of loading a calendar, and we
                // are already going to reproduce the content line(s) anyway.
                else if (typeof(ContentLine).IsAssignableFrom(type))
                    return null;
                else if (typeof(iCalObject).IsAssignableFrom(type))
                    s = new iCalObjectSerializer(obj as iCalObject);

                if (s != null && ctx != null)
                    s.SerializationContext = ctx;

                return s;
            }
            return null;
        }
    }
}
