using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.ComponentModel;

namespace DDay.iCal.Components
{
    /// <summary>
    /// The base class for all iCalendar objects, components, and data types.
    /// </summary>
    public class iCalObject        
    {
        #region Public Events

        public event EventHandler Loaded;

        #endregion

        #region Private Fields

        private iCalObject _Parent = null;
        private List<iCalObject> _Children = new List<iCalObject>();
        private string _Name;
        private Hashtable _Properties = new Hashtable();
        private Hashtable _Parameters = new Hashtable();

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the parent <see cref="iCalObject"/> that owns this one.
        /// </summary>
        public iCalObject Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }

        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
        public Hashtable Properties
        {
            get { return _Properties; }
            set { _Properties = value; }
        }

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
        public Hashtable Parameters
        {
            get { return _Parameters; }
            set { _Parameters = value; }
        }

        /// <summary>
        /// A collection of <see cref="iCalObject"/>s that are children 
        /// of the current object.
        /// </summary>
        public List<iCalObject> Children
        {
            get { return _Children; }
            set { _Children = value; }
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
            get { return _Name; }
            set { _Name = value; } 
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
        /// Invokes the <see cref="Load"/> event handler when the object has been fully loaded.
        /// This is automatically called when processing objects that inherit from 
        /// <see cref="ComponentBase"/> (i.e. all iCalendar components).
        /// </summary>
        virtual public void OnLoaded(EventArgs e)
        {
            if (this.Loaded != null)
                this.Loaded(this, e);
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
                    obj = ci.Invoke(new object[] { parent, Name }) as iCalObject;
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
                        obj = ci.Invoke(new object[] { parent }) as iCalObject;
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
