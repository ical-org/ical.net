using System;
using System.Data;
using System.Configuration;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// <note>This class has not yet been implemented.</note>
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "FreeBusy", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class FreeBusy : Component
    {
        public FreeBusy()
        {
            this.Name = ComponentFactory.FREEBUSY;
        }
    }
}
