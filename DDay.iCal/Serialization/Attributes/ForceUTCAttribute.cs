using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// Forces a property of <see cref="iCalDateTime"/> type
    /// to serialize in UTC time.
    /// </summary>
    public class ForceUTCAttribute : Attribute
    {
    }
}
