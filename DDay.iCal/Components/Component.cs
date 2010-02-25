using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// This class is used by the parsing framework as a factory class
    /// for <see cref="iCalendar"/> components.  Generally, you should
    /// not need to use this class directly.
    /// </summary>
#if DATACONTRACT
    [DataContract(IsReference = true, Name = "ComponentBase", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Component :
        CalendarObject,
        ICalendarComponent
    {
        #region Private Fields

        private ICalendarPropertyList m_Properties;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public ICalendarPropertyList Properties
        {
            get { return m_Properties; }
            protected set
            {
                this.m_Properties = value;
            }
        }

        #endregion

        #region Constructors

        public Component() : base() { Initialize(); }
        public Component(string name) : base(name) { Initialize(); }

        private void Initialize()
        {
            m_Properties = new CalendarPropertyList();
        }

        #endregion        

        #region Public Methods

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(string name, string value)
        {
            CalendarProperty p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        virtual public void AddProperty(CalendarProperty p)
        {
            p.Parent = this;
            Properties[p.Name] = p;
        }

        #endregion

        #region Protected Methods

        //protected CalendarProperty[] GetProperties(string name)
        //{
        //    if (name != null && Properties.ContainsKey(name))
        //    {
        //        CalendarProperty[] properties = new CalendarProperty[Properties.CountOf(name)];
        //        Properties.AllOf(name).CopyTo(properties, 0);
        //        return properties;
        //    }
        //    return null;
        //}

        //protected T GetEnumProperty<T>(string name)
        //{
        //    CalendarProperty[] properties = GetProperties(name);
        //    if (properties != null && properties.Length > 0)
        //    {
        //        Type returnType = typeof(T);
        //        if (returnType.IsEnum)
        //        {
        //            // Return enumerated values
        //            return (T)Enum.Parse(returnType, properties[0].Value, true);
        //        }
        //    }
        //    return default(T);
        //}

        //protected T[] GetArrayProperty<T>(string name)
        //{
        //    CalendarProperty[] properties = GetProperties(name);
        //    if (properties != null && properties.Length > 0)
        //    {
        //        Type returnType = typeof(T);
        //        if (returnType.IsArray)
        //        {
        //            // Return array values of iCalDataType (or subclasses),
        //            // or arrays of strings
        //            Type elementType = returnType.GetElementType();
        //            if (typeof(iCalDataType).IsAssignableFrom(elementType))
        //            {
        //                ArrayList al = new ArrayList();
        //                foreach (CalendarProperty p in properties)
        //                {
        //                    iCalDataType dt = (iCalDataType)Activator.CreateInstance(returnType);
        //                    dt.CopyFrom(dt.Parse(p.Value));
        //                    al.Add(dt);
        //                }
        //                return al.ToArray(elementType);
        //            }
        //            else if (typeof(string).IsAssignableFrom(elementType))
        //            {
        //                // An array of strings, return this value directly.
        //                return parms;
        //            }
        //        }
        //        else if (typeof(string).IsAssignableFrom(returnType))
        //        {
        //            return parms[0];
        //        }
        //    }
        //    return null;
        //}

        //protected T GetProperty<T>(string name)
        //{
        //    //Property[] properties = GetProperties(name);
        //    //if (properties != null && properties.Length > 0)
        //    //{
        //    //    Type returnType = typeof(T);
        //    //    if (returnType.IsEnum)
        //    //    {
        //    //        // Return enumerated values
        //    //        return (T)Enum.Parse(returnType, properties[0].Value, true);
        //    //    }
        //    //    else if (typeof(iCalDataType).IsAssignableFrom(returnType))
        //    //    {
        //    //        // Return iCalDataType values (or subclasses)
        //    //        iCalDataType dt = (iCalDataType)Activator.CreateInstance(returnType);
        //    //        dt.CopyFrom(dt.Parse(parms[0]));
        //    //        return dt;
        //    //    }
        //    //    else if (returnType.IsArray)
        //    //    {
        //    //        // Return array values of iCalDataType (or subclasses),
        //    //        // or arrays of strings
        //    //        Type elementType = returnType.GetElementType();
        //    //        if (typeof(iCalDataType).IsAssignableFrom(elementType))
        //    //        {
        //    //            ArrayList al = new ArrayList();
        //    //            foreach (string p in parms)
        //    //            {
        //    //                iCalDataType dt = (iCalDataType)Activator.CreateInstance(returnType);
        //    //                dt.CopyFrom(dt.Parse(p));
        //    //                al.Add(dt);
        //    //            }
        //    //            return al.ToArray(elementType);
        //    //        }
        //    //        else if (typeof(string).IsAssignableFrom(elementType))
        //    //        {
        //    //            // An array of strings, return this value directly.
        //    //            return parms;
        //    //        }
        //    //    }
        //    //    else if (typeof(string).IsAssignableFrom(returnType))
        //    //    {
        //    //        return parms[0];
        //    //    }
        //    //}
        //    return null;

        //}

        #endregion

        #region Private Methods

#if DATACONTRACT
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }
#endif

        #endregion

        #region Overrides

        protected override System.Collections.Generic.List<object> SerializedItems
        {
            get
            {
                //
                // Get a list of all the public fields and properties
                // that are marked as Serialized.
                //
                List<object> List = new List<object>();
                foreach (FieldInfo fi in GetType().GetFields())
                    if (fi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(fi);
                foreach (PropertyInfo pi in GetType().GetProperties())
                    if (pi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(pi);
                return List;
            }
        }

        #endregion
    }
}
