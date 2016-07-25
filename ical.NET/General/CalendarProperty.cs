using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    /// <summary>
    /// A class that represents a property of the <see cref="Calendar"/>
    /// itself or one of its components.  It can also represent non-standard
    /// (X-) properties of an iCalendar component, as seen with many
    /// applications, such as with Apple's iCal.
    /// X-WR-CALNAME:US Holidays
    /// </summary>
    /// <remarks>
    /// Currently, the "known" properties for an iCalendar are as
    /// follows:
    /// <list type="bullet">
    ///     <item>ProdID</item>
    ///     <item>Version</item>
    ///     <item>CalScale</item>
    ///     <item>Method</item>
    /// </list>
    /// There may be other, custom X-properties applied to the calendar,
    /// and X-properties may be applied to calendar components.
    /// </remarks>
    [DebuggerDisplay("{Name}:{Value}")]
    public class CalendarProperty : CalendarObject, ICalendarProperty
    {
        private IList<object> _values = new List<object>(128);

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
        public virtual ICalendarParameterCollection Parameters { get; protected set; } = new CalendarParameterList();

        public CalendarProperty() {}

        public CalendarProperty(string name) : base(name) {}

        public CalendarProperty(string name, object value) : base(name)
        {
            _values.Add(value);
        }

        public CalendarProperty(int line, int col) : base(line, col) {}

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        public virtual void AddParameter(string name, string value)
        {
            var p = new CalendarParameter(name, value);
            Parameters.Add(p);
        }

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        public virtual void AddParameter(ICalendarParameter p)
        {
            Parameters.Add(p);
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var p = obj as ICalendarProperty;
            if (p == null)
            {
                return;
            }

            SetValue(p.Values);
        }

        public virtual IEnumerable<object> Values => _values;

        public object Value
        {
            get
            {
                return _values?.FirstOrDefault();
            }
            set
            {
                if (value == null)
                {
                    _values = null;
                    return;
                }

                if (_values != null && _values.Count > 0)
                {
                    _values[0] = value;
                }
                else
                {
                    _values?.Clear();
                    _values?.Add(value);
                }
            }
        }

        public virtual bool ContainsValue(object value)
        {
            return _values.Contains(value);
        }

        public virtual int ValueCount => _values?.Count ?? 0;

        public virtual void SetValue(object value)
        {
            if (_values.Count == 0)
            {
                _values.Add(value);
            }
            else if (value != null)
            {
                // Our list contains values.  Let's set the first value!
                _values[0] = value;
            }
            else
            {
                _values.Clear();
            }
        }

        public virtual void SetValue(IEnumerable<object> values)
        {
            // Remove all previous values
            _values.Clear();
            _values.AddRange(values);
        }

        public virtual void AddValue(object value)
        {
            if (value == null)
            {
                return;
            }

            _values.Add(value);
        }

        public virtual void RemoveValue(object value)
        {
            if (value == null)
            {
                return;
            }
            _values.Remove(value);
        }
    }
}