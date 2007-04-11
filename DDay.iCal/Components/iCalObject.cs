using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// The base class for all iCalendar objects, components, and data types.
    /// </summary>
    public class iCalObject
    {
        #region Public Events

        public event EventHandler Load;

        #endregion

        #region Private Fields

        private iCalObject m_Parent = null;
        private ArrayList m_Children = new ArrayList();
        private string m_name;
        private Hashtable m_Properties = new Hashtable();
        private Hashtable m_Parameters = new Hashtable();

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the parent <see cref="iCalObject"/> that owns this one.
        /// </summary>
        public iCalObject Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
        public Hashtable Properties
        {
            get { return m_Properties; }
            set { m_Properties = value; }
        }

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
        public Hashtable Parameters
        {
            get { return m_Parameters; }
            set { m_Parameters = value; }
        }

        /// <summary>
        /// A collection of <see cref="iCalObject"/>s that are children 
        /// of the current object.
        /// </summary>
        public ArrayList Children
        {
            get { return m_Children; }
            set { m_Children = value; }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="iCalObject"/>.  For iCalendar components,
        /// this is the RFC 2445 name of the component.
        /// <example>
        ///     <list type="bullet">
        ///         <item>Event - "VEVENT"</item>
        ///         <item>Todo - "VTODO"</item>
        ///         <item>TimeZone - "VTIMEZONE"</item>
        ///         <item>etc.</item>
        ///     </list>
        /// </example>
        /// </summary>        
        virtual public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Returns the <see cref="DDay.iCal.iCalendar"/> that this <see cref="iCalObject"/>
        /// belongs to.
        /// </summary>
        public iCalendar iCalendar
        {
            get
            {
                iCalObject obj = this;
                while (obj.Parent != null)
                    obj = obj.Parent;

                if (obj is iCalendar)
                    return obj as iCalendar;
                return null;
            }
        }

        #endregion

        #region Protected Properties

        virtual protected List<object> SerializedItems
        {
            get
            {
                return new List<object>();
            }
        }

        #endregion

        #region Constructors

        internal iCalObject() { }
        public iCalObject(iCalObject parent)
        {
            Parent = parent;
            if (parent != null)
            {
                if (!(this is Property) &&
                    !(this is Parameter))
                    parent.AddChild(this);
            }
        }
        public iCalObject(iCalObject parent, string name)
            : this(parent)
        {
            Name = name;
        }

        #endregion

        #region Public Overridable Methods

        /// <summary>
        /// Adds an <see cref="iCalObject"/>-based object as a child
        /// of the current object.
        /// </summary>
        /// <param name="child">The <see cref="iCalObject"/>-based object to add.</param>
        virtual public void AddChild(iCalObject child)
        {
            Children.Add(child);
        }

        /// <summary>
        /// Removed an <see cref="iCalObject"/>-based object from the <see cref="Children"/>
        /// collection.
        /// </summary>
        /// <param name="child"></param>
        virtual public void RemoveChild(iCalObject child)
        {
            if (Children.Contains(child))
                Children.Remove(child);
        }

        /// <summary>
        /// For iCalendar components, automatically finds and retrieves fields that
        /// match the field specified in the <see cref="ContentLine"/>, and sets
        /// their value.
        /// <example>
        /// For example, if a public DTStart field exists in the specified component,
        /// (i.e. <c>public Date_Time DTStart;</c>)
        /// and a content line of <c>DTSTART;TZID=US-Eastern:20060830T090000</c> is
        /// encountered, this method will automatically set the value of the
        /// DTStart field to Aug. 30, 2006, 9:00 AM in the US-Eastern TimeZone.
        /// </example>
        /// <note>
        ///     It should not be necessary to invoke this method manually as it
        ///     is handled automatically during the iCalendar parsing.
        /// </note>
        /// </summary>
        /// <param name="cl">The <see cref="ContentLine"/> to process.</param>
        virtual public void SetContentLineValue(ContentLine cl)
        {
            if (cl.Name != null)
            {
                string name = cl.Name;
                Type type = GetType();

                // Remove X- from the property name, since
                // non-standard properties are named like
                // everything else, but also have the NonstandardAttribute
                // attached.
                if (name.StartsWith("X-"))
                    name = name.Remove(0, 2);
                
                // Replace invalid characters
                name = name.Replace("-","_");

                //
                // Find the public field that matches the name of our content line (ignoring case)
                //
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Static);
                PropertyInfo property = null;
                
                if (field == null)
                    property = type.GetProperty(name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Static);

                if (field != null ||
                    property != null)
                {
                    // Get the field/property's value
                    object value = field == null ? property.GetValue(this, null) : field.GetValue(this);
                    Type itemType = field == null ? property.PropertyType : field.FieldType;
                    object[] itemAttributes = field == null ? property.GetCustomAttributes(true) : field.GetCustomAttributes(true);

                    Type elementType = itemType.IsArray ? itemType.GetElementType() : itemType;

                    // If it's an iCalDataType, or an array of iCalDataType, then let's fill it!
                    if (itemType.IsSubclassOf(typeof(iCalDataType)) ||
                        (itemType.IsArray && itemType.GetElementType().IsSubclassOf(typeof(iCalDataType))))
                    {
                        iCalDataType icdt = null;
                        if (!itemType.IsArray)
                            icdt = (iCalDataType)value;
                        if (icdt == null)
                            icdt = (iCalDataType)Activator.CreateInstance(elementType);

                        // Assign custom attributes for the specific field
                        icdt.Attributes = itemAttributes;

                        // Set the content line for the object.  On most objects, this
                        // triggers the object to parse the content line with Parse().
                        icdt.ContentLine = cl;

                        // It's an array, let's add an item to the end
                        if (itemType.IsArray)
                        {
                            ArrayList arr = new ArrayList();
                            if (value != null)
                                arr.AddRange((ICollection)value);
                            arr.Add(icdt);
                            if (field != null)
                                field.SetValue(this, arr.ToArray(elementType));
                            else
                                property.SetValue(this, arr.ToArray(elementType), null);
                        }
                        // Otherwise, set the value directly!
                        else 
                        {
                            if (field != null)
                                field.SetValue(this, icdt);
                            else property.SetValue(this, icdt, null);
                        }
                    }
                    else
                    {
                        FieldInfo minValue = itemType.GetField("MinValue");
                        object minVal = (minValue != null) ? minValue.GetValue(null) : null;

                        if (itemType.IsArray)
                        {
                            ArrayList arr = new ArrayList();
                            if (value != null)
                                arr.AddRange((ICollection)value);
                            arr.Add(cl.Value);

                            if (field != null)
                                field.SetValue(this, arr.ToArray(elementType));
                            else property.SetValue(this, arr.ToArray(elementType), null);
                        }
                        // Always assign enum values
                        else if (itemType.IsEnum)
                        {
                            if (field != null)
                                field.SetValue(this, Enum.Parse(itemType, cl.Value.Replace("-", "_")));
                            else property.SetValue(this, Enum.Parse(itemType, cl.Value.Replace("-", "_")), null);
                        }
                        // Otherwise, set the value directly!
                        else if (value == null || value.Equals(minVal))
                        {
                            if (field != null)
                                field.SetValue(this, cl.Value);
                            else property.SetValue(this, cl.Value, null);
                        }
                        else ;// FIXME: throw single-value exception, if "strict" parsing is enabled
                    }
                }
                else
                {
                    // This is a non-standard property.  Let's load it into memory,
                    // So we can serialize it later
                    Property p = new Property(cl);
                    p.AddToParent();
                }
            }
        }

        /// <summary>
        /// Invokes the <see cref="Load"/> event handler when the object has been fully loaded.
        /// This is automatically called when processing objects that inherit from 
        /// <see cref="ComponentBase"/> (i.e. all iCalendar components).
        /// </summary>
        virtual public void OnLoad(EventArgs e)
        {
            if (this.Load != null)
                this.Load(this, e);
        }

        /// <summary>
        /// Creates a copy of the <see cref="iCalObject"/>.
        /// </summary>
        /// <returns>The copy of the <see cref="iCalObject"/>.</returns>
        public iCalObject Copy() { return Copy(Parent); }
        virtual public iCalObject Copy(iCalObject parent)
        {
            iCalObject obj = null;
            Type type = GetType();
            ConstructorInfo[] constructors = type.GetConstructors();
            foreach (ConstructorInfo ci in constructors)
            {
                // Try to find a constructor with the following signature:
                // .ctor(iCalObject parent, string name)
                ParameterInfo[] parms = ci.GetParameters();
                if (parms.Length == 2 &&
                    parms[0].ParameterType == typeof(iCalObject) &&
                    parms[1].ParameterType == typeof(string))
                {                    
                    obj = (iCalObject)Activator.CreateInstance(type, parent, Name);
                }
            }
            if (obj == null)
            {
                foreach (ConstructorInfo ci in constructors)
                {
                    // Try to find a constructor with the following signature:
                    // .ctor(iCalObject parent)
                    ParameterInfo[] parms = ci.GetParameters();
                    if (parms.Length == 1 &&
                        parms[0].ParameterType == typeof(iCalObject))
                    {
                        obj = (iCalObject)Activator.CreateInstance(type, parent);
                    }
                }
            }
            // No complex constructor was found, use the default constructor!
            if (obj == null)
                obj = (iCalObject)Activator.CreateInstance(type);

            // Add properties
            foreach (DictionaryEntry de in Properties)
                ((Property)(de.Value)).Copy(obj);

            // Add parameters
            foreach (DictionaryEntry de in Parameters)
                ((Parameter)(de.Value)).Copy(obj);

            // Add each child
            foreach (iCalObject child in Children)
                child.Copy(obj);

            //
            // Get a list of serialized items,
            // iterate through each, make a copy
            // of each item, and assign it to our
            // copied object.
            //
            List<object> items = SerializedItems;
            foreach (object item in items)
            {
                FieldInfo field = null;
                PropertyInfo prop = null;

                if (item is FieldInfo)
                    field = (FieldInfo)item;
                else
                    prop = (PropertyInfo)item;

                // Get the item's value
                object itemValue = (field == null) ? prop.GetValue(this, null) : field.GetValue(this);

                // Make a copy of the item, if it's copyable.
                if (itemValue is iCalObject)
                    itemValue = ((iCalObject)itemValue).Copy(obj);
                else { } // FIXME: make an exact copy, if possible.

                // Set the item's value in our copied object
                if (field == null)
                    prop.SetValue(obj, itemValue, null);
                else field.SetValue(obj, itemValue);
            }

            return obj;
        }

        /// <summary>
        /// Initializes an object that has just been created.
        /// </summary>
        virtual public void CreateInitialize()
        {
        }

        #endregion
    }
}
