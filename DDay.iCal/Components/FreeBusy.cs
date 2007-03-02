using System;
using System.Data;
using System.Configuration;
using DDay.iCal.Components;

namespace DDay.iCal.Components
{
    /// <summary>
    /// <note>This class has not yet been implemented.</note>
    /// </summary>
    public class FreeBusy : ComponentBase
    {
        public FreeBusy(iCalObject parent) : base(parent)
        {
            this.Name = "VFREEBUSY";
        }

        #region Overrides

        /// <summary>
        /// Returns a typed copy of the FreeBusy object.
        /// </summary>
        /// <returns>A typed copy of the FreeBusy object.</returns>
        public FreeBusy Copy()
        {
            return (FreeBusy)base.Copy();
        }

        #endregion
    }
}
