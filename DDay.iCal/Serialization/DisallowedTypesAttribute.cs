using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// Lists the iCalendar types that are not
    /// allowed on the property.
    /// <example>
    /// For example, if "DATE" were a disallowed type on a
    /// iCalDateTime object, it would always be serialized as
    /// DATE-TIME.
    /// </example>
    /// </summary>
    public class DisallowedTypesAttribute : Attribute
    {
        #region Private Fields

        private List<string> _Types;

        #endregion

        #region Public Properties

        public List<string> Types
        {
            get { return _Types; }
            set { _Types = value; }
        }

        #endregion

        #region Constructors

        public DisallowedTypesAttribute() : base()
        {
            Types = new List<string>();
        }
        public DisallowedTypesAttribute(string type) : this()
        {            
            Types.Add(type);
        }
        public DisallowedTypesAttribute(params string[] types)
            : this()
        {
            Types.AddRange(types);
        }

        #endregion
    }
}
