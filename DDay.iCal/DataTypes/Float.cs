using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an RFC 5545 floating-point decimal value.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    [Encodable("BASE64,8BIT,7BIT")]
#if DATACONTRACT
    [DataContract(Name = "Float", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Float : EncodableDataType
    {
        #region Private Fields

        private double m_Value;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
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

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is Float)
            {
                Float i = (Float)obj;
                Value = i.Value;
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarDataType obj)
        {
            if (!base.TryParse(value, ref obj))
                return false;

            // Retrieve the encoded value
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
            return i != null ? i.Value : default(double);            
        }

        static public implicit operator Float(double d)
        {
            return new Float(d);
        }

        #endregion
    }
}
