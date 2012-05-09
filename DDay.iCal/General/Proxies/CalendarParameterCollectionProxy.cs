using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.Collections;
using System.Collections;

namespace DDay.iCal
{
    public class CalendarParameterCollectionProxy :
        GroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>,
        ICalendarParameterCollectionProxy
    {
        #region Protected Properties

        protected IGroupedValueList<string, ICalendarParameter, CalendarParameter, string> Parameters
        {
            get
            {
                return RealObject as IGroupedValueList<string, ICalendarParameter, CalendarParameter, string>;
            }
        }

        #endregion

        #region Constructors

        public CalendarParameterCollectionProxy(IGroupedList<string, ICalendarParameter> realObject) :
            base(realObject)
        {
        }

        #endregion

        #region ICalendarParameterCollection

        virtual public void SetParent(ICalendarObject parent)
        {            
            foreach (ICalendarParameter parameter in this)
            {
                parameter.Parent = parent;
            }
        }

        virtual public void Add(string name, string value)
        {
            RealObject.Add(new CalendarParameter(name, value));
        }

        virtual public string Get(string name)
        {
            var parameter = RealObject
                .AllOf(name)
                .FirstOrDefault();

            if (parameter != null)
                return parameter.Value;
            return default(string);
        }

        virtual public IList<string> GetMany(string name)
        {
            return new GroupedValueListProxy<string, ICalendarParameter, CalendarParameter, string, string>(
                Parameters, 
                name
            );
        }

        virtual public void Set(string name, string value)
        {
            var parameter = RealObject
                .AllOf(name)
                .FirstOrDefault();

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, value));
            }
            else
            {
                parameter.SetValue(value);
            }
        }

        virtual public void Set(string name, IEnumerable<string> values)
        {
            var parameter = RealObject
                .AllOf(name)
                .FirstOrDefault();

            if (parameter == null)
            {
                RealObject.Add(new CalendarParameter(name, values));
            }
            else
            {
                parameter.SetValue(values);
            }
        }

        virtual public int IndexOf(ICalendarParameter obj)
        {
            return Parameters.IndexOf(obj);
        }

        virtual public void Insert(int index, ICalendarParameter item)
        {
            Parameters.Insert(index, item);
        }

        virtual public void RemoveAt(int index)
        {
            Parameters.RemoveAt(index);
        }

        virtual public ICalendarParameter this[int index]
        {
            get
            {
                return Parameters[index];
            }
            set
            {                
            }
        }

        #endregion
    }
}
