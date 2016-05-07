using System;
using System.Runtime.Serialization;
using ical.NET.Collections;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
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

        private ICalendarObject _parent = null;
        private ICalendarObjectList<ICalendarObject> _children;
        private ServiceProvider _serviceProvider;
        private string _name;
        
        private int _line;
        private int _column;

        #endregion

        #region Constructors

        internal CalendarObject()
        {
            Initialize();
        }

        public CalendarObject(string name)
            : this()
        {
            Name = name;
        }

        public CalendarObject(int line, int col) : this()
        {
            Line = line;
            Column = col;
        }

        void Initialize()
        {
            _children = new CalendarObjectList(this);
            _serviceProvider = new ServiceProvider();

            _children.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarObject, int>>(_Children_ItemAdded);
            _children.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarObject, int>>(_Children_ItemRemoved);
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

        #region Event Handlers

        void _Children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            e.First.Parent = null;
        }

        void _Children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            e.First.Parent = this;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            var o = obj as ICalendarObject;
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
            var obj = c as ICalendarObject;
            if (obj != null)
            {
                // Copy the name and basic information
                this.Name = obj.Name;
                this.Parent = obj.Parent;
                this.Line = obj.Line;
                this.Column = obj.Column;
                
                // Add each child
                this.Children.Clear();
                foreach (var child in obj.Children)
                    this.AddChild(child.Copy<ICalendarObject>());
            }
        }        

        #endregion

        #region ICalendarObject Members
        
        /// <summary>
        /// Returns the parent iCalObject that owns this one.
        /// </summary>
        virtual public ICalendarObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// A collection of iCalObjects that are children of the current object.
        /// </summary>
        virtual public ICalendarObjectList<ICalendarObject> Children
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// Gets or sets the name of the iCalObject.  For iCalendar components, this is the RFC 5545 name of the component.
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
            get { return _name; }
            set
            {
                if (!object.Equals(_name, value))
                {
                    var old = _name;
                    _name = value;
                    OnGroupChanged(old, _name);
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="ICalendar"/> that this DDayiCalObject belongs to.
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
                _parent = value;
            }
        }

        virtual public IICalendar ICalendar
        {
            get { return Calendar; }
            protected set { Calendar = value; }
        }

        virtual public int Line
        {
            get { return _line; }
            set { _line = value; }
        }

        virtual public int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        #endregion       

        #region IServiceProvider Members

        virtual public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        virtual public object GetService(string name)
        {
            return _serviceProvider.GetService(name);            
        }

        virtual public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        virtual public T GetService<T>(string name)
        {
            return _serviceProvider.GetService<T>(name);
        }

        virtual public void SetService(string name, object obj)
        {
            _serviceProvider.SetService(name, obj);
        }

        virtual public void SetService(object obj)
        {
            _serviceProvider.SetService(obj);
        }

        virtual public void RemoveService(Type type)
        {
            _serviceProvider.RemoveService(type);
        }

        virtual public void RemoveService(string name)
        {
            _serviceProvider.RemoveService(name);
        }

        #endregion

        #region IGroupedObject Members

        [field: NonSerialized]
        public event EventHandler<ObjectEventArgs<string, string>> GroupChanged;

        protected void OnGroupChanged(string @old, string @new)
        {
            if (GroupChanged != null)
                GroupChanged(this, new ObjectEventArgs<string, string>(@old, @new));
        }

        virtual public string Group
        {
            get { return Name; }
            set { Name = value; }
        }

        #endregion
    }
}
