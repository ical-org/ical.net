using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal.Objects;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.iCalendar.Objects
{
    public class ComponentBaseSerializer : iCalObjectSerializer
    {
        #region Private Fields

        private DDay.iCal.Objects.ComponentBase m_component;

        #endregion

        #region Protected Properties

        protected ComponentBase Component
        {
            get { return m_component; }
        }

        virtual protected List<object> FieldsAndProperties
        {
            get
            {
                List<object> List = new List<object>();
                foreach (FieldInfo fi in m_component.GetType().GetFields())
                    if (fi.GetCustomAttributes(typeof(Serialized), true).Length > 0)
                        List.Add(fi);
                foreach (PropertyInfo pi in m_component.GetType().GetProperties())
                    if (pi.GetCustomAttributes(typeof(Serialized), true).Length > 0)
                        List.Add(pi);
                return List;
            }
        }

        #endregion

        #region Constructors

        public ComponentBaseSerializer(DDay.iCal.Objects.ComponentBase component) : base(component)
        {
            this.m_component = component;
        }

        #endregion

        #region ISerializable Members

        public override void Serialize(Stream stream, Encoding encoding)
        {
            // Open the component
            byte[] open = encoding.GetBytes("BEGIN:" + m_component.Name + "\r\n");
            stream.Write(open, 0, open.Length);
                        
            // Get a list of fields and properties
            List<object> items = this.FieldsAndProperties;
            
            // Alphabetize the list of fields & properties we just obtained
            items.Sort(new FieldPropertyAlphabetizer());

            // Iterate through each item and attempt to serialize it
            foreach (object item in items)
            {
                FieldInfo field = null;
                PropertyInfo prop = null;
                Type itemType = null;
                string itemName = null;
                object[] itemAttrs = null;

                if (item is FieldInfo)
                {
                    field = (FieldInfo)item;
                    itemType = field.FieldType;
                    itemName = field.Name;
                }
                else
                {
                    prop = (PropertyInfo)item;
                    itemType = prop.PropertyType;
                    itemName = prop.Name;
                }

                // Get attributes that are attached to each item
                itemAttrs = (field != null) ? field.GetCustomAttributes(true) : prop.GetCustomAttributes(true);

                // Get the item's value
                object obj = (field == null) ? prop.GetValue(m_component, null) : field.GetValue(m_component);

                // Adjust the items' name to be iCal-compliant
                if (obj is iCalObject)
                {
                    iCalObject ico = (iCalObject)obj;
                    if (ico.Name == null)
                        ico.Name = itemName.ToUpper().Replace("_", "-");                    
                }

                // Retrieve custom attributes for this field/property
                if (obj is iCalDataType)
                    ((iCalDataType)obj).Attributes = itemAttrs;                

                // Get the default value of the object, if available
                object defaultValue = null;
                foreach (Attribute a in itemAttrs)
                    if (a is DefaultValueAttribute)
                        defaultValue = ((DefaultValueAttribute)a).Value;

                // Create a serializer for the object
                ISerializable serializer = SerializerFactory.Create(obj);           
                
                // To continue, the default value must either not be set,
                // or it must not match the actual value of the item.
                if (defaultValue == null ||
                    (serializer != null && !serializer.SerializeToString().Equals(defaultValue.ToString())))
                {
                    // FIXME: enum values cannot name themselves; we need to do it for them.
                    // For this to happen, we probably need to wrap enum values into a 
                    // class that inherits from iCalObject.
                    if (itemType.IsEnum)
                    {       
                        byte[] data = encoding.GetBytes(itemName.ToUpper().Replace("_", "-") + ":");
                        stream.Write(data, 0, data.Length);
                    }
                    
                    // Actually serialize the object
                    if (serializer != null)
                        serializer.Serialize(stream, encoding);
                }
            }

            // If any extra serialization is necessary, do it now
            base.Serialize(stream, encoding);

            // Close the component
            byte[] close = encoding.GetBytes("END:" + m_component.Name + "\r\n");
            stream.Write(close, 0, close.Length);
        }

        #endregion

        #region Helper Classes

        private class FieldPropertyAlphabetizer : IComparer<object>
        {
            #region IComparer<object> Members

            public int Compare(object x, object y)
            {
                string xName = null;
                string yName = null;
                if (x is FieldInfo)
                    xName = ((FieldInfo)x).Name;
                else if (x is PropertyInfo)
                    xName = ((PropertyInfo)x).Name;

                if (y is FieldInfo)
                    yName = ((FieldInfo)y).Name;
                else if (y is PropertyInfo)
                    yName = ((PropertyInfo)y).Name;

                if (xName == null || yName == null)
                    return 0;
                else return xName.CompareTo(yName);
            }

            #endregion
        }

        #endregion
    }
}
