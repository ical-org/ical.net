using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using ical.NET.Collections;
using Ical.Net.Interfaces.General;

namespace Ical.Net.General
{
    [DebuggerDisplay("{Name}={string.Join(\",\", Values)}")]
    public class CalendarParameter : CalendarObject, ICalendarParameter
    {
        private List<string> _values;

        public CalendarParameter()
        {
            Initialize();
        }

        public CalendarParameter(string name) : base(name)
        {
            Initialize();
        }

        public CalendarParameter(string name, string value) : base(name)
        {
            Initialize();
            AddValue(value);
        }

        public CalendarParameter(string name, IEnumerable<string> values) : base(name)
        {
            Initialize();
            foreach (var v in values)
            {
                AddValue(v);
            }
        }

        private void Initialize()
        {
            _values = new List<string>(128);
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable c)
        {
            base.CopyFrom(c);

            var p = c as ICalendarParameter;
            if (p?.Values != null)
            {
                _values = new List<string>(p.Values);
            }
        }

        public event EventHandler<ValueChangedEventArgs<string>> ValueChanged;

        public virtual IEnumerable<string> Values => _values;

        public virtual bool ContainsValue(string value)
        {
            return _values.Contains(value);
        }

        public virtual int ValueCount => _values?.Count ?? 0;

        public virtual void SetValue(string value)
        {
            if (_values.Count == 0)
            {
                // Our list doesn't contain any values.  Let's add one!
                _values.Add(value);
            }
            else if (value != null)
            {
                // Our list contains values.  Let's set the first value!
                _values[0] = value;
            }
            else
            {
                // Remove all values
                _values.Clear();
            }
        }

        public virtual void SetValue(IEnumerable<string> values)
        {
            // Remove all previous values
            _values.Clear();
            var materializedValues = values.ToList();
            _values.AddRange(materializedValues);
        }

        public virtual void AddValue(string value)
        {
            if (value != null)
            {
                _values.Add(value);
            }
        }

        public virtual void RemoveValue(string value) {}

        public virtual string Value
        {
            get
            {
                return Values?.FirstOrDefault();
            }
            set { SetValue(value); }
        }
    }
}