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
    public class CalendarObject : CalendarObjectBase, ICalendarObject
    {
        private ICalendarObject _parent;
        private ICalendarObjectList<ICalendarObject> _children;
        private ServiceProvider _serviceProvider;
        private string _name;

        private int _line;
        private int _column;

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
            //ToDo: I'm fairly certain this is ONLY used for null checking. If so, maybe it can just be a bool? CalendarObjectList is an empty object, and
            //ToDo: its constructor parameter is ignored
            _children = new CalendarObjectList(this);
            _serviceProvider = new ServiceProvider();

            _children.ItemAdded += _Children_ItemAdded;
            _children.ItemRemoved += _Children_ItemRemoved;
        }

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

        protected virtual void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        protected virtual void OnDeserialized(StreamingContext context) {}

        private void _Children_ItemRemoved(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            e.First.Parent = null;
        }

        private void _Children_ItemAdded(object sender, ObjectEventArgs<ICalendarObject, int> e)
        {
            e.First.Parent = this;
        }

        protected bool Equals(CalendarObject other)
        {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CalendarObject) obj);
        }

        public override int GetHashCode()
        {
            return _name?.GetHashCode() ?? 0;
        }

        public override void CopyFrom(ICopyable c)
        {
            var obj = c as ICalendarObject;
            if (obj != null)
            {
                // Copy the name and basic information
                Name = obj.Name;
                Parent = obj.Parent;
                Line = obj.Line;
                Column = obj.Column;

                // Add each child
                Children.Clear();
                foreach (var child in obj.Children)
                {
                    this.AddChild(child.Copy<ICalendarObject>());
                }
            }
        }

        /// <summary>
        /// Returns the parent iCalObject that owns this one.
        /// </summary>
        public virtual ICalendarObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// A collection of iCalObjects that are children of the current object.
        /// </summary>
        public virtual ICalendarObjectList<ICalendarObject> Children => _children;

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
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (!Equals(_name, value))
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
        public virtual ICalendar Calendar
        {
            get
            {
                ICalendarObject obj = this;
                while (!(obj is ICalendar) && obj.Parent != null)
                {
                    obj = obj.Parent;
                }

                if (obj is ICalendar)
                {
                    return (ICalendar) obj;
                }
                return null;
            }
            protected set { _parent = value; }
        }

        public virtual ICalendar ICalendar
        {
            get { return Calendar; }
            protected set { Calendar = value; }
        }

        public virtual int Line
        {
            get { return _line; }
            set { _line = value; }
        }

        public virtual int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public virtual object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public virtual object GetService(string name)
        {
            return _serviceProvider.GetService(name);
        }

        public virtual T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        public virtual T GetService<T>(string name)
        {
            return _serviceProvider.GetService<T>(name);
        }

        public virtual void SetService(string name, object obj)
        {
            _serviceProvider.SetService(name, obj);
        }

        public virtual void SetService(object obj)
        {
            _serviceProvider.SetService(obj);
        }

        public virtual void RemoveService(Type type)
        {
            _serviceProvider.RemoveService(type);
        }

        public virtual void RemoveService(string name)
        {
            _serviceProvider.RemoveService(name);
        }

        public event EventHandler<ObjectEventArgs<string, string>> GroupChanged;

        protected void OnGroupChanged(string old, string @new)
        {
            GroupChanged?.Invoke(this, new ObjectEventArgs<string, string>(old, @new));
        }

        public virtual string Group
        {
            get { return Name; }
            set { Name = value; }
        }
    }
}