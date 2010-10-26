using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IFreeBusy :
        IUniqueComponent,
        IMergeable
    {
        IList<IFreeBusyEntry> Entries { get; set; }
        IDateTime DTStart { get; set; }
        IDateTime DTEnd { get; set; }
        IDateTime Start { get; set; }
        IDateTime End { get; set; }

        FreeBusyStatus GetFreeBusyStatus(IPeriod period);
        FreeBusyStatus GetFreeBusyStatus(IDateTime dt);
    }
}
