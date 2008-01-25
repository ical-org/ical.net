using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents an RFC 2445 floating-point decimal value.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    [Encodable("BASE64,8BIT,7BIT")]
    public class Float : EncodableDataType
    {
        #region Private Fields

        private double m_Value;

        #endregion

        #region Public Properties

        public new double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        #endregion

        #region Constructors

        public Float() { }
        public Float(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }
        public Float(double value)
        {
            Value = value;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Float)
                return Value.Equals(((Float)obj).Value);    
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is double)
            {
                Float i = (Float)obj;
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

            double i;
            bool retVal = double.TryParse(value, out i);
            ((Float)obj).Value = i;
            return retVal;
        }

        public override string ToString()
        {
            return Value.ToString("0.######");
        }

        #endregion

        #region Operators

        static public implicit operator double(Float i)
        {
            return i.Value;
        }

        static public implicit operator Float(double d)
        {
            return new Float(d);
        }

        #endregion
    }
}
