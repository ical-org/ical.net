using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Validator.RFC2445.DataTypes
{
    public class BinaryValidator :
        IValidator
    {
        public Binary Binary { get; set; }

        #region Constructors

        public BinaryValidator(Binary binary)
        {
            Binary = binary;
        }

        #endregion

        #region IValidator Members

        public IValidationError[] Validate()
        {
            List<IValidationError> errors = new List<IValidationError>();

            if (Binary.Properties.ContainsKey("VALUE"))
            {
                Property p = Binary.Properties["VALUE"] as Property;
                if (!string.Equals(p.Value, "BINARY", StringComparison.InvariantCultureIgnoreCase))
                    errors.Add(new ValidationError("ATTACH: The 'VALUE' property must be 'BINARY' for inline attachments."));
            }

            return errors.ToArray();
        }

        #endregion
    }
}
