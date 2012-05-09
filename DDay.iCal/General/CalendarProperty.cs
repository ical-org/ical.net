using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics;
using DDay.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents a property of the <see cref="iCalendar"/>
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
#if !SILVERLIGHT
    [Serializable]
#endif
    public class CalendarProperty :
        CalendarObject,
        ICalendarProperty
    {
        #region Private Fields

        private IList<object> _Values;        
        private ICalendarParameterCollection _Parameters;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns a list of parameters that are associated with the iCalendar object.
        /// </summary>
        virtual public ICalendarParameterCollection Parameters
        {
            get { return _Parameters; }
            protected set
            {
                this._Parameters = value;
            }
        }

        #endregion

        #region Constructors

        public CalendarProperty() : base()
        {
            Initialize();
        }

        public CalendarProperty(ICalendarProperty other) : this()
        {
            CopyFrom(other);
        }

        public CalendarProperty(string name)
            : base(name)
        {
            Initialize();
        }

        public CalendarProperty(string name, object value)
            : base(name)
        {
            Initialize();
            _Values.Add(value);
        }

        public CalendarProperty(int line, int col) : base(line, col)
        {
            Initialize();
        }

        private void Initialize()
        {
            _Values = new List<object>();
            _Parameters = new CalendarParameterList(this, true);
            ValueChanged += new EventHandler<ValueChangedEventArgs<object>>(CalendarProperty_ValueChanged);
        }        

        #endregion
               
        #region Public Methods

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        virtual public void AddParameter(string name, string value)
        {            
            CalendarParameter p = new CalendarParameter(name, value);
            Parameters.Add(p);
        }

        /// <summary>
        /// Adds a parameter to the iCalendar object.
        /// </summary>
        virtual public void AddParameter(ICalendarParameter p)
        {
            Parameters.Add(p);
        }

        #endregion

        #region Event Handlers

        void CalendarProperty_ValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            // Deassociate the old values
            foreach (object removed in e.RemovedValues)
                AssociationUtil.DeassociateItem(removed);

            // Associate the new values with this object.
            foreach (object added in e.AddedValues)
                AssociationUtil.AssociateItem(added, this);
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null)
            {
                // Copy/clone the object if possible (deep copy)
                if (p.Values is ICopyable)
                    SetValue(((ICopyable)p.Values).Copy<object>());
                else if (p.Values is ICloneable)
                    SetValue(((ICloneable)p.Values).Clone());
                else
                    SetValue(p.Values);

                // Copy parameters
                foreach (ICalendarParameter parm in p.Parameters)
                    this.AddChild(parm.Copy<ICalendarParameter>());
            }
        }

        #endregion

        #region IValueObject<object> Members

        [field: NonSerialized]
        public event EventHandler<ValueChangedEventArgs<object>> ValueChanged;

        protected void OnValueChanged(object removedValue, object addedValue)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs<object>((IEnumerable<object>)removedValue, (IEnumerable<object>)addedValue));
        }

        virtual public IEnumerable<object> Values
        {
            get 
            {
                return _Values;
            }
        }

        public object Value
        {
            get
            {
                if (_Values != null)
                    return _Values.FirstOrDefault();
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (_Values != null && _Values.Count > 0)
                        _Values[0] = value;
                    else
                    {
                        _Values.Clear();
                        _Values.Add(value);
                    }
                }
                else
                {
                    _Values = null;
                }
            }
        }

        virtual public bool ContainsValue(object value)
        {
            return _Values.Contains(value);
        }

        virtual public int ValueCount
        {
            get
            {
                return _Values != null ? _Values.Count : 0;
            }
        }

        virtual public void SetValue(object value)
        {
            if (_Values.Count == 0)
            {
                // Our list doesn't contain any values.  Let's add one!
                _Values.Add(value);
                OnValueChanged(null, new object[] { value });
            }
            else if (value != null)
            {
                // Our list contains values.  Let's set the first value!
                object oldValue = _Values[0];
                _Values[0] = value;
                OnValueChanged(new object[] { oldValue }, new object[] { value });
            }
            else
            {
                // Remove all values
                List<object> values = new List<object>(Values);
                _Values.Clear();
                OnValueChanged(values, null);
            }
        }

        virtual public void SetValue(IEnumerable<object> values)
        {
            // Remove all previous values
            var removedValues = _Values.ToList();
            _Values.Clear();
            _Values.AddRange(values);
            OnValueChanged(removedValues, values);
        }

        virtual public void AddValue(object value)
        {
            if (value != null)
            {
                _Values.Add(value);
                OnValueChanged(null, new object[] { value });
            }
        }
        
        virtual public void RemoveValue(object value)
        {
            if (value != null &&
                _Values.Contains(value) &&
                _Values.Remove(value))
            {
                OnValueChanged(new object[] { value }, null);
            }
        }

        #endregion
    }
}
