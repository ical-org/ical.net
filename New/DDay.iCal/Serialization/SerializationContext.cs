using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class SerializationContext :
        ISerializationContext
    {
        #region Static Private Fields

        static private ISerializationContext _Default;

        #endregion

        #region Static Public Properties

        /// <summary>
        /// Gets the Singleton instance of the SerializationContext class.
        /// </summary>
        static public ISerializationContext Default
        {
            get
            {
                if (_Default == null)
                    _Default = new SerializationContext();
                return _Default;
            }
        }

        #endregion

        #region Private Fields

        private bool m_EnsureAccurateLineNumbers = false;
        private ParsingModeType m_ParsingMode = ParsingModeType.Strict;

        #endregion

        #region ISerializationContext Members

        virtual public bool EnsureAccurateLineNumbers
        {
            get { return m_EnsureAccurateLineNumbers; }
            set { m_EnsureAccurateLineNumbers = value; }
        }

        virtual public ParsingModeType ParsingMode
        {
            get { return m_ParsingMode; }
            set { m_ParsingMode = value; }
        }

        #endregion
    }
}
