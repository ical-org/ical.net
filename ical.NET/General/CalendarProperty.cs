using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using ical.NET.Collections;
using Ical.Net.ExtensionMethods;
using Ical.Net.Interfaces.General;
using Ical.Net.Utility;

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
        private IList<object> _values;
        private ICalendarParameterCollection _parameters;

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
        public virtual ICalendarParameterCollection Parameters
        {
            get { return _parameters; }
            protected set { _parameters = value; }
        }

        public CalendarProperty()
        {
            Initialize();
        }

        public CalendarProperty(ICalendarProperty other) : this()
        {
            CopyFrom(other);
        }

        public CalendarProperty(string name) : base(name)
        {
            Initialize();
        }

        public CalendarProperty(string name, object value) : base(name)
        {
            Initialize();
            _values.Add(value);
        }

        public CalendarProperty(int line, int col) : base(line, col)
        {
            Initialize();
        }

        private void Initialize()
        {
            _values = new List<object>(128);
            _parameters = new CalendarParameterList(this, true);
            ValueChanged += CalendarProperty_ValueChanged;
        }

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

        private void CalendarProperty_ValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            // Deassociate the old values
            foreach (var removed in e.RemovedValues)
            {
                AssociationUtil.DeassociateItem(removed);
            }

            // Associate the new values with this object.
            foreach (var added in e.AddedValues)
            {
                AssociationUtil.AssociateItem(added, this);
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var p = obj as ICalendarProperty;
            if (p != null)
            {
                // Copy/clone the object if possible (deep copy)
                if (p.Values is ICopyable)
                {
                    SetValue(((ICopyable) p.Values).Copy<object>());
                }
                else
                {
                    SetValue(p.Values);
                }

                // Copy parameters
                foreach (var parm in p.Parameters)
                {
                    this.AddChild(parm.Copy<ICalendarParameter>());
                }
            }
        }

        [field: NonSerialized]
        public event EventHandler<ValueChangedEventArgs<object>> ValueChanged;

        protected void OnValueChanged(object removedValue, object addedValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<object>((IEnumerable<object>) removedValue, (IEnumerable<object>) addedValue));
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
                if (value != null)
                {
                    if (_values != null && _values.Count > 0)
                    {
                        _values[0] = value;
                    }
                    else
                    {
                        _values.Clear();
                        _values.Add(value);
                    }
                }
                else
                {
                    _values = null;
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
                // Our list doesn't contain any values.  Let's add one!
                _values.Add(value);
                OnValueChanged(null, new[] {value});
            }
            else if (value != null)
            {
                // Our list contains values.  Let's set the first value!
                var oldValue = _values[0];
                _values[0] = value;
                OnValueChanged(new[] {oldValue}, new[] {value});
            }
            else
            {
                // Remove all values
                var values = new List<object>(Values);
                _values.Clear();
                OnValueChanged(values, null);
            }
        }

        public virtual void SetValue(IEnumerable<object> values)
        {
            // Remove all previous values
            var removedValues = _values.ToList();
            _values.Clear();
            _values.AddRange(values);
            OnValueChanged(removedValues, values);
        }

        public virtual void AddValue(object value)
        {
            if (value != null)
            {
                _values.Add(value);
                OnValueChanged(null, new[] {value});
            }
        }

        public virtual void RemoveValue(object value)
        {
            if (value != null && _values.Contains(value) && _values.Remove(value))
            {
                OnValueChanged(new[] {value}, null);
            }
        }
    }
}