using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.DataTypes
{
    public class FreeBusyEntry :
        Period,
        IFreeBusyEntry
    {
        #region Private Fields

        #endregion

        #region Constructors

        public FreeBusyEntry() : base() { Initialize(); }
        public FreeBusyEntry(IPeriod period, FreeBusyStatus status) : base()
        {
            Initialize();
            CopyFrom(period);
            Status = status;
        }

        void Initialize()
        {
            Status = FreeBusyStatus.Busy;
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var fb = obj as IFreeBusyEntry;
            if (fb != null)
            {
                Status = fb.Status;
            }
        }

        #endregion

        #region IFreeBusyEntry Members

        public virtual FreeBusyStatus Status { get; set; }

        #endregion
    }
}
