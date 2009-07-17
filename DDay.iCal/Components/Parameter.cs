using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// A class that provides additional information about a <see cref="ContentLine"/>.
    /// </summary>
    /// <remarks>
    /// <example>
    /// For example, a DTSTART line may look like this: <c>DTSTART;VALUE=DATE:20060116</c>.  
    /// The <c>VALUE=DATE</c> portion is a <see cref="Parameter"/> of the DTSTART value.
    /// </example>
    /// </remarks>
#if SILVERLIGHT
    [DataContract(Name = "Parameter", Namespace="http://www.ddaysoftware.com/dday.ical/components/2009/07/")]
#else
    [Serializable]
#endif
    public class Parameter : 
        iCalObject,
        IKeyedObject<string>
    {
        #region Private Fields

        private List<string> m_Values = new List<string>();
        
        #endregion

        #region Public Properties

        public List<string> Values
        {
            get { return m_Values; }
            set { m_Values = value; }
        }

        #endregion

        #region Constructors

        public Parameter(string name)
            : base()
        {
            Name = name;
        }
        public Parameter(string name, string value) : this(name)
        {            
            Values.Add(value);
        }
        public Parameter(iCalObject parent) : base(parent) { }
        public Parameter(iCalObject parent, string name) : base(parent, name)
        {
            AddToParent();
        }

        #endregion

        #region Public Methods

        public void CopyFrom(object obj)
        {
            if (obj is Parameter)
            {
                Values.Clear();

                Parameter p = (Parameter)obj;
                foreach (string value in p.Values)
                    Values.Add(value);
            }
        }

        public void AddToParent()
        {
            if (Parent != null &&
                Name != null)
                Parent.Parameters.Add(this);
        }

        #endregion

        #region Overrides

        public override iCalObject Copy(iCalObject parent)
        {
            Parameter p = (Parameter)base.Copy(parent);
            foreach (string s in Values)
                p.Values.Add(s);
            return p;
        }

        public override bool Equals(object obj)
        {
            Parameter p = obj as Parameter;
            if (p != null)
                return object.Equals(p.Name, Name);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Name != null)
                return Name.GetHashCode();
            else return base.GetHashCode();
        }
        
        #endregion

        #region IKeyedObject Members

        public string Key
        {
            get { return Name; }
        }

        #endregion
    }
}
