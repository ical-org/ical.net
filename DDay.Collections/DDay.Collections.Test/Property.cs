using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.Collections.Test
{
    public class Property :
        IGroupedObject<string>,
        IValueObject<string>
    {
        public Property()
        {
            _InternalValues = new List<string>();
        }

        #region IKeyedObject<string> Members

        public event EventHandler<ObjectEventArgs<string, string>> GroupChanged;

        protected void OnKeyChanged(string oldKey, string newKey)
        {
            if (GroupChanged != null)
                GroupChanged(this, new ObjectEventArgs<string, string>(oldKey, newKey));
        }

        string _Key;
        virtual public string Group
        {
            get { return _Key; }
            set
            {
                if (!object.Equals(_Key, value))
                {
                    string oldKey = _Key;
                    _Key = value;
                    OnKeyChanged(oldKey, value);
                }
            }
        }

        #endregion

        #region IValueObject<string> Members

        public event EventHandler<ValueChangedEventArgs<string>> ValueChanged;

        protected void OnValueChanged(IEnumerable<string> removedValues, IEnumerable<string> addedValues)
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs<string>(removedValues, addedValues));
        }

        List<string> _InternalValues;
        virtual public IEnumerable<string> Values { get { return _InternalValues; } }

        virtual public bool ContainsValue(string value)
        {
            if (_InternalValues != null)
                return _InternalValues.Contains(value);
            return false;
        }

        virtual public void SetValue(string value)
        {
            var removedValues = Values;
            _InternalValues.Clear();
            if (value != null)
            {
                _InternalValues.Add(value);
            }
            OnValueChanged(removedValues, _InternalValues);
        }

        virtual public void SetValue(IEnumerable<string> values)
        {
            var removedValues = Values;
            _InternalValues = new List<string>(values);
            OnValueChanged(removedValues, _InternalValues);
        }

        virtual public void AddValue(string value)
        {
            _InternalValues.Add(value);
            OnValueChanged(null, new string[] { value });
        }

        virtual public void RemoveValue(string value)
        {
            _InternalValues.Remove(value);
            OnValueChanged(new string[] { value }, null);
        }

        virtual public int ValueCount
        {
            get { return _InternalValues.Count; }
        }

        #endregion
    }
}
