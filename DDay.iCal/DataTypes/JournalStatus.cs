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
        DRAFT,      // Indicates journal is draft.
        FINAL,      // Indicates journal is final.
        CANCELLED   // Indicates journal is removed.
    };
}
