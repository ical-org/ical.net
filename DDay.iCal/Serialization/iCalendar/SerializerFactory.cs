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
        static public ISerializable Create(object obj)
        {
            if (obj != null)
            {
                Type type = obj.GetType();
                if (type.IsArray)
                    return new ArraySerializer(obj as Array);
                else if (type.IsEnum)
                    return new EnumSerializer(obj as Enum);
                else if (type == typeof(DDay.iCal.iCalendar) || type.IsSubclassOf(typeof(DDay.iCal.iCalendar)))
                    return new iCalendarSerializer(obj as DDay.iCal.iCalendar);
                else if (type == typeof(UniqueComponent) || type.IsSubclassOf(typeof(UniqueComponent)))
                    return new UniqueComponentSerializer(obj as UniqueComponent);
                else if (type == typeof(ComponentBase) || type.IsSubclassOf(typeof(ComponentBase)))
                    return new ComponentBaseSerializer(obj as ComponentBase);
                else if (type == typeof(iCalDataType) || type.IsSubclassOf(typeof(iCalDataType)))
                    return new DataTypeSerializer(obj as iCalDataType);
                else if (type == typeof(Parameter) || type.IsSubclassOf(typeof(Parameter)))
                    return new ParameterSerializer(obj as Parameter);
                else if (type == typeof(Property) || type.IsSubclassOf(typeof(Property)))
                    return new PropertySerializer(obj as Property);
                // We don't allow ContentLines to directly serialize, as
                // they're likely a byproduct of loading a calendar, and we
                // are already going to reproduce the content line(s) anyway.
                else if (type == typeof(ContentLine) || type.IsSubclassOf(typeof(ContentLine)))
                    return null;
                else if (type == typeof(iCalObject) || type.IsSubclassOf(typeof(iCalObject)))
                    return new iCalObjectSerializer(obj as iCalObject);
            }
            return null;
        }
    }
}
