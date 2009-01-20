using System;
using System.Collections;
using System.Text;
using System.Reflection;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// An abstract class from which all iCalendar data types inherit.
    /// </summary>
    public abstract class iCalDataType : iCalObject
    {
        #region Protected Fields

        protected ContentLine m_ContentLine = null;
        protected object[] m_Attributes = new object[0];

        #endregion

        #region Public Properties

        public object[] Attributes
        {
            get { return m_Attributes; }
            set { m_Attributes = value; }
        }

        #endregion

        #region Overridable Properties & Methods

        virtual public ContentLine ContentLine
        {
            get { return m_ContentLine; }
            set
            {
                m_ContentLine = value;
                if (value != null)
                {
                    // Assign parameters from the content line
                    foreach (Parameter p in value.Parameters)
                        Parameters.Add(p);

                    // Assign the NAME of the object from the content line
                    Name = value.Name;

                    // Assign a parent to the object if it doesn't already have one
                    // NOTE: This assures that some data types will load correctly
                    // by being associated to an iCalendar.
                    if (this.Parent == null)
                        this.Parent = value.Parent;

                    // Parse the content line
                    iCalDataType icdt = Parse(value.Value) as iCalDataType;
                    if (icdt != null)
                    {
                        // Set the parent on the copied object
                        icdt.Parent = value.Parent;

                        CopyFrom(icdt);

                        // FIXME: is there a way to avoid having to do this twice?
                        // Assign a parent to the object if it doesn't already have one
                        // NOTE: this makes sure that some objects have a parent
                        // in case they lost it while parsing
                        if (this.Parent == null)
                            this.Parent = value.Parent;

                        OnLoaded(EventArgs.Empty);
                    }
                }
            }
        }        
        
        virtual public object Parse(string value)
        {
            Type t = GetType();
            object obj = Activator.CreateInstance(t);
            if (!TryParse(value, ref obj))
                throw new ArgumentException(t.Name + ".Parse cannot parse the value '" + value + "' because it is not formatted correctly.");
            return obj;
        }

        virtual public void CopyFrom(object obj)
        {
            iCalDataType icdt = obj as iCalDataType;
            if (icdt != null)
                this.Parent = icdt.Parent;
        }
        
        virtual public bool TryParse(string value, ref object obj) { return false; }        

        virtual public Type ValueType()
        {
            if (Parameters.ContainsKey("VALUE"))
            {
                Parameter p = Parameters["VALUE"];
                if (p.Values.Count > 0)
                {
                    string type = p.Values[0].ToString().ToUpper();
                    switch (type)
                    {
                        case "DATE":
                        case "DATE-TIME":
                            return typeof(iCalDateTime);
                        case "DURATION":
                            return typeof(Duration);
                        default:
                            return null;
                    }
                }
            }

            return GetType();
        }
        
        #endregion

        #region Overrides

        public override iCalObject Copy(iCalObject parent)
        {
            iCalDataType icdt = (iCalDataType)Activator.CreateInstance(GetType());
            icdt.CopyFrom(this);

            // Add parameters
            foreach (Parameter p in Parameters)
                p.Copy(icdt);

            icdt.Parent = parent;
            return icdt;            
        }

        #endregion

        #region Content Validation

        public void CheckRange(string name, ICollection values, int min, int max)
        {
            bool allowZero = (min == 0 || max == 0) ? true : false;
            foreach(int value in values)
                CheckRange(name, value, min, max, allowZero);
        }
        public void CheckRange(string name, int value, int min, int max)
        {
            CheckRange(name, value, min, max, (min == 0 || max == 0) ? true : false);
        }
        public void CheckRange(string name, int value, int min, int max, bool allowZero)
        {
            if (value != int.MinValue && (value < min || value > max || (!allowZero && value == 0)))
                throw new ArgumentException(name + " value " + value + " is out of range. Valid values are between " + min + " and " + max + (allowZero ? "" : ", excluding zero (0)") + ".");
        }

        public void CheckMutuallyExclusive(string name1, string name2, object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
                return;
            else
            {
                bool has1 = false,
                    has2 = false;

                Type t1 = obj1.GetType(),
                    t2 = obj2.GetType();

                FieldInfo fi1 = t1.GetField("MinValue");
                FieldInfo fi2 = t1.GetField("MinValue");

                has1 = fi1 == null || !obj1.Equals(fi1.GetValue(null));
                has2 = fi2 == null || !obj2.Equals(fi2.GetValue(null));
                if (has1 && has2)
                    throw new ArgumentException("Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive.");
            }
        }

        #endregion        
    }
}
