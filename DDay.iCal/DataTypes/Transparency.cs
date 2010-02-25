using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{    
    /// <summary>
    /// Determines whether or not an event is represented
    /// as "busy"-time, or "free"-time.
    /// </summary>
    public enum Transparency
    {
        Opaque,     /// Opaque (busy) on busy time searches.
        Transparent /// Transparent (not busy) on busy time searches.
    };    
}
