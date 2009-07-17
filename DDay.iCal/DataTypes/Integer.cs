using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents in iCalendar integer
    /// </summary>
    [DebuggerDisplay("{Value}")]
    [Encodable("BASE64,8BIT,7BIT")]
#if SILVERLIGHT
    [DataContract(Name = "Integer", Namespace="http://www.ddaysoftware.com/dday.ical/datatypes/2009/07/")]
#else
    [Serializable]
#endif
    public class Integer : EncodableDataType
    {
        #region Private Fields

        private int m_Value;

        #endregion

        #region Public Properties

        public new int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        #endregion

        #region Constructors

        public Integer() { }
        public Integer(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }
        public Integer(int value)
            : this()
        {
            this.Value = value;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Integer)
                return Value.Equals(((Integer)obj).Value);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is Integer)
            {
                Integer i = (Integer)obj;
                Value = i.Value;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref object obj)
        {
            if (!base.TryParse(value, ref obj))
                return false;

            EncodableDataType ecd = (EncodableDataType)obj;
            if (ecd.Value != null)
                value = ecd.Value;

            int i;
            bool retVal = Int32.TryParse(value, out i);
            ((Integer)obj).Value = i;
            return retVal;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region Operators

        static public implicit operator int(Integer i)
        {
            return i != null ? i.Value : default(int);            
        }

        static public implicit operator Integer(int i)
        {
            return new Integer(i);
        }

        #endregion
    }
}
