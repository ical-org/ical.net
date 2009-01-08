using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class ComponentBaseSerializer : iCalObjectSerializer
    {
        #region Private Fields

        private DDay.iCal.Components.ComponentBase m_Component;
        private bool _OptimizeForSpeed = true;
        
        #endregion

        #region Public Properties

        protected ComponentBase Component
        {
            get { return m_Component; }
            set
            {
                if (!object.Equals(m_Component, value))
                {
                    m_Component = value;
                    base.Object = value;
                }
            }
        }

        public bool OptimizeForSpeed
        {
            get { return _OptimizeForSpeed; }
            set
            {
                if (!object.Equals(_OptimizeForSpeed, value))
                    _OptimizeForSpeed = value;
            }
        }

        #endregion

        #region Protected Properties

        virtual protected List<object> FieldsAndProperties
        {
            get
            {
                List<object> List = new List<object>();
                foreach (FieldInfo fi in Component.GetType().GetFields())
                    if (fi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(fi);
                foreach (PropertyInfo pi in Component.GetType().GetProperties())
                    if (pi.GetCustomAttributes(typeof(SerializedAttribute), true).Length > 0)
                        List.Add(pi);
                return List;
            }
        }

        #endregion

        #region Constructors

        public ComponentBaseSerializer() {}
        public ComponentBaseSerializer(DDay.iCal.Components.ComponentBase component) : base(component)
        {
            Component = component;
        }

        #endregion

        #region Overrides

        public override string SerializeToString()
        {
            MemoryStream ms = new MemoryStream();
            Serialize(ms, Encoding.UTF8);
            
            ms.Position = 0;
            return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        }

        public override void Serialize(Stream stream, Encoding encoding)
        {
            // Open the component
            byte[] open = encoding.GetBytes("BEGIN:" + Component.Name + "\r\n");
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
                object obj = (field == null) ? prop.GetValue(Component, null) : field.GetValue(Component);

                // Adjust the items' name to be iCal-compliant
                if (obj is iCalObject)
                {
                    iCalObject ico = (iCalObject)obj;
                    if (ico.Name == null)
                        ico.Name = itemName.ToUpper().Replace("_", "-");

                    // If the property is non-standard, then replace
                    // it with an X-name
                    if (!ico.Name.StartsWith("X-"))
                    {
                        foreach (object attr in itemAttrs)
                        {
                            if (attr is NonstandardAttribute)
                            {
                                ico.Name = "X-" + ico.Name;
                                break;
                            }
                        }
                    }
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
            byte[] close = encoding.GetBytes("END:" + Component.Name + "\r\n");
            stream.Write(close, 0, close.Length);
        }

        public override iCalObject Deserialize(TextReader tr, Type iCalendarType)
        {
            // Normalize line endings, so "\r" is treated the same as "\r\n"
            // NOTE: fixed bug #1773194 - Some applications emit mixed line endings
            TextReader textReader = NormalizeLineEndings(tr, !OptimizeForSpeed);

            // Create a lexer for our text stream
            iCalLexer lexer = new iCalLexer(textReader);
            iCalParser parser = new iCalParser(lexer);            

            // Determine the calendar type we'll be using when constructing
            // iCalendar objects...
            parser.iCalendarType = iCalendarType;

            // Parse the component!
            DDay.iCal.iCalendar iCal = new DDay.iCal.iCalendar();
            iCalObject component = parser.component(iCal);

            // Close our text stream
            tr.Close();
            textReader.Close();

            // Return the parsed component
            return component;
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
