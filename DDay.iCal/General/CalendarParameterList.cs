using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal
{
    public class CalendarParameterList :
        KeyedList<ICalendarParameter, string>,
        ICalendarParameterList
    {
        #region ICalendarParameterList Members

        public void Set(string name, object value)
        {
            if (name != null)
            {
                if (value != null)
                    this[name] = new CalendarParameter(name, value.ToString());
                else
                    Remove(name);
            }
        }

        public T Get<T>(string name)
        {
            object obj = Get(name, typeof(T));
            if (obj is T)
                return (T)obj;
            return default(T);
        }

        public string[] Get(string name)
        {
            if (name != null && ContainsKey(name))
            {
                CalendarParameter p = (CalendarParameter)this[name];
                if (p.Values != null)
                {
                    string[] values = new string[p.Values.Count];
                    p.Values.CopyTo(values, 0);
                    return values;
                }
            }
            return null;
        }

        public object Get(string name, Type returnType)
        {
            if (name != null && returnType != null && ContainsKey(name))
            {
                string[] parms = Get(name);
                if (parms != null && parms.Length > 0)
                {
                    if (returnType.IsEnum)
                    {
                        // Return enumerated values
                        return Enum.Parse(returnType, parms[0], true);
                    }
                    else if (typeof(iCalDataType).IsAssignableFrom(returnType))
                    {
                        // Return iCalDataType values (or subclasses)
                        iCalDataType dt = (iCalDataType)Activator.CreateInstance(returnType);
                        dt.CopyFrom(dt.Parse(parms[0]));
                        return dt;
                    }
                    else if (returnType.IsArray)
                    {
                        // Return array values of iCalDataType (or subclasses),
                        // or arrays of strings
                        Type elementType = returnType.GetElementType();
                        if (typeof(iCalDataType).IsAssignableFrom(elementType))
                        {
                            ArrayList al = new ArrayList();
                            foreach (string p in parms)
                            {
                                iCalDataType dt = (iCalDataType)Activator.CreateInstance(returnType);
                                dt.CopyFrom(dt.Parse(p));
                                al.Add(dt);
                            }
                            return al.ToArray(elementType);
                        }
                        else if (typeof(string).IsAssignableFrom(elementType))
                        {
                            // An array of strings, return this value directly.
                            return parms;
                        }
                    }
                    else if (typeof(string).IsAssignableFrom(returnType))
                    {
                        return parms[0];
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
