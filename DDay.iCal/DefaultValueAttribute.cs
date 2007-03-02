using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class DefaultValueAttribute : Attribute
    {
        #region Private Fields

        private object m_Value;

        #endregion

        #region Public Properties

        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        #endregion

        #region Constructors

        public DefaultValueAttribute(object value)
        {
            Value = value;
        }

        #endregion
    }
}
