using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public class ObjectFactory :
        IObjectFactory
    {
        #region IObjectFactory Members

        virtual public object Create(Type objectType, string value)
        {
            if (objectType != null && value != null)
            {
                if (objectType.IsEnum)
                {
                    // Return enumerated values
                    return Enum.Parse(objectType, value, true);
                }
                else if (typeof(ICalendarDataType).IsAssignableFrom(objectType))
                {
                    // Return ICalendarObject values (or subclasses)
                    ICalendarDataType dt = (ICalendarDataType)Activator.CreateInstance(objectType);
                    dt.CopyFrom(dt.Parse(value));
                    return dt;
                }                
                else if (typeof(string).IsAssignableFrom(objectType))
                {
                    return value;
                }
            }
            return null;
        }

        virtual public object Create(Type objectType, string[] values)
        {
            if (objectType != null &&
                values != null &&
                values.Length > 0)
            {
                if (objectType.IsEnum ||
                    typeof(ICalendarDataType).IsAssignableFrom(objectType))
                {
                    return Create(objectType, values[0]);
                }
                else if (objectType.IsArray)
                {
                    // Return array values of iCalDataType (or subclasses),
                    // or arrays of strings
                    Type elementType = objectType.GetElementType();
                    if (typeof(ICalendarDataType).IsAssignableFrom(elementType))
                    {
                        ArrayList al = new ArrayList();
                        foreach (string p in values)
                        {
                            ICalendarDataType dt = (ICalendarDataType)Activator.CreateInstance(objectType);
                            dt.CopyFrom(dt.Parse(p));
                            al.Add(dt);
                        }
                        return al.ToArray(elementType);
                    }
                    else if (typeof(string).IsAssignableFrom(elementType))
                    {
                        // An array of strings, return this value directly.
                        return values;
                    }
                }
                else if (typeof(string[]).IsAssignableFrom(objectType))
                {
                    return values;
                }
            }
            return null;
        }

        #endregion
    }
}
