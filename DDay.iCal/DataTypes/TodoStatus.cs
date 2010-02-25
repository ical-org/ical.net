using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    /// <summary>
    /// Status codes available to a <see cref="Todo"/> item.
    /// </summary>    
    public enum TodoStatus
    {        
        Needs_Action,
        Completed,
        In_Process,
        Cancelled
    };
}
