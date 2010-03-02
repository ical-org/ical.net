using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class CalendarValueObjectList<T, U> :
        KeyedList<T, string>,
        ICalendarDataList<T>
        where T : ICalendarValueObject, IKeyedObject<string>
        where U : T, new()
    {
        #region Private Fields

        IObjectFactory _ObjectFactory = new ObjectFactory();

        #endregion

        #region Constructors

        public CalendarValueObjectList()
        {
        }

        public CalendarValueObjectList(IObjectFactory objectFactory)
        {
            _ObjectFactory = objectFactory;
        }

        #endregion

        #region Private Methods

        private string[] GetPrivate(string name)
        {
            if (name != null && ContainsKey(name))
            {
                T data = this[name];
                if (data != null)
                    return data.ValueArray;
            }
            return null;
        }

        #endregion

        #region ICalendarDataList<T> Members

        public void Set(string name, object value)
        {
            if (name != null)
            {
                if (value != null)
                {
                    T data = new U();
                    data.Name = name;
                    data.Value = value.ToString();
                }
                else
                {
                    Remove(name);
                }
            }
        }

        public T Get<T>(string name)
        {
            object obj = Get(name, typeof(T));
            if (obj is T)
                return (T)obj;
            return default(T);
        }

        public object Get(string name, Type returnType)
        {
            if (name != null && ContainsKey(name))
            {
                return _ObjectFactory.Create(returnType, GetPrivate(name));
            }
            return null;
        }

        public T[] GetAll<T>(string name)
        {
            object obj = GetAll(name, typeof(T));
            if (obj is T[])
                return (T[])obj;
            return null;
        }

        public object[] GetAll(string name, Type returnType)
        {
            if (name != null && ContainsKey(name))
            {
                string[] values = GetPrivate(name);
                if (values != null)
                {
                    object[] objs = new object[values.Length];
                    for (int i = 0; i < values.Length; i++)
                        objs[i] = _ObjectFactory.Create(returnType, values[i]);
                }
            }
            return null;
        }

        #endregion
    }
}
