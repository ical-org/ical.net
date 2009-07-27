using System;
using System.Data;
using System.Configuration;
using DDay.iCal.Components;
using System.Runtime.Serialization;

namespace DDay.iCal.Components
{
    /// <summary>
    /// <note>This class has not yet been implemented.</note>
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "FreeBusy", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#else
    [Serializable]
#endif
    public class FreeBusy : ComponentBase
    {
        public FreeBusy(iCalObject parent) : base(parent)
        {
            this.Name = ComponentBase.FREEBUSY;
        }

        #region Overrides

        /// <summary>
        /// Returns a typed copy of the FreeBusy object.
        /// </summary>
        /// <returns>A typed copy of the FreeBusy object.</returns>
        public new FreeBusy Copy()
        {
            return (FreeBusy)base.Copy();
        }

        #endregion
    }
}
