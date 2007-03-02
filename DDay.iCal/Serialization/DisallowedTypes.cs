using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class DisallowedTypes : Attribute
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

        public DisallowedTypes() : base()
        {
            Types = new List<string>();
        }
        public DisallowedTypes(string type) : this()
        {            
            Types.Add(type);
        }
        public DisallowedTypes(params string[] types) : this()
        {
            Types.AddRange(types);
        }

        #endregion
    }
}
