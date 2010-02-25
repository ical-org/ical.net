using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class EncodableAttribute : System.Attribute
    {
        #region Private Fields

        private string[] m_Values;

        #endregion

        #region Public Properties

        public string[] Values
        {
            get { return m_Values; }
            set { m_Values = value; }
        }

        #endregion

        #region Constructors

        public EncodableAttribute(string name)
        {
            this.Values = name.Split(',');
        }

        #endregion
    }
}
