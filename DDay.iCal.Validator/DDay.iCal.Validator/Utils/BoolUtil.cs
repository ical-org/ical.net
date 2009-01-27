using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    static public class BoolUtil
    {
        static public bool IsTrue(bool? value)
        {
            return (value != null && value.HasValue && value.Value);
        }
    }
}
