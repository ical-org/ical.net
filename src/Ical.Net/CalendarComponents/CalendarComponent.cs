using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ical.Net.CalendarComponents
{
    /// <summary>
    /// This class is used by the parsing framework for iCalendar components.
    /// Generally, you should not need to use this class directly.
    /// </summary>
    [DebuggerDisplay("Component: {Name}")]
    public class CalendarComponent : CalendarObject, ICalendarComponent
    {
        /// <summary>
        /// Returns a list of properties that are associated with the iCalendar object.
        /// </summary>
        public CalendarPropertyList Properties { get; protected set; }

        public CalendarComponent()
        {
            Initialize();
        }

        public CalendarComponent(string name) : base(name)
        {
            Initialize();
        }

        void Initialize()
        {
            Properties = new CalendarPropertyList(this);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            if (!(obj is ICalendarComponent c))
            {
                return;
            }

            Properties.Clear();
            foreach (var p in c.Properties)
            {
                Properties.Add(p);
            }
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public void AddProperty(string name, string value)
        {
            var p = new CalendarProperty(name, value);
            AddProperty(p);
        }

        /// <summary>
        /// Adds a property to this component.
        /// </summary>
        public void AddProperty(ICalendarProperty p)
        {
            p.Parent = this;
            Properties.Set(p.Name, p.Value);
        }
    }
}