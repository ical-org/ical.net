using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.DataTypes
{
    public class FreeBusyEntry : Period, IFreeBusyEntry
    {
        public FreeBusyEntry()
        {
            Initialize();
        }

        public FreeBusyEntry(IPeriod period, FreeBusyStatus status)
        {
            Initialize();
            CopyFrom(period);
            Status = status;
        }

        private void Initialize()
        {
            Status = FreeBusyStatus.Busy;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var fb = obj as IFreeBusyEntry;
            if (fb != null)
            {
                Status = fb.Status;
            }
        }

        public virtual FreeBusyStatus Status { get; set; }
    }
}