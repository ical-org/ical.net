using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that represents a property of the <see cref="iCalendar"/>
    /// itself, or a non-standard (X-) property of an iCalendar component,
    /// as seen with many applications, such as with Apple's iCal.
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
    public class Property : iCalObject
    {
        #region Private Fields

        private string m_value = null;        

        #endregion

        #region Public Properties

        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        #endregion

        #region Constructors

        public Property(ContentLine cl) : base(cl.Parent)
        {
            this.Name = cl.Name;
            this.Value = cl.Value;
            foreach (DictionaryEntry de in cl.Parameters)
                this.Parameters[de.Key] = de.Value;
        }
        public Property(iCalObject parent) : base(parent) { }
        public Property(iCalObject parent, string name) : base(parent, name)
        {
            AddToParent();
        }
        public Property(iCalObject parent, string name, string value) : this(parent, name)
        {
            Value = value;            
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Property)
            {
                Property p = (Property)obj;
                return p.Name.Equals(Name) &&
                    ((p.Value == null && Value == null) ||
                    (p.Value != null && p.Value.Equals(Value)));
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                (Value != null ? Value.GetHashCode() : 0);
        }

        public override iCalObject Copy(iCalObject parent)
        {
            Property p = (Property)base.Copy(parent);
            p.Name = Name;
            p.Value = Value;
            return p;
        }

        #endregion

        #region Public Methods

        public void AddToParent()
        {
            if (Parent != null &&
                Name != null)
            {
                // Remove a previous property, if it exists
                if (Parent.Properties.ContainsKey(Name))
                    Parent.Properties.Remove(Name);

                Parent.Properties[Name] = this;
            }
        }

        #endregion
    }
}
