using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// The base class for all iCalendar objects and components.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarObject :
        CalendarObjectBase,
        ICalendarObject
    {
        #region Private Fields

        private ICalendarObject _Parent = null;
        private IList<ICalendarObject> _Children;
        private ServiceProvider _ServiceProvider;
        private string _Name;
        
        private int _Line;
        private int _Column;

        #endregion

        #region Constructors

        internal CalendarObject()
        {
            Initialize();
        }

        public CalendarObject(string name) : this()
        {
            Name = name;
        }

        public CalendarObject(int line, int col) : this()
        {
            Line = line;
            Column = col;
        }

        private void Initialize()
        {
            _Children = new List<ICalendarObject>();
            _ServiceProvider = new ServiceProvider();
        }

        #endregion

        #region Internal Methods

        [OnDeserializing]
        internal void DeserializingInternal(StreamingContext context)
        {
            OnDeserializing(context);
        }

        [OnDeserialized]
        internal void DeserializedInternal(StreamingContext context)
        {
            OnDeserialized(context);
        }

        #endregion

        #region Protected Methods

        virtual protected void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        virtual protected void OnDeserialized(StreamingContext context)
        {
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            ICalendarObject o = obj as ICalendarObject;
            if (o != null)
                return object.Equals(o.Name, Name);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Name != null)
                return Name.GetHashCode();
            else return base.GetHashCode();
        }

        public override void CopyFrom(ICopyable c)
        {
            ICalendarObject obj = c as ICalendarObject;
            if (obj != null)
            {
                // Copy the name
                this.Name = obj.Name;
                this.Parent = obj.Parent;
                this.Line = obj.Line;
                this.Column = obj.Column;
                
                // Add each child
                foreach (ICalendarObject child in obj.Children)
                    AddChild(child.Copy<ICalendarObject>());
            }
        }        

        #endregion

        #region ICalendarObject Members

        public event EventHandler<ObjectEventArgs<ICalendarObject>> ChildAdded;
        public event EventHandler<ObjectEventArgs<ICalendarObject>> ChildRemoved;

        protected void OnChildAdded(ICalendarObject child)
        {
            if (ChildAdded != null)
                ChildAdded(this, new ObjectEventArgs<ICalendarObject>(child));
        }

        protected void OnChildRemoved(ICalendarObject child)
        {
            if (ChildRemoved != null)
                ChildRemoved(this, new ObjectEventArgs<ICalendarObject>(child));
        }

        /// <summary>
        /// Returns the parent <see cref="iCalObject"/> that owns this one.
        /// </summary>
        virtual public ICalendarObject Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }

        /// <summary>
        /// A collection of <see cref="iCalObject"/>s that are children 
        /// of the current object.
        /// </summary>
        virtual public IList<ICalendarObject> Children
        {
            get { return _Children; }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="iCalObject"/>.  For iCalendar components,
        /// this is the RFC 5545 name of the component.
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
        virtual public IICalendar Calendar
        {
            get
            {
                ICalendarObject obj = this;
                while (!(obj is IICalendar) && obj.Parent != null)
                    obj = obj.Parent;

                if (obj is IICalendar)
                    return (IICalendar)obj;
                return null;
            }
            protected set
            {
                this._Parent = value;
            }
        }

        virtual public IICalendar iCalendar
        {
            get { return Calendar; }
            protected set { Calendar = value; }
        }

        virtual public int Line
        {
            get { return _Line; }
            set { _Line = value; }
        }

        virtual public int Column
        {
            get { return _Column; }
            set { _Column = value; }
        }

        /// <summary>
        /// Adds an <see cref="iCalObject"/>-based object as a child
        /// of the current object.
        /// </summary>
        /// <param name="child">The <see cref="iCalObject"/>-based object to add.</param>
        virtual public void AddChild(ICalendarObject child)
        {
            child.Parent = this;
            Children.Add(child);
            OnChildAdded(child);
        }

        /// <summary>
        /// Removed an <see cref="iCalObject"/>-based object from the <see cref="Children"/>
        /// collection.
        /// </summary>
        /// <param name="child"></param>
        virtual public void RemoveChild(ICalendarObject child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
                child.Parent = null;
                OnChildRemoved(child);
            }
        }

        #endregion       

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return _ServiceProvider.GetService(serviceType);
        }

        virtual public object GetService(string name)
        {
            return _ServiceProvider.GetService(name);            
        }

        virtual public T GetService<T>()
        {
            return _ServiceProvider.GetService<T>();
        }

        virtual public T GetService<T>(string name)
        {
            return _ServiceProvider.GetService<T>(name);
        }

        virtual public void SetService(string name, object obj)
        {
            _ServiceProvider.SetService(name, obj);
        }

        virtual public void SetService(object obj)
        {
            _ServiceProvider.SetService(obj);
        }

        virtual public void RemoveService(Type type)
        {
            _ServiceProvider.RemoveService(type);
        }

        virtual public void RemoveService(string name)
        {
            _ServiceProvider.RemoveService(name);
        }

        #endregion

        #region IKeyedObject<string> Members

        public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
