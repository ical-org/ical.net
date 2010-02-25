using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class DefaultValueTypeAttribute : Attribute
    {
        #region Private Fields

        private string m_Type;

        #endregion

        #region Public Properties

        public string Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        #endregion

        #region Constructors

        public DefaultValueTypeAttribute(string type)
        {
            Type = type;
        }

        #endregion
    }
}
