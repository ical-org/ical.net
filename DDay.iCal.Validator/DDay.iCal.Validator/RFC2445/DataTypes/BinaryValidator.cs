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

        public IValidationResult[] Validate()
        {
            // FIXME: implement this
            /*IValidationResult result = new ValidationResult(
            List<IValidationResult> results = new List<IValidationResult>();

            if (Binary.Properties.ContainsKey("VALUE"))
            {
                Property p = Binary.Properties["VALUE"] as Property;
                if (!string.Equals(p.Value, "BINARY", StringComparison.InvariantCultureIgnoreCase))
                    results.Add(new ValidationError("ATTACH: The 'VALUE' property must be 'BINARY' for inline attachments."));
            }

            return results.ToArray();*/
            return new IValidationResult[0];
        }

        #endregion
    }
}
