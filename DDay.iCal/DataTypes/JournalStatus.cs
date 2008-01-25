using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Status codes available to a <see cref="Journal"/> entry.
    /// </summary>    
    public enum JournalStatus
    {
        Draft,      // Indicates journal is draft.
        Final,      // Indicates journal is final.
        Cancelled   // Indicates journal is removed.
    };
}
