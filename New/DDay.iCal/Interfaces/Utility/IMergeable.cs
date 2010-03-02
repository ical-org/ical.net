using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IMergeable
    {
        /// <summary>
        /// Merges this object with another.
        /// </summary>
        void MergeWith(IMergeable obj);
    }
}
